// this is a major hack to allow file explorer to be faster
// by not having to iterate through each node to find the one
// associated with the FileSystemWatcher
using System.IO;

namespace ORCToolKit
{
	internal class TaggedFileSystemWatcher: FileSystemWatcher
	{
		private object? _userdata;
		public object? Tag
		{
			get { return _userdata; }
			set { _userdata = value; }
		}

		public TaggedFileSystemWatcher() : base() { }
		public TaggedFileSystemWatcher( string path ) : base( path ) { }
		public TaggedFileSystemWatcher( string path, string filter ) : base( path, filter ) { }
	}
}
