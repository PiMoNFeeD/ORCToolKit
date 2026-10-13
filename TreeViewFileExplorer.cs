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

		private Int32 CompareNodes( TreeNode Node1, TreeNode Node2 )
		{
			Debug.Assert( Node1.Tag != null && Node2.Tag != null );

			if ( Node1.Tag.GetType() != Node2.Tag.GetType() )
			{
				// put directories above everything
				if ( Node1.Tag is DirectoryInfo )
					return -1;
				if ( Node2.Tag is DirectoryInfo )
					return 1;
			}

			return StrCmpLogicalW( Node1.Text, Node2.Text );
		}
	}

	internal class TreeViewFileExplorer: TreeView
	{
		private ImageList FileImageList = new ImageList();
		private List<TaggedFileSystemWatcher> FileSystemWatchers = new List<TaggedFileSystemWatcher>();

		public TreeViewFileExplorer()
		{
			this.BeforeExpand += TreeViewFileExplorer_BeforeExpand;

			FileImageList.Images.Add( SystemIcons.GetStockIcon( StockIconId.Folder, StockIconOptions.SmallIcon ) ); // make first icon a folder icon, to be used by directories by default
			FileImageList.Images.Add( SystemIcons.GetStockIcon( StockIconId.DocumentNoAssociation, StockIconOptions.SmallIcon ) ); // used for unknown file types
			this.ImageList = FileImageList;
			this.TreeViewNodeSorter = (IComparer)new ExplorerTreeNodeComparer();
		}

		public void LoadFolder( string FolderPath )
		{
			if ( string.IsNullOrEmpty( FolderPath ) )
				return;

			BeginUpdate();

			Nodes.Clear();
			FileSystemWatchers.Clear();

			DirectoryInfo FolderInfo = new DirectoryInfo( FolderPath );
			AddFolderWatcher( FolderInfo );

			// fill directories
			foreach ( DirectoryInfo SubFolderInfo in FolderInfo.EnumerateDirectories() )
				Nodes.Add( CreateFolderNode( SubFolderInfo ) );

			// fill paths
			foreach ( FileInfo SubFileInfo in FolderInfo.EnumerateFiles() )
				Nodes.Add( CreateFileNode( SubFileInfo ) );

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

		private void EnsureIconForExtensionExists( string FileExtension )
		{
			SHFILEINFO shinfo = new SHFILEINFO();
			uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON;

			SHGetFileInfo( FileExtension, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf( shinfo ), flags );

			if ( shinfo.hIcon != IntPtr.Zero )
				FileImageList.Images.Add( FileExtension, (Icon)Icon.FromHandle( shinfo.hIcon ) );
		}

		private TreeNode CreateFolderNode( DirectoryInfo FolderInfo )
		{
			TreeNode DirectoryNode = new TreeNode()
			{
				Text = FolderInfo.Name,
				Tag = FolderInfo
			};

			// if this folder has any files or subfolders, add a dummy node to be able to expand it
			if ( FolderInfo.EnumerateFiles().Any() || FolderInfo.EnumerateDirectories().Any() )
				DirectoryNode.Nodes.Add( new TreeNode() );

			return DirectoryNode;
		}

		private TreeNode CreateFileNode( FileInfo FileInfo )
		{
			TreeNode FileNode = new TreeNode()
			{
				Text = FileInfo.Name,
				Tag = FileInfo,
				ImageIndex = 1, // unknown file type
				SelectedImageIndex = 1 // unknown file type
			};

			// load file icon
			if ( !string.IsNullOrEmpty( FileInfo.Extension ) )
			{
				if ( FileInfo.Extension == ".exe" )
				{
					// load an icon from that executable
					FileImageList.Images.Add( FileInfo.FullName, Icon.ExtractAssociatedIcon( FileInfo.FullName ) );
					FileNode.ImageIndex = FileNode.SelectedImageIndex = FileImageList.Images.IndexOfKey( FileInfo.FullName );
				}
				else
				{
					EnsureIconForExtensionExists( FileInfo.Extension );
					FileNode.ImageIndex = FileNode.SelectedImageIndex = FileImageList.Images.IndexOfKey( FileInfo.Extension );
				}
			}

			return FileNode;
		}

		private void AddFolderWatcher( DirectoryInfo FolderInfo, TreeNode? ParentNode = null )
		{
			TaggedFileSystemWatcher Watcher = new TaggedFileSystemWatcher( FolderInfo.FullName );
			Watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
			Watcher.EnableRaisingEvents = true;
			Watcher.IncludeSubdirectories = false;
			Watcher.Tag = ParentNode;
			Watcher.Created += FileSystemWatcher_Created;
			Watcher.Deleted += FileSystemWatcher_Deleted;
			Watcher.Renamed += FileSystemWatcher_Renamed;
			FileSystemWatchers.Add( Watcher );
		}

		private void TreeViewFileExplorer_BeforeExpand( object? sender, TreeViewCancelEventArgs e )
		{
			if ( e.Node == null )
				return;

			// if we already filled a list of files in this directory, bail
			if ( e.Node.FirstNode != null && e.Node.FirstNode.Tag != null )
				return;

			e.Node.Nodes.Clear();

			DirectoryInfo ParentDirectoryInfo = (DirectoryInfo)e.Node.Tag;
			AddFolderWatcher( ParentDirectoryInfo, e.Node ); // don't watch a folder until we open it for the first time

			// fill directories
			List<TreeNode> NewNodes = new List<TreeNode>();
			foreach ( DirectoryInfo DirectoryInfo in ParentDirectoryInfo.EnumerateDirectories() )
				NewNodes.Add( CreateFolderNode( DirectoryInfo ) );

			// fill paths
			foreach ( FileInfo FileInfo in ParentDirectoryInfo.EnumerateFiles() )
				NewNodes.Add( CreateFileNode( FileInfo ) );

			NewNodes.Sort( new ExplorerTreeNodeComparer() );
			e.Node.Nodes.AddRange( NewNodes.ToArray() );
		}

		private void FileSystemWatcher_Created( object sender, FileSystemEventArgs e )
		{
			TaggedFileSystemWatcher Watcher = (TaggedFileSystemWatcher)sender;

			TreeNode? ParentNode = (TreeNode?)Watcher.Tag;
			if ( ParentNode != null )
			{
				if ( File.GetAttributes( e.FullPath ).HasFlag( FileAttributes.Directory ) )
				{
					BeginInvoke( delegate
					{
						// need to move all child nodes to a list, sort it, and re-add them back, because stupid c# does not allow sorting
						// child nodes only, you can only sort the entire fucking tree at once!
						TreeNode[] NewNodes = new TreeNode[ParentNode.Nodes.Count + 1];
						ParentNode.Nodes.CopyTo( NewNodes, 0 );
						ParentNode.Nodes.Clear();

						NewNodes[NewNodes.Count() - 1] = CreateFolderNode( new DirectoryInfo( e.FullPath ) );

						Array.Sort( NewNodes, new ExplorerTreeNodeComparer() );
						ParentNode.Nodes.AddRange( NewNodes );
					} );
				}
				else
				{
					BeginInvoke( delegate
					{
						TreeNode[] NewNodes = new TreeNode[ParentNode.Nodes.Count + 1];
						ParentNode.Nodes.CopyTo( NewNodes, 0 );
						ParentNode.Nodes.Clear();

						NewNodes[NewNodes.Count() - 1] = CreateFileNode( new FileInfo( e.FullPath ) );

						Array.Sort( NewNodes, new ExplorerTreeNodeComparer() );
						ParentNode.Nodes.AddRange( NewNodes );
					} );
				}
			}
			else
			{
				if ( File.GetAttributes( e.FullPath ).HasFlag( FileAttributes.Directory ) )
				{
					BeginInvoke( delegate
					{
						TreeNode[] NewNodes = new TreeNode[Nodes.Count + 1];
						Nodes.CopyTo( NewNodes, 0 );
						Nodes.Clear();

						Nodes.Add( CreateFolderNode( new DirectoryInfo( e.FullPath ) ) );

						Array.Sort( NewNodes, new ExplorerTreeNodeComparer() );
						Nodes.AddRange( NewNodes );
					} );
				}
				else
				{
					BeginInvoke( delegate
					{
						TreeNode[] NewNodes = new TreeNode[Nodes.Count + 1];
						Nodes.CopyTo( NewNodes, 0 );
						Nodes.Clear();

						NewNodes[NewNodes.Count() - 1] = CreateFileNode( new FileInfo( e.FullPath ) );

						Array.Sort( NewNodes, new ExplorerTreeNodeComparer() );
						Nodes.AddRange( NewNodes );
					} );
				}
			}
		}

		private void FileSystemWatcher_Deleted( object sender, FileSystemEventArgs e )
		{
			TaggedFileSystemWatcher Watcher = (TaggedFileSystemWatcher)sender;

			TreeNode? ParentNode = (TreeNode?)Watcher.Tag;
			foreach ( TreeNode FileNode in (ParentNode != null) ? ParentNode.Nodes : Nodes )
			{
				FileSystemInfo FileInfo = (FileSystemInfo)FileNode.Tag;
				if ( FileInfo.FullName == e.FullPath )
				{
					BeginInvoke( delegate
					{
						FileNode.Remove();
					} );
					break;
				}
			}
		}

		private void FileSystemWatcher_Renamed( object sender, RenamedEventArgs e )
		{
			TaggedFileSystemWatcher Watcher = (TaggedFileSystemWatcher)sender;

			TreeNode? ParentNode = (TreeNode?)Watcher.Tag;
			foreach ( TreeNode FileNode in (ParentNode != null) ? ParentNode.Nodes : Nodes )
			{
				FileSystemInfo FileInfo = (FileSystemInfo)FileNode.Tag;
				if ( FileInfo.FullName == e.OldFullPath )
				{
					BeginInvoke( delegate
					{
						FileNode.Tag = FileInfo is FileInfo ? new FileInfo( e.FullPath ) : new DirectoryInfo( e.FullPath );
						FileNode.Name = e.FullPath;
						FileNode.Text = e.Name;
						Sort();
					} );
					break;
				}
			}
		}
	}
}
