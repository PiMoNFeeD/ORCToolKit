using ORCToolKit.FileTypes;
using ORCToolKit.Search;
using System.Diagnostics;

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
			OnSaveFile += FormSWB2DatabaseEditor_OnSaveFile;
			OnEntriesUpdated += FormSWB2DatabaseEditor_OnEntriesUpdated;
#if DEBUG // for hashcat
			EntryTreeView.NodeMouseClick += EntryTreeView_NodeMouseClick1;
#endif
		}

#if DEBUG // for hashcat
		private void RecurseEntryTable( ref string result, SWB2DatabaseFile.EntryTable entryTable )
		{
			if ( CRC32StringMap.FindString( entryTable.Identifier ) == null )
			{
				result += (~(UInt32)entryTable.Identifier).ToString( "X8" );
				result += '\n';
			}
			foreach ( SWB2DatabaseFile.Entry entry in entryTable.Entries )
			{
				if ( CRC32StringMap.FindString( entry.Key ) == null )
				{
					result += (~(UInt32)entry.Key).ToString( "X8" );
					result += '\n';
				}
				if ( entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_STRING_HASH )
				{
					CRC32[] values = entry.Value as CRC32[];
					foreach ( CRC32 value in values )
					{
						if ( CRC32StringMap.FindString( value ) == null )
						{
							result += (~(UInt32)value).ToString( "X8" );
							result += '\n';
						}
					}
				}
				else if ( entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
				{
					RecurseEntryTable( ref result, entry.Value as SWB2DatabaseFile.EntryTable );
				}
			}
		}

		private void EntryTreeView_NodeMouseClick1( object? sender, TreeNodeMouseClickEventArgs e )
		{
			if ( e.Button == MouseButtons.Right )
			{
				string result = string.Empty;
				foreach ( SWB2DatabaseFile.EntryTable entryTable in _file.EntryTables )
					RecurseEntryTable( ref result, entryTable );
				Clipboard.SetText( result );
			}
		}
#endif

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

		private void FormSWB2DatabaseEditor_Shown( object? sender, EventArgs e )
		{
			// keep in sync with FormSWB2DatabaseEditor_OnEntriesUpdated
			MenuStripItemFileSave.Enabled = false;
			MenuStripItemEditSearch.Enabled = (_file.EntryTables.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.EntryTables.Count );
		}

		private void FormSWB2DatabaseEditor_OnNewFile( object? sender, EventArgs e )
		{
			EntryTreeView.Nodes.Clear();
			EntryPropertyGrid.SelectedObject = null;
		}

		private void FormSWB2DatabaseEditor_OnOpenFile( object? sender, EventArgs e )
		{
			EntryTreeView.LoadSWB2File( _file );
		}

		private void FormSWB2DatabaseEditor_OnSaveFile( Object? sender, EventArgs e )
		{
			MenuStripItemFileSave.Enabled = false;
		}

		private void FormSWB2DatabaseEditor_OnEntriesUpdated( object? sender, EntriesUpdatedEventArgs e )
		{
			TreeNode selectedNode = EntryTreeView.SelectedNode;
			if ( selectedNode != null )
			{
				// check if currently selected node is not the type it should be, in case the user changed to TYPE_ENTRY_TABLE
				if ( selectedNode.Tag is SWB2DatabaseFile.Entry entry )
				{
					if ( entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
					{
						selectedNode.Tag = entry.Value;
						EntryPropertyGrid.SelectedObject = entry.Value;
					}
				}

				// in case the number of entries has changed, update the text of the select node hierarchy
				TreeNode parentNode = selectedNode;
				while ( parentNode != null )
				{
					parentNode.Text = parentNode.Tag.ToString();
					parentNode = parentNode.Parent;
				}
			}

			// keep in sync with FormSWB2DatabaseEditor_Shown
			MenuStripItemFileSave.Enabled = e.HasUnsavedEdits;
			MenuStripItemEditSearch.Enabled = (_file.EntryTables.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.EntryTables.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.EntryTables.Count );
		}

		override protected bool TrySearch( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
		{
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			object? selectedEntry = EntryTreeView.SelectedNode?.Tag;
			for ( Int32 i = (direction == SearchDirection.Backward) ? (_file.EntryTables.Count - 1) : 0;
				  i >= 0 && i < _file.EntryTables.Count;
				  i += (direction == SearchDirection.Backward) ? -1 : 1 )
			{
				object? searchResult = RecursiveFindSWB2Entry( _file.EntryTables[i], ref selectedEntry, direction, searchString, wholeWord, caseSensitive );
				if ( searchResult != null )
				{
					EntryTreeView.BeginUpdate();
					TreeNode treeNode = FindAndExpandTreeNodeForEntry( EntryTreeView.Nodes, searchResult );
					EntryTreeView.SelectedNode = treeNode;
					EntryTreeView.EndUpdate();
					return true;
				}
			}

			// if this isn't null, then we have never found our search starting point in the first place
			// which is impossible??!?
			Debug.Assert( selectedEntry == null );
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
			SWB2DatabaseFile.EntryTable newEntry = new SWB2DatabaseFile.EntryTable( null );
			_file.EntryTables.Add( newEntry );
			EntryTreeView.CreateEntryTableNode( newEntry, EntryTreeView.Nodes );
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

		private void EntryTreeView_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			TreeView entryView = (TreeView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					entryView.SelectedNode = e.Node;

					// open context menu
					EntryTreeViewContextMenuItemInsertEntry.Visible = (e.Node.Tag is SWB2DatabaseFile.EntryTable);
					EntryTreeViewContextMenu.Tag = e.Node;
					EntryTreeViewContextMenu.Show( entryView, e.Location );

					break;
				}
			}
		}

		private void EntryTreeViewContextMenuItemInsertEntry_Click( object sender, EventArgs e )
		{
			EntryTreeView.BeginUpdate();

			TreeNode selectedNode = (TreeNode)EntryTreeViewContextMenu.Tag;
			SWB2DatabaseFile.EntryTable selectedTable = (SWB2DatabaseFile.EntryTable)selectedNode.Tag;

			SWB2DatabaseFile.Entry newEntry = new SWB2DatabaseFile.Entry( selectedTable );
			selectedTable.Entries.Add( newEntry );

			if ( selectedNode.Nodes.Count > 0 && selectedNode.Nodes[0].Tag != null )
			{
				// if the child list has already been populated, then manually add a new node
				TreeNode newNode = EntryTreeView.CreateEntryNode( newEntry, selectedNode.Nodes );
				EntryTreeView.SelectedNode = newNode;
			}
			else
			{
				// otherwise, just expand it and let it make a node manually
				if ( selectedNode.Nodes.Count == 0 ) // make sure it can actually expand!
					selectedNode.Nodes.Add( new TreeNode() );
				selectedNode.Expand();

				TreeNode? newNode = FindTreeNodeForEntry( EntryTreeView.Nodes, newEntry );
				Debug.Assert( newNode != null );
				EntryTreeView.SelectedNode = newNode;
			}
			// TODO: it doesn't scroll to the new node?!?!?

			EntryTreeView.EndUpdate();

			EntriesUpdated();
		}

		private void EntryTreeViewContextMenuItemDelete_Click( object sender, EventArgs e )
		{
			TreeNode selectedNode = (TreeNode)EntryTreeViewContextMenu.Tag;
			object selectedEntry = selectedNode.Tag;
			if ( selectedEntry is SWB2DatabaseFile.EntryTable entryTable )
			{
				if ( entryTable.ParentTable != null )
				{
					foreach ( SWB2DatabaseFile.Entry entry in entryTable.Entries )
					{
						if ( entry.Value == selectedEntry )
						{
							entryTable.Entries.Remove( entry );
							break;
						}
					}
				}
				else
				{
					_file.EntryTables.Remove( entryTable );
				}
			}
			else
			{
				SWB2DatabaseFile.Entry entry = (SWB2DatabaseFile.Entry)selectedEntry;
				Debug.Assert( entry.ParentTable != null );

				foreach ( SWB2DatabaseFile.Entry subEntry in entry.ParentTable.Entries )
				{
					if ( subEntry == entry )
					{
						entry.ParentTable.Entries.Remove( subEntry );
						break;
					}
				}
			}

			selectedNode.Remove();

			EntriesUpdated();
		}

		private void EntryPropertyGrid_PropertyValueChanged( object s, PropertyValueChangedEventArgs e )
		{
			if ( s is not PropertyGrid )
				return;

			PropertyGrid entryPropertyGrid = (PropertyGrid)s;
			TreeNode? treeNode = FindTreeNodeForEntry( EntryTreeView.Nodes, EntryPropertyGrid.SelectedObject );
			Debug.Assert( treeNode != null );
			EntryPropertyGrid.Refresh();

			EntriesUpdated();
		}

		private TreeNode? FindTreeNodeForEntry( TreeNodeCollection nodes, object entry )
		{
			foreach ( TreeNode node in nodes )
			{
				if ( node.Tag == entry )
					return node;

				// only look through child nodes if it's actually populated nodes with valid entries
				if ( node.Nodes.Count > 0 && node.Nodes[0].Tag != null )
				{
					TreeNode? childNode = FindTreeNodeForEntry( node.Nodes, entry );
					if ( childNode != null )
						return childNode;
				}
			}

			return null;
		}

		// NOTE: this uses CRC32.ToString() for comparison purposes, so that we can find by *exactly* how they are visualized
		// rather than the CRC32 of the string itself (so that you can search by both strings and CRC32s at the same time!)
		public object? RecursiveFindSWB2Entry( SWB2DatabaseFile.EntryTable entryTable, ref object? searchStartPoint, SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
		{
			if ( searchStartPoint != null )
			{
				// if we haven't found the search start point, ignore all entries
				if ( entryTable == searchStartPoint )
					searchStartPoint = null;
			}
			else
			{
				if ( StringSearch.Compare( entryTable.Identifier.ToString(), searchString, wholeWord, caseSensitive ) )
					return entryTable;
			}

			for ( Int32 i = (direction == SearchDirection.Backward) ? (entryTable.Entries.Count - 1) : 0;
				  i >= 0 && i < entryTable.Entries.Count;
				  i += (direction == SearchDirection.Backward) ? -1 : 1 )
			{
				SWB2DatabaseFile.Entry entry = entryTable.Entries[i];

				if ( searchStartPoint != null )
				{
					// if we haven't found the search start point, ignore all entries
					if ( entry == searchStartPoint )
						searchStartPoint = null;
				}
				else
				{
					if ( StringSearch.Compare( entry.Key.ToString(), searchString, wholeWord, caseSensitive ) )
					{
						if ( entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
							return entry.Value;
						return entry;
					}
				}

				if ( entry.ValueType == SWB2DatabaseFile.EntryValueType.TYPE_ENTRY_TABLE )
				{
					SWB2DatabaseFile.EntryTable value = entry.Value as SWB2DatabaseFile.EntryTable;
					object? result = RecursiveFindSWB2Entry( value, ref searchStartPoint, direction, searchString, wholeWord, caseSensitive );
					if ( result != null )
						return result;
				}
			}

			return null;
		}

		private TreeNode FindAndExpandTreeNodeForEntry( TreeNodeCollection nodes, object entry )
		{
			// expand our parent first, because we won't exist until the parent gets expanded!
			SWB2DatabaseFile.EntryTable? parentTable;
			if ( entry is SWB2DatabaseFile.EntryTable entryTable )
				parentTable = entryTable.ParentTable;
			else
				parentTable = (entry as SWB2DatabaseFile.Entry).ParentTable;

			if ( parentTable != null )
			{
				TreeNode parentNode = FindAndExpandTreeNodeForEntry( nodes, parentTable );
				// if this asserts, then either this logic is seriously flawed (it wasn't when I wrote it)
				// or we ended up in a scenario where recursion did not expand a parent of our parent so we ended up without one...
				Debug.Assert( parentNode != null );
				parentNode.Expand();
				nodes = parentNode.Nodes; // no need to look through the root if we already know where we reside...
			}

			foreach ( TreeNode node in nodes )
			{
				if ( node.Tag == entry )
					return node;
			}

			// we should NEVER end up here!
			Debug.Assert( false );
			return null;
		}
	}
}
