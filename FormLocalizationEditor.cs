using System.Text.RegularExpressions;
using System.Windows.Forms;
using ORCToolKit.FileTypes;
using ORCToolKit.Search;

namespace ORCToolKit
{
	public partial class FormLocalizationEditor: Form
	{
		private bool HasUnsavedEdits = false;
		private LocalizationFile CurrentDocument = new LocalizationFile();

		private FormSearch FormSearch = new FormSearch();
		private string LastSearchString = string.Empty;
		private bool LastSearchWholeWord = false;
		private bool LastSearchCaseSensitive = false;

		public FormLocalizationEditor()
		{
			InitializeComponent();
			Program.ReloadFormResources( this );

			TextEntriesGridView.DataSource = CurrentDocument.TextEntries;
			OnEntriesUpdated();

			FormSearch.OnSearch += FormSearch_OnSearch;
		}

		public void OpenFile( FileInfo LocalizationFileInfo )
		{
			HasUnsavedEdits = false;
			CurrentDocument.LoadFromFile( LocalizationFileInfo );
			OnEntriesUpdated();
		}

		public void SaveFile( FileInfo LocalizedFileInfo )
		{
			HasUnsavedEdits = false;
			CurrentDocument.SaveToFile( LocalizedFileInfo );
		}

		private bool InvokeSaveDialog()
		{
			using ( SaveFileDialog FileDialog = new SaveFileDialog() )
			{
				FileDialog.Filter = "All files (*.*)|*.*";
				FileDialog.RestoreDirectory = true;
				if ( FileDialog.ShowDialog() == DialogResult.OK )
				{
					FileInfo LocalizationFileInfo = new FileInfo( FileDialog.FileName );
					SaveFile( LocalizationFileInfo );
					return true;
				}
			}

			return false;
		}

		private void InvokeSearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			LastSearchString = SearchString;
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			switch ( Direction )
			{
				case SearchDirection.Forward:
				{
					for ( int i = TextEntriesGridView.CurrentRow != null ? TextEntriesGridView.CurrentRow.Index : 0; i < TextEntriesGridView.Rows.Count; i++ )
					{
						foreach ( DataGridViewCell Cell in TextEntriesGridView.Rows[i].Cells )
						{
							if ( TextEntriesGridView.CurrentCell != null )
							{
								// if this is a cell on the same row, skip it if it's behind of currently selected one
								if ( TextEntriesGridView.CurrentCell.RowIndex == Cell.RowIndex && Cell.ColumnIndex <= TextEntriesGridView.CurrentCell.ColumnIndex )
									continue;
							}

							string Text = (string)Cell.Value;

							bool Match = false;
							if ( WholeWord )
							{
								// use regex for whole word matching
								RegexOptions Options = RegexOptions.Compiled;
								if ( !CaseSensitive )
									Options |= RegexOptions.IgnoreCase;
								Match = Regex.IsMatch( Text, $@"\b{SearchString}\b", Options );
							}
							else
							{
								Match = Text.Contains( SearchString, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase );
							}

							if ( Match )
							{
								TextEntriesGridView.CurrentCell = Cell;
								return;
							}
						}
					}

					break;
				}
				case SearchDirection.Backward:
				{
					for ( int i = TextEntriesGridView.CurrentRow != null ? TextEntriesGridView.CurrentRow.Index : TextEntriesGridView.Rows.Count - 1; i >= 0; i-- )
					{
						foreach ( DataGridViewCell Cell in TextEntriesGridView.Rows[i].Cells )
						{
							if ( TextEntriesGridView.CurrentCell != null )
							{
								// if this is a cell on the same row, skip it if it's ahead of currently selected one
								if ( TextEntriesGridView.CurrentCell.RowIndex == Cell.RowIndex && Cell.ColumnIndex >= TextEntriesGridView.CurrentCell.ColumnIndex )
									continue;
							}

							string Text = (string)Cell.Value;

							bool Match = false;
							if ( WholeWord )
							{
								// use regex for whole word matching
								RegexOptions Options = RegexOptions.Compiled;
								if ( !CaseSensitive )
									Options |= RegexOptions.IgnoreCase;
								Match = Regex.IsMatch( Text, $@"\b{SearchString}\b", Options );
							}
							else
							{
								Match = Text.Contains( SearchString, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase );
							}

							if ( Match )
							{
								TextEntriesGridView.CurrentCell = Cell;
								return;
							}
						}
					}

					break;
				}
			}

			MessageBox.Show( string.Format( Resources.ORCToolKit.SearchFailedText, SearchString ), Resources.ORCToolKit.SearchFailedTitle,
				MessageBoxButtons.OK, MessageBoxIcon.Error );
		}

		private bool CheckUnsavedChanges()
		{
			if ( !HasUnsavedEdits )
				return true;

			switch ( MessageBox.Show( Resources.ORCToolKit.UnsavedChangesText, Resources.ORCToolKit.UnsavedChangesTitle,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information ) )
			{
				case DialogResult.Yes:
					return InvokeSaveDialog();
				case DialogResult.No:
					return true;
				case DialogResult.Cancel:
					return false;
			}

			return false;
		}

		private void OnEntriesUpdated()
		{
			MenuStripItemFileSave.Enabled = (CurrentDocument.TextEntries.Count > 0);
			MenuStripItemEditInsertEntryBefore.Enabled = (CurrentDocument.TextEntries.Count > 0);
			MenuStripItemEditRemoveEntry.Enabled = (CurrentDocument.TextEntries.Count > 0);
			MenuStripItemEditSearch.Enabled = (CurrentDocument.TextEntries.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (CurrentDocument.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (CurrentDocument.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, CurrentDocument.TextEntries.Count );
		}

		private void MenuStripItemFileNew_Click( object sender, EventArgs e )
		{
			if ( CheckUnsavedChanges() )
			{
				CurrentDocument.Reset();
				HasUnsavedEdits = false;
				OnEntriesUpdated();
			}
		}

		private void MenuStripItemFileOpen_Click( object sender, EventArgs e )
		{
			if ( CheckUnsavedChanges() )
			{
				using ( OpenFileDialog FileDialog = new OpenFileDialog() )
				{
					FileDialog.Filter = "All files (*.*)|*.*";
					FileDialog.RestoreDirectory = true;
					FileDialog.Multiselect = false;
					if ( FileDialog.ShowDialog() == DialogResult.OK )
					{
						FileInfo LocalizationFileInfo = new FileInfo( FileDialog.FileName );
						OpenFile( LocalizationFileInfo );
					}
				}
			}
		}

		private void MenuStripItemFileSave_Click( object sender, EventArgs e )
		{
			InvokeSaveDialog();
		}

		private void MenuStripItemEditInsertEntryAfter_Click( object sender, EventArgs e )
		{
			Int32 HighestSelectedRow = Int32.MinValue;
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				HighestSelectedRow = Int32.Max( HighestSelectedRow, SelectedCell.RowIndex );

			// clamp to valid range
			HighestSelectedRow = Int32.Clamp( HighestSelectedRow + 1, 0, TextEntriesGridView.Rows.Count );

			CurrentDocument.TextEntries.Insert( HighestSelectedRow, new LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			HasUnsavedEdits = true;
			OnEntriesUpdated();
		}

		private void MenuStripItemEditInsertEntryBefore_Click( object sender, EventArgs e )
		{
			Int32 LowestSelectedRow = Int32.MaxValue;
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				LowestSelectedRow = Int32.Min( LowestSelectedRow, SelectedCell.RowIndex );
			LowestSelectedRow = Int32.Clamp( LowestSelectedRow, 0, TextEntriesGridView.Rows.Count );

			CurrentDocument.TextEntries.Insert( LowestSelectedRow, new LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			HasUnsavedEdits = true;
			OnEntriesUpdated();
		}

		private void MenuStripItemEditRemoveEntry_Click( object sender, EventArgs e )
		{
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				CurrentDocument.TextEntries.RemoveAt( SelectedCell.RowIndex );
			HasUnsavedEdits = (TextEntriesGridView.Rows.Count > 0);
			OnEntriesUpdated();
		}

		private void MenuStripItemFileExit_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void FormLocalizationEditor_FormClosing( object sender, FormClosingEventArgs e )
		{
			if ( !CheckUnsavedChanges() )
				e.Cancel = true;
		}

		private void MenuStripItemEditSearch_Click( object sender, EventArgs e )
		{
			FormSearch.Show();
		}

		private void FormSearch_OnSearch( object sender, SearchEventArgs e )
		{
			InvokeSearch( e.Direction, e.SearchString, e.WholeWord, e.CaseSensitive );
		}

		private void MenuStripItemEditSearchForward_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Forward, LastSearchString, LastSearchWholeWord, LastSearchCaseSensitive );
		}

		private void MenuStripItemEditSearchBackward_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Backward, LastSearchString, LastSearchWholeWord, LastSearchCaseSensitive );
		}
	}
}
