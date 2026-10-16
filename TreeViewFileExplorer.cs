using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ORCToolKit
{
	internal class ExplorerTreeNodeComparer: IComparer, IComparer<TreeNode>
	{
		[DllImport( "Shlwapi.dll", CharSet = CharSet.Auto )]
		private static extern Int32 StrCmpLogicalW( string psz1, string psz2 );

		public Int32 Compare( object? x, object? y )
		{
			if ( x is not TreeNode || y is not TreeNode )
				return Comparer.Default.Compare( x, y );

			return CompareNodes( (TreeNode)x, (TreeNode)y );
		}

		public Int32 Compare( TreeNode? x, TreeNode? y )
		{
			if ( x == null || y == null )
				return Comparer.Default.Compare( x, y );

			return CompareNodes( x, y );
		}

		private Int32 CompareNodes( TreeNode node1, TreeNode node2 )
		{
			Debug.Assert( node1.Tag != null && node2.Tag != null );

			if ( node1.Tag.GetType() != node2.Tag.GetType() )
			{
				// put directories above everything
				if ( node1.Tag is DirectoryInfo )
					return -1;
				if ( node2.Tag is DirectoryInfo )
					return 1;
			}

			return StrCmpLogicalW( node1.Text, node2.Text );
		}
	}

	internal class TreeViewFileExplorer: TreeView
	{
		private ImageList _fileImageList = new ImageList();
		private List<TaggedFileSystemWatcher> _fileSystemWatchers = new List<TaggedFileSystemWatcher>();

		public TreeViewFileExplorer()
		{
			this.BeforeExpand += TreeViewFileExplorer_BeforeExpand;

			_fileImageList.Images.Add( SystemIcons.GetStockIcon( StockIconId.Folder, StockIconOptions.SmallIcon ) ); // make first icon a folder icon, to be used by directories by default
			_fileImageList.Images.Add( SystemIcons.GetStockIcon( StockIconId.DocumentNoAssociation, StockIconOptions.SmallIcon ) ); // used for unknown file types
			this.ImageList = _fileImageList;
			this.TreeViewNodeSorter = (IComparer)new ExplorerTreeNodeComparer();
		}

		public void LoadFolder( string folderPath )
		{
			if ( string.IsNullOrEmpty( folderPath ) )
				return;

			BeginUpdate();

			Nodes.Clear();
			_fileSystemWatchers.Clear();

			DirectoryInfo folderInfo = new DirectoryInfo( folderPath );
			AddFolderWatcher( folderInfo );

			// fill directories
			foreach ( DirectoryInfo subFolderInfo in folderInfo.EnumerateDirectories() )
				Nodes.Add( CreateFolderNode( subFolderInfo ) );

			// fill paths
			foreach ( FileInfo subFileInfo in folderInfo.EnumerateFiles() )
				Nodes.Add( CreateFileNode( subFileInfo ) );

			Sort();

			EndUpdate();
		}

		[DllImport( "shell32.dll", CharSet = CharSet.Auto )]
		private static extern IntPtr SHGetFileInfo(
			string pszPath,
			uint dwFileAttributes,
			ref SHFILEINFO psfi,
			uint cbFileInfo,
			uint uFlags );

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 260 )]
			public string szDisplayName;
			[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 80 )]
			public string szTypeName;
		}

		private const uint SHGFI_ICON = 0x000000100;
		private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
		private const uint SHGFI_SMALLICON = 0x000000001;  // Optional: Use SHGFI_LARGEICON for large icons
		private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

		private void EnsureIconForExtensionExists( string fileExtension )
		{
			SHFILEINFO shinfo = new SHFILEINFO();
			uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON;

			SHGetFileInfo( fileExtension, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf( shinfo ), flags );

			if ( shinfo.hIcon != IntPtr.Zero )
				_fileImageList.Images.Add( fileExtension, (Icon)Icon.FromHandle( shinfo.hIcon ) );
		}

		private TreeNode CreateFolderNode( DirectoryInfo folderInfo )
		{
			TreeNode directoryNode = new TreeNode()
			{
				Text = folderInfo.Name,
				Tag = folderInfo
			};

			// if this folder has any files or subfolders, add a dummy node to be able to expand it
			if ( folderInfo.EnumerateFiles().Any() || folderInfo.EnumerateDirectories().Any() )
				directoryNode.Nodes.Add( new TreeNode() );

			return directoryNode;
		}

		private TreeNode CreateFileNode( FileInfo fileInfo )
		{
			TreeNode fileNode = new TreeNode()
			{
				Text = fileInfo.Name,
				Tag = fileInfo,
				ImageIndex = 1, // unknown file type
				SelectedImageIndex = 1 // unknown file type
			};

			// load file icon
			if ( !string.IsNullOrEmpty( fileInfo.Extension ) )
			{
				if ( fileInfo.Extension == ".exe" )
				{
					// load an icon from that executable
					_fileImageList.Images.Add( fileInfo.FullName, Icon.ExtractAssociatedIcon( fileInfo.FullName ) );
					fileNode.ImageIndex = fileNode.SelectedImageIndex = _fileImageList.Images.IndexOfKey( fileInfo.FullName );
				}
				else
				{
					EnsureIconForExtensionExists( fileInfo.Extension );
					fileNode.ImageIndex = fileNode.SelectedImageIndex = _fileImageList.Images.IndexOfKey( fileInfo.Extension );
				}
			}

			return fileNode;
		}

		private void AddFolderWatcher( DirectoryInfo folderInfo, TreeNode? parentNode = null )
		{
			TaggedFileSystemWatcher watcher = new TaggedFileSystemWatcher( folderInfo.FullName );
			watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = false;
			watcher.Tag = parentNode;
			watcher.Created += FileSystemWatcher_Created;
			watcher.Deleted += FileSystemWatcher_Deleted;
			watcher.Renamed += FileSystemWatcher_Renamed;
			_fileSystemWatchers.Add( watcher );
		}

		private void TreeViewFileExplorer_BeforeExpand( object? sender, TreeViewCancelEventArgs e )
		{
			if ( e.Node == null )
				return;

			// if we already filled a list of files in this directory, bail
			if ( e.Node.FirstNode != null && e.Node.FirstNode.Tag != null )
				return;

			e.Node.Nodes.Clear();

			DirectoryInfo parentDirectoryInfo = (DirectoryInfo)e.Node.Tag;
			AddFolderWatcher( parentDirectoryInfo, e.Node ); // don't watch a folder until we open it for the first time

			// fill directories
			List<TreeNode> newNodes = new List<TreeNode>();
			foreach ( DirectoryInfo directoryInfo in parentDirectoryInfo.EnumerateDirectories() )
				newNodes.Add( CreateFolderNode( directoryInfo ) );

			// fill paths
			foreach ( FileInfo fileInfo in parentDirectoryInfo.EnumerateFiles() )
				newNodes.Add( CreateFileNode( fileInfo ) );

			newNodes.Sort( new ExplorerTreeNodeComparer() );
			e.Node.Nodes.AddRange( newNodes.ToArray() );
		}

		private void FileSystemWatcher_Created( object sender, FileSystemEventArgs e )
		{
			TaggedFileSystemWatcher watcher = (TaggedFileSystemWatcher)sender;

			// HACK: for some reason, when saving a file, this gets triggered BEFORE a file is actually created??
			// causes a crash down below if I don't do it...
			if ( !File.Exists( e.FullPath ) && !Directory.Exists( e.FullPath ) )
				return;

			TreeNode? parentNode = (TreeNode?)watcher.Tag;
			if ( parentNode != null )
			{
				if ( File.GetAttributes( e.FullPath ).HasFlag( FileAttributes.Directory ) )
				{
					BeginInvoke( delegate
					{
						// need to move all child nodes to a list, sort it, and re-add them back, because stupid c# does not allow sorting
						// child nodes only, you can only sort the entire fucking tree at once!
						BeginUpdate();

						TreeNode[] newNodes = new TreeNode[parentNode.Nodes.Count + 1];
						parentNode.Nodes.CopyTo( newNodes, 0 );
						parentNode.Nodes.Clear();

						newNodes[newNodes.Count() - 1] = CreateFolderNode( new DirectoryInfo( e.FullPath ) );

						Array.Sort( newNodes, new ExplorerTreeNodeComparer() );
						parentNode.Nodes.AddRange( newNodes );

						EndUpdate();
					} );
				}
				else
				{
					BeginInvoke( delegate
					{
						BeginUpdate();

						TreeNode[] newNodes = new TreeNode[parentNode.Nodes.Count + 1];
						parentNode.Nodes.CopyTo( newNodes, 0 );
						parentNode.Nodes.Clear();

						newNodes[newNodes.Count() - 1] = CreateFileNode( new FileInfo( e.FullPath ) );

						Array.Sort( newNodes, new ExplorerTreeNodeComparer() );
						parentNode.Nodes.AddRange( newNodes );

						EndUpdate();
					} );
				}
			}
			else
			{
				if ( File.GetAttributes( e.FullPath ).HasFlag( FileAttributes.Directory ) )
				{
					BeginInvoke( delegate
					{
						BeginUpdate();

						TreeNode[] newNodes = new TreeNode[Nodes.Count + 1];
						Nodes.CopyTo( newNodes, 0 );
						Nodes.Clear();

						newNodes[newNodes.Count() - 1] = CreateFolderNode( new DirectoryInfo( e.FullPath ) );

						Array.Sort( newNodes, new ExplorerTreeNodeComparer() );
						Nodes.AddRange( newNodes );

						EndUpdate();
					} );
				}
				else
				{
					BeginInvoke( delegate
					{
						BeginUpdate();

						TreeNode[] newNodes = new TreeNode[Nodes.Count + 1];
						Nodes.CopyTo( newNodes, 0 );
						Nodes.Clear();

						newNodes[newNodes.Count() - 1] = CreateFileNode( new FileInfo( e.FullPath ) );

						Array.Sort( newNodes, new ExplorerTreeNodeComparer() );
						Nodes.AddRange( newNodes );

						EndUpdate();
					} );
				}
			}
		}

		private void FileSystemWatcher_Deleted( object sender, FileSystemEventArgs e )
		{
			TaggedFileSystemWatcher watcher = (TaggedFileSystemWatcher)sender;

			TreeNode? parentNode = (TreeNode?)watcher.Tag;
			foreach ( TreeNode fileNode in (parentNode != null) ? parentNode.Nodes : Nodes )
			{
				FileSystemInfo fileInfo = (FileSystemInfo)fileNode.Tag;
				if ( fileInfo.FullName == e.FullPath )
				{
					BeginInvoke( delegate
					{
						fileNode.Remove();
					} );
					break;
				}
			}
		}

		private void FileSystemWatcher_Renamed( object sender, RenamedEventArgs e )
		{
			TaggedFileSystemWatcher watcher = (TaggedFileSystemWatcher)sender;

			TreeNode? parentNode = (TreeNode?)watcher.Tag;
			foreach ( TreeNode fileNode in (parentNode != null) ? parentNode.Nodes : Nodes )
			{
				FileSystemInfo fileInfo = (FileSystemInfo)fileNode.Tag;
				if ( fileInfo.FullName == e.OldFullPath )
				{
					BeginInvoke( delegate
					{
						BeginUpdate();

						fileNode.Tag = fileInfo is FileInfo ? new FileInfo( e.FullPath ) : new DirectoryInfo( e.FullPath );
						fileNode.Name = e.FullPath;
						fileNode.Text = e.Name;
						Sort();

						EndUpdate();
					} );
					break;
				}
			}
		}
	}
}
