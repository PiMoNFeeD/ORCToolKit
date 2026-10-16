using ORCToolKit.FileTypes;
using System.Diagnostics;

namespace ORCToolKit
{
	internal class TreeViewSWB2EntryExplorer: TreeView
	{
		public TreeViewSWB2EntryExplorer()
		{
			BeforeExpand += TreeViewSWB2EntryExplorer_BeforeExpand;
		}

		public void LoadSWB2File( SWB2DatabaseFile file )
		{
			BeginUpdate();

			Nodes.Clear();

			// create an array of nodes, and then populate it all at once using AddRange instead of adding nodes one by one
			TreeNode[] entryTableNodes = new TreeNode[file.EntryTables.Count];
			for ( Int32 i = 0; i < file.EntryTables.Count; i++ )
				entryTableNodes[i] = CreateEntryTableNode( file.EntryTables[i] );
			Nodes.AddRange( entryTableNodes );

			EndUpdate();
		}

		public TreeNode CreateEntryTableNode( SWB2DatabaseFile.EntryTable entryTable, TreeNodeCollection? insertTo = null )
		{
			TreeNode entryTableNode = new TreeNode()
			{
				Text = entryTable.ToString(),
				Tag = entryTable
			};

			// if this entry table has any entries, add a dummy node to be able to expand it
			if ( entryTable.Entries.Count > 0 )
				entryTableNode.Nodes.Add( new TreeNode() );

			if ( insertTo != null )
				insertTo.Add( entryTableNode );

			return entryTableNode;
		}

		public TreeNode CreateEntryNode( SWB2DatabaseFile.Entry entry, TreeNodeCollection? insertTo = null )
		{
			TreeNode entryTableNode = new TreeNode()
			{
				Text = entry.ToString(),
				Tag = entry
			};

			if ( insertTo != null )
				insertTo.Add( entryTableNode );

			return entryTableNode;
		}

		private void TreeViewSWB2EntryExplorer_BeforeExpand( object? sender, TreeViewCancelEventArgs e )
		{
			if ( e.Node == null )
				return;

			// if we already filled a list of files in this directory, bail
			if ( e.Node.FirstNode != null && e.Node.FirstNode.Tag != null )
				return;

			e.Node.Nodes.Clear();

			SWB2DatabaseFile.EntryTable parentEntryTable = (SWB2DatabaseFile.EntryTable)e.Node.Tag;
			Debug.Assert( parentEntryTable.Entries.Count > 0 );

			TreeNode[] entryNodes = new TreeNode[parentEntryTable.Entries.Count];
			for ( Int32 i = 0; i < parentEntryTable.Entries.Count; i++ )
			{
				if ( parentEntryTable.Entries[i].ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
				{
					Debug.Assert( parentEntryTable.Entries[i].Value != null );
					entryNodes[i] = CreateEntryTableNode( parentEntryTable.Entries[i].Value as SWB2DatabaseFile.EntryTable );
				}
				else
				{
					entryNodes[i] = CreateEntryNode( parentEntryTable.Entries[i] );
				}
			}
			e.Node.Nodes.AddRange( entryNodes );
		}
	}
}
