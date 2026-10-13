using ORCToolKit.Search;

namespace ORCToolKit
{
	public partial class FormLocalizationEditor: FormEditor
	{
		private FileTypes.LocalizationFile _file = new FileTypes.LocalizationFile();
		override protected FileTypes.IFileType? CurrentFile { get { return _file; } }
		override protected string FileDialogFilter { get { return "All files (*.*)|*.*"; } }

		public FormLocalizationEditor()
		{
			InitializeComponent();
			Program.ReloadFormResources( this );

			OnEntriesUpdated += FormLocalizationEditor_OnEntriesUpdated;
			TextEntriesGridView.DataSource = _file.TextEntries;
		}

		private void FormLocalizationEditor_OnEntriesUpdated( Object? sender, EntriesUpdatedEventArgs e )
		{
			MenuStripItemFileSave.Enabled = e.HasUnsavedEdits;
			MenuStripItemEditInsertEntryBefore.Enabled = (_file.TextEntries.Count > 0);
			MenuStripItemEditRemoveEntry.Enabled = (_file.TextEntries.Count > 0);
			MenuStripItemEditSearch.Enabled = (_file.TextEntries.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.TextEntries.Count );
		}

		override protected bool TrySearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
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

							if ( SearchCompareString( (string)Cell.Value, SearchString, WholeWord, CaseSensitive ) )
							{
								TextEntriesGridView.CurrentCell = Cell;
								return true;
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

							if ( SearchCompareString( (string)Cell.Value, SearchString, WholeWord, CaseSensitive ) )
							{
								TextEntriesGridView.CurrentCell = Cell;
								return true;
							}
						}
					}

					break;
				}
			}

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

		private void MenuStripItemEditInsertEntryAfter_Click( object sender, EventArgs e )
		{
			Int32 HighestSelectedRow = Int32.MinValue;
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				HighestSelectedRow = Int32.Max( HighestSelectedRow, SelectedCell.RowIndex );

			// clamp to valid range
			HighestSelectedRow = Int32.Clamp( HighestSelectedRow + 1, 0, TextEntriesGridView.Rows.Count );

			_file.TextEntries.Insert( HighestSelectedRow, new FileTypes.LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			EntriesUpdated();
		}

		private void MenuStripItemEditInsertEntryBefore_Click( object sender, EventArgs e )
		{
			Int32 LowestSelectedRow = Int32.MaxValue;
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				LowestSelectedRow = Int32.Min( LowestSelectedRow, SelectedCell.RowIndex );
			LowestSelectedRow = Int32.Clamp( LowestSelectedRow, 0, TextEntriesGridView.Rows.Count );

			_file.TextEntries.Insert( LowestSelectedRow, new FileTypes.LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			EntriesUpdated();
		}

		private void MenuStripItemEditRemoveEntry_Click( object sender, EventArgs e )
		{
			foreach ( DataGridViewCell SelectedCell in TextEntriesGridView.SelectedCells )
				_file.TextEntries.RemoveAt( SelectedCell.RowIndex );
			EntriesUpdated( (TextEntriesGridView.Rows.Count > 0) );
		}

		private void MenuStripItemFileExit_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void MenuStripItemEditSearch_Click( object sender, EventArgs e )
		{
			InvokeSearchDialog();
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
