using ORCToolKit.FileTypes;
using ORCToolKit.Search;
using System.ComponentModel;
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
			Program.ReloadFormResources( this );

			Shown += FormSWB2DatabaseEditor_Shown;
			OnNewFile += FormSWB2DatabaseEditor_OnNewFile;
			OnOpenFile += FormSWB2DatabaseEditor_OnOpenFile;
			OnEntriesUpdated += FormSWB2DatabaseEditor_OnEntriesUpdated;
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
					TreeNode TreeNode = FindAndExpandTreeNodeForEntry( EntryTreeView.Nodes, SearchResult );
					EntryTreeView.SelectedNode = TreeNode;
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

		private void EntryPropertyGrid_PropertyValueChanged( object s, PropertyValueChangedEventArgs e )
		{
			if ( s is not PropertyGrid )
				return;

			PropertyGrid EntryPropertyGrid = (PropertyGrid)s;
			TreeNode? TreeNode = FindTreeNodeForEntry( EntryTreeView.Nodes, EntryPropertyGrid.SelectedObject );
			Debug.Assert( TreeNode != null );
			TreeNode.Text = EntryPropertyGrid.SelectedObject.ToString(); // update name of this entry
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
