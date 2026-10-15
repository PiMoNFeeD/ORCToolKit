using ORCToolKit.Extensions;
using ORCToolKit.Search;
using System.Diagnostics;

namespace ORCToolKit
{
	public partial class FormSSGArchiveEditor: FormEditor
	{
		private FileTypes.SSGArchiveFile _file = new FileTypes.SSGArchiveFile();
		private Int32 LastOpenedSection = -1;
		override protected FileTypes.IFileType? CurrentFile { get { return _file; } }
		override protected string FileDialogFilter { get { return "SSG Archives (*.ssg)|*.ssg"; } }

		public FormSSGArchiveEditor()
		{
			InitializeComponent();
			LocalizeComponent();

			Shown += FormSSGArchiveEditor_Shown;
			OnNewFile += FormSSGArchiveEditor_OnNewFile;
			OnOpenFile += FormSSGArchiveEditor_OnOpenFile;
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

		private void FormSSGArchiveEditor_Shown( Object? sender, EventArgs e )
		{
			// keep in sync with FormSSGArchiveEditor_OnEntriesUpdated
			MenuStripItemFileSave.Enabled = false;
			MenuStripItemEditSearch.Enabled = (_file.DataSections.Count > 0);
			MenuStripItemEditSearchForward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			MenuStripItemEditSearchBackward.Enabled = (_file.DataSections.Count > 0) && !string.IsNullOrEmpty( LastSearchString );
			StatusStripEntriesLabel.Text = string.Format( Resources.ORCToolKit.NumberOfEntries, _file.DataSections.Count );
		}

		private void FormSSGArchiveEditor_OnNewFile( Object? sender, EventArgs e )
		{
			LastOpenedSection = -1;
			FilesListView.Items.Clear();
		}

		private void FormSSGArchiveEditor_OnOpenFile( object? sender, EventArgs e )
		{
			LastOpenedSection = -1;
			FilesListView.Items.Clear();
			foreach ( FileTypes.SSGArchiveFile.DataSection DataSection in _file.DataSections )
			{
				ListViewItem ListItem = new ListViewItem();
				ListItem.Text = DataSection.Name;
				string[] SubItemsRow = { DataSection.Type.ToString(),
										 string.Format( Resources.ORCToolKit.SizeInBytes, DataSection.RawData.Length ),
										 DataSection.IsCompressed ? Resources.ORCToolKit.Yes : Resources.ORCToolKit.No };
				ListItem.SubItems.AddRange( SubItemsRow );
				FilesListView.Items.Add( ListItem );
			}
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

		override protected bool TrySearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			MenuStripItemEditSearchForward.Enabled = true;
			MenuStripItemEditSearchBackward.Enabled = true;

			switch ( Direction )
			{
				case SearchDirection.Forward:
				{
					for ( int i = FilesListView.SelectedIndices.Count != 0 ? FilesListView.SelectedIndices[FilesListView.SelectedItems.Count - 1] + 1 : 0;
						i < _file.DataSections.Count; i++ )
					{
						if ( StringSearch.Compare( _file.DataSections[i].Name, SearchString, WholeWord, CaseSensitive ) )
						{
							foreach ( ListViewItem Item in FilesListView.Items )
							{
								Item.Focused = (Item.Index == i);
								Item.Selected = (Item.Index == i);
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
						if ( StringSearch.Compare( _file.DataSections[i].Name, SearchString, WholeWord, CaseSensitive ) )
						{
							foreach ( ListViewItem Item in FilesListView.Items )
							{
								Item.Focused = (Item.Index == i);
								Item.Selected = (Item.Index == i);
							}
							return true;
						}
					}

					break;
				}
			}

			return false;
		}

		override protected void SaveChild( FileTypes.IFileType? File )
		{
			if ( File == null )
				return;

			Debug.Assert( LastOpenedSection != -1 );
			if ( LastOpenedSection != -1 )
			{
				// TODO: swap RawData for a memory stream to save directly into it, without a middleman stream
				using ( MemoryStream FileStream = new MemoryStream() )
				{
					File.SaveToBuffer( FileStream );

					_file.DataSections[LastOpenedSection].RawData = FileStream.ToArray();
					FilesListView.Items[LastOpenedSection].SubItems[2].Text = FileStream.Length.ToString(); // cursed
					MessageBox.Show( Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveText, Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveTitle, MessageBoxButtons.OK, MessageBoxIcon.Information );
				}
			}
			base.SaveChild( File );
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
				LastOpenedSection = FilesListView.SelectedIndices[0];
				using ( MemoryStream FileStream = new MemoryStream( _file.DataSections[LastOpenedSection].RawData ) )
				{
					FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[LastOpenedSection].Name, FileStream, this );
				}
			}
		}

		private void FilesListView_MouseClick( object sender, MouseEventArgs e )
		{
			ListView FilesList = (ListView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					if ( FilesList.SelectedIndices.Count > 0 )
					{
						// open context menu
						FilesListViewContextMenu.Tag = FilesList.SelectedItems;
						FilesListViewContextMenu.Show( FilesList, e.Location );
					}

					break;
				}
			}
		}

		private void FilesListView_AfterLabelEdit( object sender, LabelEditEventArgs e )
		{
			if ( e.Label != null )
			{
				foreach ( ListViewItem Item in FilesListView.SelectedItems )
				{
					_file.DataSections[Item.Index].Name = e.Label;
					Item.Text = e.Label;
				}
			}
		}

		private void FilesListView_KeyDown( object sender, KeyEventArgs e )
		{
			ListView FilesList = (ListView)sender;
			switch ( e.KeyData )
			{
				case Keys.F2:
				{
					if ( FilesList.SelectedItems.Count > 0 )
						FilesList.SelectedItems[0].BeginEdit();
					break;
				}
				case Keys.Enter:
				{
					if ( FilesList.SelectedItems.Count > 0 )
					{
						LastOpenedSection = FilesList.SelectedItems[0].Index;
						using ( MemoryStream FileStream = new MemoryStream( _file.DataSections[LastOpenedSection].RawData, false ) )
						{
							FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[LastOpenedSection].Name, FileStream, this );
						}
					}
					break;
				}
			}
		}

		private void FilesListViewContextMenuItemOpen_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? SelectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( SelectedItems == null )
				return;

			if ( SelectedItems.Count > 0 )
			{
				LastOpenedSection = SelectedItems[0].Index;
				using ( MemoryStream FileStream = new MemoryStream( _file.DataSections[LastOpenedSection].RawData, false ) )
				{
					FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[LastOpenedSection].Name, FileStream, this );
				}
			}
		}

		private void FilesListViewContextMenuItemExtract_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? SelectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( SelectedItems == null )
				return;

			if ( SelectedItems.Count == 1 )
			{
				using ( SaveFileDialog FileDialog = new SaveFileDialog() )
				{
					FileDialog.FileName = Path.GetFileName( _file.DataSections[SelectedItems[0].Index].Name );
					FileDialog.RestoreDirectory = true;
					if ( FileDialog.ShowDialog() == DialogResult.OK )
					{
						using ( FileStream File = new FileStream( FileDialog.FileName, FileMode.Create, FileAccess.Write ) )
						{
							File.Write( _file.DataSections[SelectedItems[0].Index].RawData );
						}
					}
				}
			}
			else if ( SelectedItems.Count > 1 )
			{
				using ( FolderBrowserDialog FolderDialog = new FolderBrowserDialog() )
				{
					if ( FolderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace( FolderDialog.SelectedPath ) )
					{
						foreach ( ListViewItem SelectedItem in SelectedItems )
						{
							string FilePath = FolderDialog.SelectedPath + "\\" + _file.DataSections[SelectedItem.Index].Name;
							DirectoryInfo FileDirectory = new DirectoryInfo( Path.GetDirectoryName( FilePath ) );
							if ( !FileDirectory.Exists )
								FileDirectory.Create();

							using ( FileStream File = new FileStream( FilePath.NormalizeSlashes(), FileMode.Create, FileAccess.Write ) )
							{
								File.Write( _file.DataSections[SelectedItem.Index].RawData );
							}
						}
					}
				}
			}
		}

		private void FilesListViewContextMenuItemReplace_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? SelectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( SelectedItems == null )
				return;

			if ( SelectedItems.Count > 0 )
			{
				using ( OpenFileDialog FileDialog = new OpenFileDialog() )
				{
					FileDialog.RestoreDirectory = true;
					FileDialog.Multiselect = false;
					if ( FileDialog.ShowDialog() == DialogResult.OK )
					{
						byte[] FileData = Array.Empty<byte>();
						using ( FileStream File = new FileStream( FileDialog.FileName, FileMode.Open, FileAccess.Read ) )
						{
							using ( BinaryReader Reader = new BinaryReader( File ) )
							{
								FileData = Reader.ReadBytes( Convert.ToInt32( File.Length ) );
							}
						}

						foreach ( ListViewItem SelectedItem in SelectedItems )
						{
							_file.DataSections[SelectedItem.Index].RawData = FileData;
							SelectedItem.SubItems[2].Text = FileData.Length.ToString(); // cursed
						}
					}
				}
			}
		}

		private void FilesListViewContextMenuItemRename_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? SelectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( SelectedItems == null )
				return;

			if ( SelectedItems.Count > 0 )
				FilesListView.Items[SelectedItems[0].Index].BeginEdit();
		}

		private void FilesListViewContextMenuItemDelete_Click( object sender, EventArgs e )
		{
			ListView.SelectedListViewItemCollection? SelectedItems = FilesListViewContextMenu.Tag as ListView.SelectedListViewItemCollection;
			if ( SelectedItems == null )
				return;

			foreach ( ListViewItem SelectedItem in SelectedItems )
			{
				_file.DataSections.RemoveAt( SelectedItem.Index );
				FilesListView.Items.RemoveAt( SelectedItem.Index );
			}
			EntriesUpdated();
		}
	}
}
