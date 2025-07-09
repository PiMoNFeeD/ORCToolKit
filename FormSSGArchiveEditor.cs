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
			Program.ReloadFormResources( this );

			OnOpenFile += FormSSGArchiveEditor_OnOpenFile;
			OnEntriesUpdated += FormSSGArchiveEditor_OnEntriesUpdated;
		}

		private void FormSSGArchiveEditor_OnOpenFile( object sender, EventArgs e )
		{
			LastOpenedSection = -1;
			FilesListView.Items.Clear();
			for ( int i = 0; i < _file.DataSections.Count; i++ )
			{
				ListViewItem ListItem = new ListViewItem();
				ListItem.Text = _file.DataSections[i].Name;
				string[] SubItemsRow = { _file.DataSections[i].Type.ToString(),
										 string.Format( Resources.ORCToolKit.SizeInBytes, _file.DataSections[i].RawData.Length ),
										 _file.DataSections[i].IsCompressed ? Resources.ORCToolKit.Yes : Resources.ORCToolKit.No };
				ListItem.SubItems.AddRange( SubItemsRow );
				FilesListView.Items.Add( ListItem );
			}
		}

		private void FormSSGArchiveEditor_OnEntriesUpdated( object sender, EventArgs e )
		{
			MenuStripItemFileSave.Enabled = (_file.DataSections.Count > 0);
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
						if ( SearchCompareString( _file.DataSections[i].Name, SearchString, WholeWord, CaseSensitive ) )
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
						if ( SearchCompareString( _file.DataSections[i].Name, SearchString, WholeWord, CaseSensitive ) )
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
					HasUnsavedEdits = false;
					File.SaveToBuffer( FileStream );

					_file.DataSections[LastOpenedSection].RawData = FileStream.ToArray();
					FilesListView.Items[LastOpenedSection].SubItems[2].Text = FileStream.Length.ToString(); // cursed
					MessageBox.Show( Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveText, Resources.ORCToolKit.FileSSGArchiveEmbeddedSaveTitle, MessageBoxButtons.OK, MessageBoxIcon.Information );
				}
			}
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
						Int32 FileIndex = FilesList.SelectedIndices[0];

						// open context menu
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
			if ( FilesListView.SelectedItems.Count > 0 )
			{
				LastOpenedSection = FilesListView.SelectedItems[0].Index;
				using ( MemoryStream FileStream = new MemoryStream( _file.DataSections[LastOpenedSection].RawData, false ) )
				{
					FileTypes.TypeDeducer.GuessFromBufferAndOpen( _file.DataSections[LastOpenedSection].Name, FileStream, this );
				}
			}
		}

		private void FilesListViewContextMenuItemExtract_Click( object sender, EventArgs e )
		{
			if ( FilesListView.SelectedItems.Count == 1 )
			{
				using ( SaveFileDialog FileDialog = new SaveFileDialog() )
				{
					FileDialog.FileName = Path.GetFileName( _file.DataSections[FilesListView.SelectedItems[0].Index].Name );
					FileDialog.RestoreDirectory = true;
					if ( FileDialog.ShowDialog() == DialogResult.OK )
					{
						using ( FileStream File = new FileStream( FileDialog.FileName, FileMode.Create, FileAccess.Write ) )
						{
							File.Write( _file.DataSections[FilesListView.SelectedItems[0].Index].RawData );
						}
					}
				}
			}
			else if ( FilesListView.SelectedItems.Count > 1 )
			{
				using ( FolderBrowserDialog FolderDialog = new FolderBrowserDialog() )
				{
					if ( FolderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace( FolderDialog.SelectedPath ) )
					{
						foreach ( ListViewItem SelectedItem in FilesListView.SelectedItems )
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
			if ( FilesListView.SelectedItems.Count > 0 )
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

						foreach ( ListViewItem SelectedItem in FilesListView.SelectedItems )
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
			if ( FilesListView.SelectedItems.Count > 0 )
				FilesListView.Items[FilesListView.SelectedItems[0].Index].BeginEdit();
		}

		private void FilesListViewContextMenuItemDelete_Click( object sender, EventArgs e )
		{
			foreach ( ListViewItem SelectedItem in FilesListView.SelectedItems )
			{
				_file.DataSections.RemoveAt( SelectedItem.Index );
				FilesListView.Items.RemoveAt( SelectedItem.Index );
			}
			EntriesUpdated();
		}
	}
}
