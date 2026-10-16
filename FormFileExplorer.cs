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
			foreach ( CultureInfo culture in Program.AvailableCultures )
			{
				ToolStripMenuItem languageItem = new ToolStripMenuItem()
				{
					Text = culture.NativeName,
					Tag = culture,
					Checked = CultureInfo.CurrentCulture.Equals( culture )
				};

				MenuStripItemSettingsLanguage.DropDownItems.Add( languageItem );
			}
		}

		public override LocalizedControl[] LocalizedControls
		{
			get
			{
				return [
					new LocalizedControl { Object = GamePathLabel, ObjectName = GamePathLabel.Name, FieldName = "Text" },
					new LocalizedControl { Object = GamePathBrowseButton, ObjectName = GamePathBrowseButton.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFile, ObjectName = MenuStripItemFile.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemFileExit, ObjectName = MenuStripItemFileExit.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemSettings, ObjectName = MenuStripItemSettings.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemSettingsLanguage, ObjectName = MenuStripItemSettingsLanguage.Name, FieldName = "Text" },
					new LocalizedControl { Object = GameFilesExplorerContextMenuItemOpen, ObjectName = GameFilesExplorerContextMenuItemOpen.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemTools, ObjectName = MenuStripItemTools.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsLocalizationEditor, ObjectName = MenuStripItemToolsLocalizationEditor.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsSSGArchiveEditor, ObjectName = MenuStripItemToolsSSGArchiveEditor.Name, FieldName = "Text" },
					new LocalizedControl { Object = MenuStripItemToolsSWB2DatabaseEditor, ObjectName = MenuStripItemToolsSWB2DatabaseEditor.Name, FieldName = "Text" },
				];
			}
		}

		private void LoadGameFolder( string gamePath )
		{
			if ( string.IsNullOrEmpty( gamePath ) )
				return;

			// check if the given path is actually a valid game folder
			DirectoryInfo assetsFolder = new DirectoryInfo( (gamePath + "\\dlc\\pack1") );
			if ( !assetsFolder.Exists )
			{
				string errorMessage = string.Format( Resources.ORCToolKit.FormFileExplorerInvalidGameFolderText, gamePath );
				MessageBox.Show( errorMessage, Resources.ORCToolKit.FormFileExplorerInvalidGameFolderTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			GameFilesExplorer.LoadFolder( gamePath );
			GamePathTextBox.Text = gamePath;
			Properties.Settings.Default.GamePath = gamePath;
		}

		private void OpenExplorerFile( TreeNode selectedNode )
		{
			FileInfo selectedFileInfo = (FileInfo)selectedNode.Tag;
			TypeDeducer.GuessFromFileAndOpen( selectedFileInfo );
		}

		private void GamePathBrowseButton_Click( object sender, EventArgs e )
		{
			using ( FolderBrowserDialog folderDialog = new FolderBrowserDialog() )
			{
				folderDialog.ShowNewFolderButton = false;
				if ( folderDialog.ShowDialog( this ) == DialogResult.OK && !string.IsNullOrWhiteSpace( folderDialog.SelectedPath ) )
					LoadGameFolder( folderDialog.SelectedPath );
			}
		}

		private void MenuStripItemFileExit_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}

		private void MenuStripItemSettingsLanguage_DropDownItemClicked( object sender, ToolStripItemClickedEventArgs e )
		{
			ToolStripMenuItem? selectedLanguageItem = e.ClickedItem as ToolStripMenuItem;
			if ( selectedLanguageItem != null )
			{
				Program.SetCulture( (CultureInfo)selectedLanguageItem.Tag );

				foreach ( ToolStripMenuItem languageItem in MenuStripItemSettingsLanguage.DropDownItems )
					languageItem.Checked = selectedLanguageItem.Equals( languageItem );
			}
		}

		private void GameFilesExplorer_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
		{
			TreeView fileExplorer = (TreeView)sender;

			switch ( e.Button )
			{
				case MouseButtons.Right:
				{
					fileExplorer.SelectedNode = e.Node; // HACK: Windows Forms doesn't keep selected node when right clicking...

					// open context menu
					GameFilesExplorerContextMenu.Tag = fileExplorer.SelectedNode; // remember what node we are going to open
					GameFilesExplorerContextMenu.Show( fileExplorer, e.Location );

					break;
				}
			}
		}

		private void GameFilesExplorerContextMenuItemOpen_Click( object sender, EventArgs e )
		{
			ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
			TreeNode? selectedNode = (TreeNode?)menuItem.GetCurrentParent()?.Tag;
			if ( selectedNode != null )
			{
				if ( selectedNode.Nodes.Count > 0 )
				{
					// folder
					if ( selectedNode.IsExpanded )
						selectedNode.Collapse();
					else
						selectedNode.Expand();
				}
				else
				{
					OpenExplorerFile( selectedNode );
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
			FormLocalizationEditor localizationEditor = new FormLocalizationEditor();
			localizationEditor.Show();
		}

		private void MenuStripItemToolsSSGArchiveEditor_Click( object sender, EventArgs e )
		{
			FormSSGArchiveEditor ssgArchiveEditor = new FormSSGArchiveEditor();
			ssgArchiveEditor.Show();
		}

		private void MenuStripItemToolsSWB2DatabaseEditor_Click( object sender, EventArgs e )
		{
			FormSWB2DatabaseEditor swb2DatabaseEditor = new FormSWB2DatabaseEditor();
			swb2DatabaseEditor.Show();
		}
	}
}
