using ORCToolKit.FileTypes;
using System.Diagnostics;
using static ORCToolKit.FileTypes.SWB2DatabaseFile;

namespace ORCToolKit
{
	internal class TreeViewSWB2EntryExplorer: TreeView
	{
		public TreeViewSWB2EntryExplorer()
		{
			BeforeExpand += TreeViewSWB2EntryExplorer_BeforeExpand;
		}

		public void LoadSWB2File( SWB2DatabaseFile File )
		{
			BeginUpdate();

			Nodes.Clear();

			// create an array of nodes, and then populate it all at once using AddRange instead of adding nodes one by one
			TreeNode[] EntryTableNodes = new TreeNode[File.EntryTables.Count];
			for ( Int32 i = 0; i < File.EntryTables.Count; i++ )
				EntryTableNodes[i] = CreateEntryTableNode( File.EntryTables[i] );
			Nodes.AddRange( EntryTableNodes );

			EndUpdate();
		}

		public TreeNode CreateEntryTableNode( SWB2DatabaseFile.EntryTable EntryTable, TreeNodeCollection? InsertTo = null )
		{
			TreeNode EntryTableNode = new TreeNode()
			{
				Text = EntryTable.ToString(),
				Tag = EntryTable
			};

			// if this entry table has any entries, add a dummy node to be able to expand it
			if ( EntryTable.Entries.Count > 0 )
				EntryTableNode.Nodes.Add( new TreeNode() );

			if ( InsertTo != null )
				InsertTo.Add( EntryTableNode );

			return EntryTableNode;
		}

		public TreeNode CreateEntryNode( SWB2DatabaseFile.Entry Entry, TreeNodeCollection? InsertTo = null )
		{
			TreeNode EntryTableNode = new TreeNode()
			{
				Text = Entry.ToString(),
				Tag = Entry
			};

			if ( InsertTo != null )
				InsertTo.Add( EntryTableNode );

			return EntryTableNode;
		}

		private void TreeViewSWB2EntryExplorer_BeforeExpand( object? sender, TreeViewCancelEventArgs e )
		{
			if ( e.Node == null )
				return;

			// if we already filled a list of files in this directory, bail
			if ( e.Node.FirstNode != null && e.Node.FirstNode.Tag != null )
				return;

			e.Node.Nodes.Clear();

			SWB2DatabaseFile.EntryTable ParentEntryTable = (SWB2DatabaseFile.EntryTable)e.Node.Tag;
			Debug.Assert( ParentEntryTable.Entries.Count > 0 );

			TreeNode[] EntryNodes = new TreeNode[ParentEntryTable.Entries.Count];
			for ( Int32 i = 0; i < ParentEntryTable.Entries.Count; i++ )
			{
				if ( ParentEntryTable.Entries[i].ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
				{
					Debug.Assert( ParentEntryTable.Entries[i].Value != null );
					EntryNodes[i] = CreateEntryTableNode( ParentEntryTable.Entries[i].Value as SWB2DatabaseFile.EntryTable );
				}
				else
				{
					EntryNodes[i] = CreateEntryNode( ParentEntryTable.Entries[i] );
				}
			}
			e.Node.Nodes.AddRange( EntryNodes );
		}
	}
}
