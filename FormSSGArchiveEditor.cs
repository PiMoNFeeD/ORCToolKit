using ORCToolKit.Extensions;
using ORCToolKit.Search;
using System.Diagnostics;

namespace ORCToolKit
{
	public partial class FormSSGArchiveEditor: FormEditor
	{
		private FileTypes.SSGArchiveFile _file = new FileTypes.SSGArchiveFile();
		private Int32 _lastOpenedSection = -1;

		override protected FileTypes.IFileType? CurrentFile { get { return _file; } }
		override protected string FileDialogFilter { get { return "SSG Archives (*.ssg)|*.ssg"; } }

		public FormSSGArchiveEditor()
		{
			InitializeComponent();
			LocalizeComponent();

			Shown += FormSSGArchiveEditor_Shown;
			OnNewFile += FormSSGArchiveEditor_OnNewFile;
			OnOpenFile += FormSSGArchiveEditor_OnOpenFile;
			OnSaveFile += FormSSGArchiveEditor_OnSaveFile;
			OnEntriesUpdated += FormSSGArchiveEditor_OnEntriesUpdated;
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
					new LocalizedControl { Object = MenuStripItemFileExit, ObjectName = MenuStripItemFileExit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearch, ObjectName = MenuStripItemEditSearch.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchForward, ObjectName = MenuStripItemEditSearchForward.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemEditSearchBackward, ObjectName = MenuStripItemEditSearchBackward.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewHeaderName, ObjectName = FilesListViewHeaderName.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewHeaderType, ObjectName = FilesListViewHeaderType.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewContextMenuItemOpen, ObjectName = FilesListViewContextMenuItemOpen.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewContextMenuItemExtract, ObjectName = FilesListViewContextMenuItemExtract.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewContextMenuItemReplace, ObjectName = FilesListViewContextMenuItemReplace.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewContextMenuItemRename, ObjectName = FilesListViewContextMenuItemRename.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewContextMenuItemDelete, ObjectName = FilesListViewContextMenuItemDelete.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewHeaderUncompressedSize, ObjectName = FilesListViewHeaderUncompressedSize.Name, FieldName = "Text" },
					new LocalizedControl { Object = FilesListViewHeaderIsCompressed, ObjectName = FilesListViewHeaderIsCompressed.Name, FieldName = "Text" },
				];
			}
		}

		private void FormSSGArchiveEditor_Shown( object? sender, EventArgs e )
		{
			// keep in sync with FormSSGArchiveEditor_OnEntriesUpdated
			MenuStripItemFileSave.Enabled = false;
			MenuStripItemEditSearch.Enabled = (_file.DataSections.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.DataSections.Count );
		}

		private void FormSSGArchiveEditor_OnNewFile( object? sender, EventArgs e )
		{
			_lastOpenedSection = -1;
			FilesListView.Items.Clear();
		}

		private void FormSSGArchiveEditor_OnOpenFile( object? sender, EventArgs e )
		{
			_lastOpenedSection = -1;
			FilesListView.Items.Clear();
			foreach ( FileTypes.SSGArchiveFile.DataSection dataSection in _file.DataSections )
			{
				ListViewItem listItem = new ListViewItem();
				listItem.Text = dataSection.Name;
				string[] subItemsRow = { dataSection.Type.ToString(),
										 string.Format( Resources.ORCToolKit.SizeInBytes, dataSection.RawData.Length ),
										 dataSection.IsCompressed ? Resources.ORCToolKit.Yes : Resources.ORCToolKit.No };
				listItem.SubItems.AddRange( subItemsRow );
				FilesListView.Items.Add( listItem );
			}
		}

		private void FormSSGArchiveEditor_OnSaveFile( Object? sender, EventArgs e )
		{
			MenuStripItemFileSave.Enabled = false;
		}

		private void FormSSGArchiveEditor_OnEntriesUpdated( object? sender, EntriesUpdatedEventArgs e )
		{
			// keep in sync with FormSSGArchiveEditor_Shown
			MenuStripItemFileSave.Enabled = e.HasUnsavedEdits;
			MenuStripItemEditSearch.Enabled = (_file.DataSections.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.DataSections.Count );
		}

		override protected bool TrySearch( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
		{
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			switch ( direction )
			{
				case SearchDirection.Forward:
				{
					for ( int i = FilesListView.SelectedIndices.Count != 0 ? FilesListView.SelectedIndices[FilesListView.SelectedItems.Count - 1] + 1 : 0;
						i < _file.DataSections.Count; i++ )
					{
						if ( StringSearch.Compare( _file.DataSections[i].Name, searchString, wholeWord, caseSensitive ) )
						{
							foreach ( ListViewItem item in FilesListView.Items )
							{
								item.Focused = (item.Index == i);
								item.Selected = (item.Index == i);
							}
							return true;
						}
					}

					break;
				}
				case SearchDirection.Backward:
				{
					for ( int i = FilesListView.SelectedIndices.Count != 0 ? FilesListView.SelectedIndices[0] - 1 : _file.DataSections.Count - 1;
						i >= 0; i-- )
					{
						if ( StringSearch.Compare( _file.DataSections[i].Name, searchString, wholeWord, caseSensitive ) )
						{
							foreach ( ListViewItem item in FilesListView.Items )
							{
								item.Focused = (item.Index == i);
								item.Selected = (item.Index == i);
							}
							return true;
						}
					}

					break;
				}
			}

			return false;
		}

		override protected void SaveChild( FileTypes.IFileType? file )
		{
			if ( file == null )
				return;

			Debug.Assert( _lastOpenedSection != -1 );
			if ( _lastOpenedSection != -1 )
			{
				// TODO: swap RawData for a memory stream to save directly into it, without a middleman stream
				using ( MemoryStream fileStream = new MemoryStream() )
				{
					file.SaveToBuffer( fileStream );

					_file.DataSections[_lastOpenedSection].RawData = fileStream.ToArray();
					FilesListView.Items[_lastOpenedSection].SubItems[2].Text = fileStream.Length.ToString(); // cursed
					MessageBox.Show( Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveText, Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveTitle, MessageBoxButtons.OK, MessageBoxIcon.Information );
				}
			}
			base.SaveChild( file );
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

		private void FilesListView_DoubleClick( object sender, EventArgs e )
		{
			if ( FilesListView.SelectedIndices.Count > 0 )
			{
				_lastOpenedSection = FilesListView.SelectedIndices[0];
				using ( MemoryStream fileStream = new MemoryStream( _file.DataSections[_lastOpenedSection].RawData ) )
				{
					FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[_lastOpenedSection].Name, fileStream, this );
				}
			}
		}

		private void FilesListView_MouseClick( object sender, MouseEventArgs e )
		{
			ListView filesList = (ListView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					if ( filesList.SelectedIndices.Count > 0 )
					{
						// open context menu
						FilesListViewContextMenu.Tag = filesList.SelectedItems;
						FilesListViewContextMenu.Show( filesList, e.Location );
					}

					break;
				}
			}
		}

		private void FilesListView_AfterLabelEdit( object sender, LabelEditEventArgs e )
		{
			if ( e.Label != null )
			{
				foreach ( ListViewItem item in FilesListView.SelectedItems )
				{
					_file.DataSections[item.Index].Name = e.Label;
					item.Text = e.Label;
				}
			}
		}

		private void FilesListView_KeyDown( object sender, KeyEventArgs e )
		{
			ListView filesList = (ListView)sender;
			switch ( e.KeyData )
			{
				case Keys.F2:
				{
					if ( filesList.SelectedItems.Count > 0 )
						filesList.SelectedItems[0].BeginEdit();
					break;
				}
				case Keys.Enter:
				{
					if ( filesList.SelectedItems.Count > 0 )
					{
						_lastOpenedSection = filesList.SelectedItems[0].Index;
						using ( MemoryStream fileStream = new MemoryStream( _file.DataSections[_lastOpenedSection].RawData, false ) )
						{
							FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[_lastOpenedSection].Name, fileStream, this );
						}
					}
					break;
				}
			}
		}

		private void FilesListViewContextMenuItemOpen_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? selectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( selectedItems == null )
				return;

			if ( selectedItems.Count > 0 )
			{
				_lastOpenedSection = selectedItems[0].Index;
				using ( MemoryStream fileStream = new MemoryStream( _file.DataSections[_lastOpenedSection].RawData, false ) )
				{
					FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[_lastOpenedSection].Name, fileStream, this );
				}
			}
		}

		private void FilesListViewContextMenuItemExtract_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? selectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( selectedItems == null )
				return;

			if ( selectedItems.Count == 1 )
			{
				using ( SaveFileDialog fileDialog = new SaveFileDialog() )
				{
					fileDialog.FileName = Path.GetFileName( _file.DataSections[selectedItems[0].Index].Name );
					fileDialog.RestoreDirectory = true;
					if ( fileDialog.ShowDialog() == DialogResult.OK )
					{
						using ( FileStream file = new FileStream( fileDialog.FileName, FileMode.Create, FileAccess.Write ) )
						{
							file.Write( _file.DataSections[selectedItems[0].Index].RawData );
						}
					}
				}
			}
			else if ( selectedItems.Count > 1 )
			{
				using ( FolderBrowserDialog folderDialog = new FolderBrowserDialog() )
				{
					if ( folderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace( folderDialog.SelectedPath ) )
					{
						foreach ( ListViewItem selectedItem in selectedItems )
						{
							string filePath = folderDialog.SelectedPath + "\\" + _file.DataSections[selectedItem.Index].Name;
							DirectoryInfo fileDirectory = new DirectoryInfo( Path.GetDirectoryName( filePath ) );
							if ( !fileDirectory.Exists )
								fileDirectory.Create();

							using ( FileStream file = new FileStream( filePath.NormalizeSlashes(), FileMode.Create, FileAccess.Write ) )
							{
								file.Write( _file.DataSections[selectedItem.Index].RawData );
							}
						}
					}
				}
			}
		}

		private void FilesListViewContextMenuItemReplace_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? selectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( selectedItems == null )
				return;

			if ( selectedItems.Count > 0 )
			{
				using ( OpenFileDialog fileDialog = new OpenFileDialog() )
				{
					fileDialog.RestoreDirectory = true;
					fileDialog.Multiselect = false;
					if ( fileDialog.ShowDialog() == DialogResult.OK )
					{
						byte[] fileData = Array.Empty<byte>();
						using ( FileStream file = new FileStream( fileDialog.FileName, FileMode.Open, FileAccess.Read ) )
						{
							using ( BinaryReader reader = new BinaryReader( file ) )
							{
								fileData = reader.ReadBytes( Convert.ToInt32( file.Length ) );
							}
						}

						foreach ( ListViewItem selectedItem in selectedItems )
						{
							_file.DataSections[selectedItem.Index].RawData = fileData;
							selectedItem.SubItems[2].Text = fileData.Length.ToString(); // cursed
						}
					}
				}
			}
		}

		private void FilesListViewContextMenuItemRename_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? selectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( selectedItems == null )
				return;

			if ( selectedItems.Count > 0 )
				FilesListView.Items[selectedItems[0].Index].BeginEdit();
		}

		private void FilesListViewContextMenuItemDelete_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? selectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( selectedItems == null )
				return;

			foreach ( ListViewItem selectedItem in selectedItems )
			{
				_file.DataSections.RemoveAt( selectedItem.Index );
				FilesListView.Items.RemoveAt( selectedItem.Index );
			}
			EntriesUpdated();
		}
	}
}
