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
			LocalizeComponent();

			Shown += FormLocalizationEditor_Shown;
			OnEntriesUpdated += FormLocalizationEditor_OnEntriesUpdated;
			TextEntriesGridView.DataSource = _file.TextEntries;
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
					new LocalizedControl { Object = MenuStripItemEditInsertEntryAfter, ObjectName = MenuStripItemEditInsertEntryAfter.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditInsertEntryBefore, ObjectName = MenuStripItemEditInsertEntryBefore.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditRemoveEntry, ObjectName = MenuStripItemEditRemoveEntry.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileExit, ObjectName = MenuStripItemFileExit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearch, ObjectName = MenuStripItemEditSearch.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchForward, ObjectName = MenuStripItemEditSearchForward.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchBackward, ObjectName = MenuStripItemEditSearchBackward.Name, FieldName = "Text" },
					new LocalizedControl { Object = TextEntriesGridViewNameColumn, ObjectName = TextEntriesGridViewNameColumn.Name, FieldName = "HeaderText" },
					new LocalizedControl { Object = TextEntriesGridViewTextColumn, ObjectName = TextEntriesGridViewTextColumn.Name, FieldName = "HeaderText" },
				];
			}
		}

		private void FormLocalizationEditor_Shown( object? sender, EventArgs e )
		{
			// keep in sync with FormLocalizationEditor_OnEntriesUpdated
			MenuStripItemFileSave.Enabled = false;
			MenuStripItemEditSearch.Enabled = (_file.TextEntries.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.TextEntries.Count );
		}

		private void FormLocalizationEditor_OnEntriesUpdated( object? sender, EntriesUpdatedEventArgs e )
		{
			// keep in sync with FormLocalizationEditor_Shown
			MenuStripItemFileSave.Enabled = e.HasUnsavedEdits;
			MenuStripItemEditSearch.Enabled = (_file.TextEntries.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.TextEntries.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.TextEntries.Count );
		}

		override protected bool TrySearch( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
		{
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			switch ( direction )
			{
				case SearchDirection.Forward:
				{
					for ( int i = TextEntriesGridView.CurrentRow != null ? TextEntriesGridView.CurrentRow.Index : 0; i < TextEntriesGridView.Rows.Count; i++ )
					{
						foreach ( DataGridViewCell cell in TextEntriesGridView.Rows[i].Cells )
						{
							if ( TextEntriesGridView.CurrentCell != null )
							{
								// if this is a cell on the same row, skip it if it's behind of currently selected one
								if ( TextEntriesGridView.CurrentCell.RowIndex == cell.RowIndex && cell.ColumnIndex <= TextEntriesGridView.CurrentCell.ColumnIndex )
									continue;
							}

							if ( StringSearch.Compare( (string)cell.Value, searchString, wholeWord, caseSensitive ) )
							{
								TextEntriesGridView.CurrentCell = cell;
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
						foreach ( DataGridViewCell cell in TextEntriesGridView.Rows[i].Cells )
						{
							if ( TextEntriesGridView.CurrentCell != null )
							{
								// if this is a cell on the same row, skip it if it's ahead of currently selected one
								if ( TextEntriesGridView.CurrentCell.RowIndex == cell.RowIndex && cell.ColumnIndex >= TextEntriesGridView.CurrentCell.ColumnIndex )
									continue;
							}

							if ( StringSearch.Compare( (string)cell.Value, searchString, wholeWord, caseSensitive ) )
							{
								TextEntriesGridView.CurrentCell = cell;
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
			Int32 highestSelectedRow = Int32.MinValue;
			foreach ( DataGridViewCell selectedCell in TextEntriesGridView.SelectedCells )
				highestSelectedRow = Int32.Max( highestSelectedRow, selectedCell.RowIndex );

			// clamp to valid range
			highestSelectedRow = Int32.Clamp( highestSelectedRow + 1, 0, TextEntriesGridView.Rows.Count );

			_file.TextEntries.Insert( highestSelectedRow, new FileTypes.LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			EntriesUpdated();
		}

		private void MenuStripItemEditInsertEntryBefore_Click( object sender, EventArgs e )
		{
			Int32 lowestSelectedRow = Int32.MaxValue;
			foreach ( DataGridViewCell selectedCell in TextEntriesGridView.SelectedCells )
				lowestSelectedRow = Int32.Min( lowestSelectedRow, selectedCell.RowIndex );
			lowestSelectedRow = Int32.Clamp( lowestSelectedRow, 0, TextEntriesGridView.Rows.Count );

			_file.TextEntries.Insert( lowestSelectedRow, new FileTypes.LocalizationFile.TextEntry() { Name = string.Empty, Text = string.Empty } );
			EntriesUpdated();
		}

		private void MenuStripItemEditRemoveEntry_Click( object sender, EventArgs e )
		{
			foreach ( DataGridViewCell selectedCell in TextEntriesGridView.SelectedCells )
				_file.TextEntries.RemoveAt( selectedCell.RowIndex );
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

		private void MenuStripItemEdit_DropDownOpening( object sender, EventArgs e )
		{
			MenuStripItemEditInsertEntryBefore.Enabled = (TextEntriesGridView.SelectedCells.Count > 0);
			MenuStripItemEditRemoveEntry.Enabled = (TextEntriesGridView.SelectedCells.Count > 0);
		}
	}
}
