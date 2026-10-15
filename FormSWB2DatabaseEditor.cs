using ORCToolKit.FileTypes;
using ORCToolKit.Search;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Linq;

namespace ORCToolKit
{
	public partial class FormSWB2DatabaseEditor: FormEditor
	{
		private SWB2DatabaseFile _file = new SWB2DatabaseFile();
		override protected IFileType? CurrentFile { get { return _file; } }
		override protected string FileDialogFilter { get { return "SWB2 files (*.swb2)|*.swb2"; } }

		public FormSWB2DatabaseEditor()
		{
			InitializeComponent();
			LocalizeComponent();

			Shown += FormSWB2DatabaseEditor_Shown;
			OnNewFile += FormSWB2DatabaseEditor_OnNewFile;
			OnOpenFile += FormSWB2DatabaseEditor_OnOpenFile;
			OnEntriesUpdated += FormSWB2DatabaseEditor_OnEntriesUpdated;
		}

		public override LocalizedControl[] LocalizedControls
		{
			get
			{
				return [
					new LocalizedControl { Object = this, ObjectName = this.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFile, ObjectName = MenuStripItemFile.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileNew, ObjectName = MenuStripItemFileNew.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileOpen, ObjectName = MenuStripItemFileOpen.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileSave, ObjectName = MenuStripItemFileSave.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEdit, ObjectName = MenuStripItemEdit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditInsertEntryTable, ObjectName = MenuStripItemEditInsertEntryTable.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileExit, ObjectName = MenuStripItemFileExit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearch, ObjectName = MenuStripItemEditSearch.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchForward, ObjectName = MenuStripItemEditSearchForward.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchBackward, ObjectName = MenuStripItemEditSearchBackward.Name, FieldName = "Text" },
					new LocalizedControl { Object = EntryTreeViewContextMenuItemInsertEntry, ObjectName = EntryTreeViewContextMenuItemInsertEntry.Name, FieldName = "Text" },
					new LocalizedControl { Object = EntryTreeViewContextMenuItemDelete, ObjectName = EntryTreeViewContextMenuItemDelete.Name, FieldName = "Text" },
				];
			}
		}

		private void FormSWB2DatabaseEditor_Shown( Object? sender, EventArgs e )
		{
			// keep in sync with FormSWB2DatabaseEditor_OnEntriesUpdated
			MenuStripItemFileSave.Enabled = false;
			MenuStripItemEditSearch.Enabled = (_file.EntryTables.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.EntryTables.Count );
		}

		private void FormSWB2DatabaseEditor_OnNewFile( Object? sender, EventArgs e )
		{
			EntryTreeView.Nodes.Clear();
			EntryPropertyGrid.SelectedObject = null;
		}

		private void FormSWB2DatabaseEditor_OnOpenFile( Object? sender, EventArgs e )
		{
			EntryTreeView.LoadSWB2File( _file );
		}

		private void FormSWB2DatabaseEditor_OnEntriesUpdated( Object? sender, EntriesUpdatedEventArgs e )
		{
			// in case the number of entries has changed, update the text of the select node hierarchy
			if ( EntryTreeView.SelectedNode != null )
			{
				TreeNode ParentNode = EntryTreeView.SelectedNode;
				while ( ParentNode != null )
				{
					ParentNode.Text = ParentNode.Tag.ToString();
					ParentNode = ParentNode.Parent;
				}
			}

			// keep in sync with FormSWB2DatabaseEditor_Shown
			MenuStripItemFileSave.Enabled = e.HasUnsavedEdits;
			MenuStripItemEditSearch.Enabled = (_file.EntryTables.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.EntryTables.Count );
		}

		override protected bool TrySearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			object? SelectedEntry = EntryTreeView.SelectedNode?.Tag;
			for ( Int32 i = (Direction == SearchDirection.Backward) ? (_file.EntryTables.Count - 1) : 0;
				  i >= 0 && i < _file.EntryTables.Count;
				  i += (Direction == SearchDirection.Backward) ? -1 : 1 )
			{
				object? SearchResult = RecursiveFindSWB2Entry( _file.EntryTables[i], ref SelectedEntry, Direction, SearchString, WholeWord, CaseSensitive );
				if ( SearchResult != null )
				{
					EntryTreeView.BeginUpdate();
					TreeNode TreeNode = FindAndExpandTreeNodeForEntry( EntryTreeView.Nodes, SearchResult );
					EntryTreeView.SelectedNode = TreeNode;
					EntryTreeView.EndUpdate();
					return true;
				}
			}

			// if this isn't null, then we have never found our search starting point in the first place
			// which is impossible??!?
			Debug.Assert( SelectedEntry == null );
			return false;
		}

		private void MenuStripItemFileNew_Click( object sender, EventArgs e )
		{
			NewFile();
		}

		private void MenuStripItemFileOpen_Click( object sender, EventArgs e )
		{
			InvokeOpenDialog();
		}

		private void MenuStripItemFileSave_Click( object sender, EventArgs e )
		{
			InvokeSaveDialog();
		}

		private void MenuStripItemFileExit_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void MenuStripItemEditInsertEntryTable_Click( object sender, EventArgs e )
		{
			SWB2DatabaseFile.EntryTable NewEntry = new SWB2DatabaseFile.EntryTable( null );
			_file.EntryTables.Add( NewEntry );
			EntryTreeView.CreateEntryTableNode( NewEntry, EntryTreeView.Nodes );
		}

		private void MenuStripItemEditSearch_Click( object sender, EventArgs e )
		{
			InvokeSearchDialog();
		}

		private void MenuStripItemEditSearchForward_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Forward, LastSearchString, LastSearchWholeWord, LastSearchCaseSensitive );
		}

		private void MenuStripItemEditSearchBackward_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Backward, LastSearchString, LastSearchWholeWord, LastSearchCaseSensitive );
		}

		private void EntryTreeView_AfterSelect( object sender, TreeViewEventArgs e )
		{
			EntryPropertyGrid.SelectedObject = e.Node.Tag;
		}

		private void EntryTreeView_NodeMouseClick( Object sender, TreeNodeMouseClickEventArgs e )
		{
			TreeView EntryView = (TreeView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					EntryView.SelectedNode = e.Node;

					// open context menu
					EntryTreeViewContextMenuItemInsertEntry.Visible = (e.Node.Tag is SWB2DatabaseFile.EntryTable);
					EntryTreeViewContextMenu.Tag = e.Node;
					EntryTreeViewContextMenu.Show( EntryView, e.Location );

					break;
				}
			}
		}

		private void EntryTreeViewContextMenuItemInsertEntry_Click( object sender, EventArgs e )
		{
			EntryTreeView.BeginUpdate();

			TreeNode SelectedNode = (TreeNode)EntryTreeViewContextMenu.Tag;
			SWB2DatabaseFile.EntryTable SelectedTable = (SWB2DatabaseFile.EntryTable)SelectedNode.Tag;

			SWB2DatabaseFile.Entry NewEntry = new SWB2DatabaseFile.Entry( SelectedTable );
			SelectedTable.Entries.Add( NewEntry );

			if ( SelectedNode.Nodes.Count > 0 && SelectedNode.Nodes[0].Tag != null )
			{
				// if the child list has already been populated, then manually add a new node
				TreeNode NewNode = EntryTreeView.CreateEntryNode( NewEntry, SelectedNode.Nodes );
				EntryTreeView.SelectedNode = NewNode;
			}
			else
			{
				// otherwise, just expand it and let it make a node manually
				if ( SelectedNode.Nodes.Count == 0 ) // make sure it can actually expand!
					SelectedNode.Nodes.Add( new TreeNode() );
				SelectedNode.Expand();

				TreeNode? NewNode = FindTreeNodeForEntry( EntryTreeView.Nodes, NewEntry );
				Debug.Assert( NewNode != null );
				EntryTreeView.SelectedNode = NewNode;
			}
			// TODO: it doesn't scroll to the new node?!?!?

			EntryTreeView.EndUpdate();

			EntriesUpdated();
		}

		private void EntryTreeViewContextMenuItemDelete_Click( object sender, EventArgs e )
		{
			TreeNode SelectedNode = (TreeNode)EntryTreeViewContextMenu.Tag;
			object SelectedEntry = SelectedNode.Tag;
			if ( SelectedEntry is SWB2DatabaseFile.EntryTable EntryTable )
			{
				if ( EntryTable.ParentTable != null )
				{
					foreach ( SWB2DatabaseFile.Entry Entry in EntryTable.Entries )
					{
						if ( Entry.Value == SelectedEntry )
						{
							EntryTable.Entries.Remove( Entry );
							break;
						}
					}
				}
				else
				{
					_file.EntryTables.Remove( EntryTable );
				}
			}
			else
			{
				SWB2DatabaseFile.Entry Entry = (SWB2DatabaseFile.Entry)SelectedEntry;
				Debug.Assert( Entry.ParentTable != null );

				foreach ( SWB2DatabaseFile.Entry SubEntry in Entry.ParentTable.Entries )
				{
					if ( SubEntry == Entry )
					{
						Entry.ParentTable.Entries.Remove( SubEntry );
						break;
					}
				}
			}

			SelectedNode.Remove();

			EntriesUpdated();
		}

		private void EntryPropertyGrid_PropertyValueChanged( object s, PropertyValueChangedEventArgs e )
		{
			if ( s is not PropertyGrid )
				return;

			PropertyGrid EntryPropertyGrid = (PropertyGrid)s;
			TreeNode? TreeNode = FindTreeNodeForEntry( EntryTreeView.Nodes, EntryPropertyGrid.SelectedObject );
			Debug.Assert( TreeNode != null );
			EntryPropertyGrid.Refresh();

			EntriesUpdated();
		}

		private TreeNode? FindTreeNodeForEntry( TreeNodeCollection Nodes, object Entry )
		{
			foreach ( TreeNode Node in Nodes )
			{
				if ( Node.Tag == Entry )
					return Node;

				// only look through child nodes if it's actually populated nodes with valid entries
				if ( Node.Nodes.Count > 0 && Node.Nodes[0].Tag != null )
				{
					TreeNode? ChildNode = FindTreeNodeForEntry( Node.Nodes, Entry );
					if ( ChildNode != null )
						return ChildNode;
				}
			}

			return null;
		}

		// NOTE: this uses CRC32.ToString() for comparison purposes, so that we can find by *exactly* how they are visualized
		// rather than the CRC32 of the string itself (so that you can search by both strings and CRC32s at the same time!)
		public object? RecursiveFindSWB2Entry( SWB2DatabaseFile.EntryTable EntryTable, ref object? SearchStartPoint, SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			if ( SearchStartPoint != null )
			{
				// if we haven't found the search start point, ignore all entries
				if ( EntryTable == SearchStartPoint )
					SearchStartPoint = null;
			}
			else
			{
				if ( StringSearch.Compare( EntryTable.Identifier.ToString(), SearchString, WholeWord, CaseSensitive ) )
					return EntryTable;
			}

			for ( Int32 i = (Direction == SearchDirection.Backward) ? (EntryTable.Entries.Count - 1) : 0;
				  i >= 0 && i < EntryTable.Entries.Count;
				  i += (Direction == SearchDirection.Backward) ? -1 : 1 )
			{
				SWB2DatabaseFile.Entry Entry = EntryTable.Entries[i];

				if ( SearchStartPoint != null )
				{
					// if we haven't found the search start point, ignore all entries
					if ( Entry == SearchStartPoint )
						SearchStartPoint = null;
				}
				else
				{
					if ( StringSearch.Compare( Entry.Key.ToString(), SearchString, WholeWord, CaseSensitive ) )
						return Entry;
				}

				if ( Entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
				{
					SWB2DatabaseFile.EntryTable Value = Entry.Value as SWB2DatabaseFile.EntryTable;
					object? Result = RecursiveFindSWB2Entry( Value, ref SearchStartPoint, Direction, SearchString, WholeWord, CaseSensitive );
					if ( Result != null )
						return Result;
				}
			}

			return null;
		}

		private TreeNode FindAndExpandTreeNodeForEntry( TreeNodeCollection Nodes, object Entry )
		{
			// expand our parent first, because we won't exist until the parent gets expanded!
			SWB2DatabaseFile.EntryTable? ParentTable;
			if ( Entry is SWB2DatabaseFile.EntryTable EntryTable )
				ParentTable = EntryTable.ParentTable;
			else
				ParentTable = (Entry as SWB2DatabaseFile.Entry).ParentTable;

			if ( ParentTable != null )
			{
				TreeNode ParentNode = FindAndExpandTreeNodeForEntry( Nodes, ParentTable );
				// if this asserts, then either this logic is seriously flawed (it wasn't when I wrote it)
				// or we ended up in a scenario where recursion did not expand a parent of our parent so we ended up without one...
				Debug.Assert( ParentNode != null );
				ParentNode.Expand();
				Nodes = ParentNode.Nodes; // no need to look through the root if we already know where we reside...
			}

			foreach ( TreeNode Node in Nodes )
			{
				if ( Node.Tag == Entry )
					return Node;
			}

			// we should NEVER end up here!
			Debug.Assert( false );
			return null;
		}
	}
}
