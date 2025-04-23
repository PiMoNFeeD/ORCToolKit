using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ORCToolKit
{
    internal class TreeViewFileExplorer: TreeView
    {
        private ImageList FileImageList = new ImageList();

        public TreeViewFileExplorer()
        {
            this.BeforeExpand += TreeViewFileExplorer_BeforeExpand;

            FileImageList.Images.Add(SystemIcons.GetStockIcon(StockIconId.Folder, StockIconOptions.SmallIcon)); // make first icon a folder icon, to be used by directories by default
            FileImageList.Images.Add(SystemIcons.GetStockIcon(StockIconId.DocumentNoAssociation, StockIconOptions.SmallIcon)); // used for unknown file types
            this.ImageList = FileImageList;
        }

        public void LoadFolder(string FolderPath)
        {
            if (string.IsNullOrEmpty(FolderPath))
                return;

            Nodes.Clear();
            DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath);

            // fill directories
            foreach (DirectoryInfo SubFolderInfo in FolderInfo.EnumerateDirectories())
                Nodes.Add(CreateFolderNode(SubFolderInfo));

            // fill paths
            foreach (FileInfo SubFileInfo in FolderInfo.EnumerateFiles())
                Nodes.Add(CreateFileNode(SubFileInfo));
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_SMALLICON = 0x000000001;  // Optional: Use SHGFI_LARGEICON for large icons
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        private void EnsureIconForExtensionExists(string FileExtension)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON;

            SHGetFileInfo(FileExtension, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            if (shinfo.hIcon != IntPtr.Zero)
                FileImageList.Images.Add(FileExtension, (Icon)Icon.FromHandle(shinfo.hIcon));
        }

        private TreeNode CreateFolderNode(DirectoryInfo FolderInfo)
        {
            TreeNode DirectoryNode = new TreeNode()
            {
                Text = FolderInfo.Name,
                Tag = FolderInfo
            };

            // if this folder has any files or subfolders, add a dummy node to be able to expand it
            if (FolderInfo.EnumerateFiles().Any() || FolderInfo.EnumerateDirectories().Any())
                DirectoryNode.Nodes.Add(new TreeNode());

            return DirectoryNode;
        }

        private TreeNode CreateFileNode(FileInfo FileInfo)
        {
            TreeNode FileNode = new TreeNode()
            {
                Text = FileInfo.Name,
                Tag = FileInfo,
                ImageIndex = 1, // unknown file type
                SelectedImageIndex = 1 // unknown file type
            };

            // load file icon
            if (!string.IsNullOrEmpty(FileInfo.Extension))
            {
                if (FileInfo.Extension == ".exe")
                {
                    // load an icon from that executable
                    FileImageList.Images.Add(FileInfo.FullName, Icon.ExtractAssociatedIcon(FileInfo.FullName));
                    FileNode.ImageIndex = FileNode.SelectedImageIndex = FileImageList.Images.IndexOfKey(FileInfo.FullName);
                }
                else
                {
                    EnsureIconForExtensionExists(FileInfo.Extension);
                    FileNode.ImageIndex = FileNode.SelectedImageIndex = FileImageList.Images.IndexOfKey(FileInfo.Extension);
                }
            }

            return FileNode;
        }

        private void TreeViewFileExplorer_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
                return;

            // if we already filled a list of files in this directory, bail
            if (e.Node.FirstNode != null && e.Node.FirstNode.Tag != null)
                return;

            e.Node.Nodes.Clear();

            DirectoryInfo ParentDirectoryInfo = (DirectoryInfo)e.Node.Tag;

            // fill directories
            foreach (DirectoryInfo DirectoryInfo in ParentDirectoryInfo.EnumerateDirectories())
                e.Node.Nodes.Add(CreateFolderNode(DirectoryInfo));

            // fill paths
            foreach (FileInfo FileInfo in ParentDirectoryInfo.EnumerateFiles())
                e.Node.Nodes.Add(CreateFileNode(FileInfo));
        }
    }
}
