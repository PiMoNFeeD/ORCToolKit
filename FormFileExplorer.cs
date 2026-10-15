using ORCToolKit.FileTypes;
using System.Globalization;

namespace ORCToolKit
{
	public partial class FormFileExplorer: LocalizedForm
	{
		public FormFileExplorer()
		{
			InitializeComponent();
			LocalizeComponent();

			LoadGameFolder( Properties.Settings.Default.GamePath );

			// fill our languages
			foreach ( CultureInfo Culture in Program.AvailableCultures )
			{
				ToolStripMenuItem LanguageItem = new ToolStripMenuItem()
				{
					Text = Culture.NativeName,
					Tag = Culture,
					Checked = CultureInfo.CurrentCulture.Equals( Culture )
				};

				MenuStripItemSettingsLanguage.DropDownItems.Add( LanguageItem );
			}
		}

		public override LocalizedControl[] LocalizedControls
		{
			get
			{
				return [
					new LocalizedControl { Object = this, ObjectName = this.Name, FieldName = "Text" },
					new LocalizedControl { Object = GamePathLabel, ObjectName = GamePathLabel.Name, FieldName = "Text" },
					new LocalizedControl { Object = GamePathBrowseButton, ObjectName = GamePathBrowseButton.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFile, ObjectName = MenuStripItemFile.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileExit, ObjectName = MenuStripItemFileExit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemSettings, ObjectName = MenuStripItemSettings.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemSettingsLanguage, ObjectName = MenuStripItemSettingsLanguage.Name, FieldName = "Text" },
					new LocalizedControl { Object = GameFileExplorerContextMenuItemOpen, ObjectName = GameFileExplorerContextMenuItemOpen.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemTools, ObjectName = MenuStripItemTools.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsLocalizationEditor, ObjectName = MenuStripItemToolsLocalizationEditor.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsSSGArchiveEditor, ObjectName = MenuStripItemToolsSSGArchiveEditor.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsSWB2DatabaseEditor, ObjectName = MenuStripItemToolsSWB2DatabaseEditor.Name, FieldName = "Text" },
				];
			}
		}

		private void LoadGameFolder( string GamePath )
		{
			if ( string.IsNullOrEmpty( GamePath ) )
				return;

			// check if the given path is actually a valid game folder
			DirectoryInfo AssetsFolder = new DirectoryInfo( (GamePath + "\\dlc\\pack1") );
			if ( !AssetsFolder.Exists )
			{
				string ErrorMessage = string.Format( Resources.ORCToolKit.FormFileExplorerInvalidGameFolderText, GamePath );
				MessageBox.Show( ErrorMessage, Resources.ORCToolKit.FormFileExplorerInvalidGameFolderTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			GameFilesExplorer.LoadFolder( GamePath );
			GamePathTextBox.Text = GamePath;
			Properties.Settings.Default.GamePath = GamePath;
		}

		private void OpenExplorerFile( TreeNode SelectedNode )
		{
			FileInfo SelectedFileInfo = (FileInfo)SelectedNode.Tag;
			TypeDeducer.GuessFromFileAndOpen( SelectedFileInfo );
		}

		private void GamePathBrowseButton_Click( object sender, EventArgs e )
		{
			using ( FolderBrowserDialog FolderDialog = new FolderBrowserDialog() )
			{
				FolderDialog.ShowNewFolderButton = false;
				if ( FolderDialog.ShowDialog( this ) == DialogResult.OK && !string.IsNullOrWhiteSpace( FolderDialog.SelectedPath ) )
					LoadGameFolder( FolderDialog.SelectedPath );
			}
		}

		private void MenuStripItemFileExit_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}

		private void MenuStripItemSettingsLanguage_DropDownItemClicked( object sender, ToolStripItemClickedEventArgs e )
		{
			ToolStripMenuItem? SelectedLanguageItem = e.ClickedItem as ToolStripMenuItem;
			if ( SelectedLanguageItem != null )
			{
				Program.SetCulture( (CultureInfo)SelectedLanguageItem.Tag );

				foreach ( ToolStripMenuItem LanguageItem in MenuStripItemSettingsLanguage.DropDownItems )
					LanguageItem.Checked = SelectedLanguageItem.Equals( LanguageItem );
			}
		}

		private void GameFilesExplorer_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			TreeView FileExplorer = (TreeView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					FileExplorer.SelectedNode = e.Node; // HACK: Windows Forms doesn't keep selected node when right clicking...

					// open context menu
					GameFileExplorerContextMenu.Tag = FileExplorer.SelectedNode; // remember what node we are going to open
					GameFileExplorerContextMenu.Show( FileExplorer, e.Location );

					break;
				}
			}
		}

		private void GameFileExplorerContextMenuItemOpen_Click( object sender, EventArgs e )
		{
			ToolStripMenuItem MenuItem = (ToolStripMenuItem)sender;
			TreeNode? SelectedNode = (TreeNode?)MenuItem.GetCurrentParent()?.Tag;
			if ( SelectedNode != null )
			{
				if ( SelectedNode.Nodes.Count > 0 )
				{
					// folder
					if ( SelectedNode.IsExpanded )
						SelectedNode.Collapse();
					else
						SelectedNode.Expand();
				}
				else
				{
					OpenExplorerFile( SelectedNode );
				}
			}
		}

		private void GameFilesExplorer_NodeMouseDoubleClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			if ( e.Node.Nodes.Count < 1 )
				OpenExplorerFile( e.Node );
		}

		private void MenuStripItemToolsLocalizationEditor_Click( object sender, EventArgs e )
		{
			FormLocalizationEditor LocalizationEditor = new FormLocalizationEditor();
			LocalizationEditor.Show();
		}

		private void MenuStripItemToolsSSGArchiveEditor_Click( object sender, EventArgs e )
		{
			FormSSGArchiveEditor SSGArchiveEditor = new FormSSGArchiveEditor();
			SSGArchiveEditor.Show();
		}

		private void MenuStripItemToolsSWB2DatabaseEditor_Click( object sender, EventArgs e )
		{
			FormSWB2DatabaseEditor SWB2DatabaseEditor = new FormSWB2DatabaseEditor();
			SWB2DatabaseEditor.Show();
		}
	}
}
