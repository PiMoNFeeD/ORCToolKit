using ORCToolKit.FileTypes;
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
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.EntryTables.Count );
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

		private void EntryTreeView_AfterSelect( object sender, TreeViewEventArgs e )
		{
			EntryPropertyGrid.SelectedObject = e.Node.Tag;
		}

		private void EntryPropertyGrid_PropertyValueChanged( object s, PropertyValueChangedEventArgs e )
		{
			if ( s is not PropertyGrid )
				return;

			PropertyGrid EntryPropertyGrid = (PropertyGrid)s;
			TreeNode? TreeNode = FindTreeNodeForEntry( EntryPropertyGrid.SelectedObject, EntryTreeView.Nodes );
			Debug.Assert( TreeNode != null );
			TreeNode.Text = EntryPropertyGrid.SelectedObject.ToString(); // update name of this entry
			EntryPropertyGrid.Refresh();

			EntriesUpdated();
		}

		private TreeNode? FindTreeNodeForEntry( object Entry, TreeNodeCollection Nodes )
		{
			foreach ( TreeNode Node in Nodes )
			{
				if ( Node.Tag == Entry )
					return Node;

				TreeNode? ChildNode = FindTreeNodeForEntry( Entry, Node.Nodes );
				if ( ChildNode != null )
					return ChildNode;
			}

			return null;
		}
	}
}
