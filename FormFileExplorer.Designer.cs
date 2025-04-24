using System.Resources;
using System.Runtime.Versioning;

namespace ORCToolKit
{
	partial class FormFileExplorer
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FormFileExplorer ) );
			GamePathLabel = new Label();
			GamePathTextBox = new TextBox();
			GamePathBrowseButton = new Button();
			GameFilesExplorer = new TreeViewFileExplorer();
			MenuStrip = new MenuStrip();
			MenuStripItemFile = new ToolStripMenuItem();
			MenuStripItemFileExit = new ToolStripMenuItem();
			MenuStripItemSettings = new ToolStripMenuItem();
			MenuStripItemSettingsLanguage = new ToolStripMenuItem();
			GameFileExplorerContextMenu = new ContextMenuStrip( components );
			GameFileExplorerContextMenuItemOpen = new ToolStripMenuItem();
			MenuStripItemTools = new ToolStripMenuItem();
			MenuStripItemToolsLocalizationEditor = new ToolStripMenuItem();
			MenuStrip.SuspendLayout();
			GameFileExplorerContextMenu.SuspendLayout();
			SuspendLayout();
			// 
			// GamePathLabel
			// 
			GamePathLabel.AutoSize = true;
			GamePathLabel.Location = new Point( 12, 24 );
			GamePathLabel.Name = "GamePathLabel";
			GamePathLabel.Size = new Size( 115, 15 );
			GamePathLabel.TabIndex = 0;
			GamePathLabel.Text = "Path to game folder:";
			// 
			// GamePathTextBox
			// 
			GamePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GamePathTextBox.Location = new Point( 12, 42 );
			GamePathTextBox.Name = "GamePathTextBox";
			GamePathTextBox.ReadOnly = true;
			GamePathTextBox.Size = new Size( 679, 23 );
			GamePathTextBox.TabIndex = 1;
			// 
			// GamePathBrowseButton
			// 
			GamePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			GamePathBrowseButton.Location = new Point( 697, 42 );
			GamePathBrowseButton.Name = "GamePathBrowseButton";
			GamePathBrowseButton.RightToLeft = RightToLeft.No;
			GamePathBrowseButton.Size = new Size( 75, 23 );
			GamePathBrowseButton.TabIndex = 2;
			GamePathBrowseButton.Text = "Browse";
			GamePathBrowseButton.UseVisualStyleBackColor = true;
			GamePathBrowseButton.Click += GamePathBrowseButton_Click;
			// 
			// GameFilesExplorer
			// 
			GameFilesExplorer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			GameFilesExplorer.ImageIndex = 0;
			GameFilesExplorer.Location = new Point( 12, 71 );
			GameFilesExplorer.Name = "GameFilesExplorer";
			GameFilesExplorer.SelectedImageIndex = 0;
			GameFilesExplorer.Size = new Size( 760, 478 );
			GameFilesExplorer.TabIndex = 3;
			GameFilesExplorer.NodeMouseClick += GameFilesExplorer_NodeMouseClick;
			GameFilesExplorer.NodeMouseDoubleClick += GameFilesExplorer_NodeMouseDoubleClick;
			// 
			// MenuStrip
			// 
			MenuStrip.BackColor = SystemColors.Control;
			MenuStrip.Items.AddRange( new ToolStripItem[] { MenuStripItemFile, MenuStripItemTools, MenuStripItemSettings } );
			MenuStrip.Location = new Point( 0, 0 );
			MenuStrip.Name = "MenuStrip";
			MenuStrip.Size = new Size( 784, 24 );
			MenuStrip.TabIndex = 4;
			// 
			// MenuStripItemFile
			// 
			MenuStripItemFile.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemFileExit } );
			MenuStripItemFile.Name = "MenuStripItemFile";
			MenuStripItemFile.Size = new Size( 37, 20 );
			MenuStripItemFile.Text = "File";
			// 
			// MenuStripItemFileExit
			// 
			MenuStripItemFileExit.Name = "MenuStripItemFileExit";
			MenuStripItemFileExit.Size = new Size( 93, 22 );
			MenuStripItemFileExit.Text = "Exit";
			MenuStripItemFileExit.Click += MenuStripItemFileExit_Click;
			// 
			// MenuStripItemSettings
			// 
			MenuStripItemSettings.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemSettingsLanguage } );
			MenuStripItemSettings.Name = "MenuStripItemSettings";
			MenuStripItemSettings.Size = new Size( 61, 20 );
			MenuStripItemSettings.Text = "Settings";
			// 
			// MenuStripItemSettingsLanguage
			// 
			MenuStripItemSettingsLanguage.Name = "MenuStripItemSettingsLanguage";
			MenuStripItemSettingsLanguage.Size = new Size( 180, 22 );
			MenuStripItemSettingsLanguage.Text = "Language";
			MenuStripItemSettingsLanguage.DropDownItemClicked += MenuStripItemSettingsLanguage_DropDownItemClicked;
			// 
			// GameFileExplorerContextMenu
			// 
			GameFileExplorerContextMenu.Items.AddRange( new ToolStripItem[] { GameFileExplorerContextMenuItemOpen } );
			GameFileExplorerContextMenu.Name = "GameFileExplorerContextMenu";
			GameFileExplorerContextMenu.Size = new Size( 104, 26 );
			// 
			// GameFileExplorerContextMenuItemOpen
			// 
			GameFileExplorerContextMenuItemOpen.Name = "GameFileExplorerContextMenuItemOpen";
			GameFileExplorerContextMenuItemOpen.Size = new Size( 103, 22 );
			GameFileExplorerContextMenuItemOpen.Text = "Open";
			GameFileExplorerContextMenuItemOpen.Click += GameFileExplorerContextMenuItemOpen_Click;
			// 
			// MenuStripItemTools
			// 
			MenuStripItemTools.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemToolsLocalizationEditor } );
			MenuStripItemTools.Name = "MenuStripItemTools";
			MenuStripItemTools.Size = new Size( 46, 20 );
			MenuStripItemTools.Text = "Tools";
			// 
			// MenuStripItemToolsLocalizationEditor
			// 
			MenuStripItemToolsLocalizationEditor.Name = "MenuStripItemToolsLocalizationEditor";
			MenuStripItemToolsLocalizationEditor.Size = new Size( 180, 22 );
			MenuStripItemToolsLocalizationEditor.Text = "Localization Editor";
			MenuStripItemToolsLocalizationEditor.Click += MenuStripItemToolsLocalizationEditor_Click;
			// 
			// FormFileExplorer
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size( 784, 561 );
			ContextMenuStrip = GameFileExplorerContextMenu;
			Controls.Add( GameFilesExplorer );
			Controls.Add( GamePathBrowseButton );
			Controls.Add( GamePathTextBox );
			Controls.Add( GamePathLabel );
			Controls.Add( MenuStrip );
			Icon = (Icon)resources.GetObject( "$this.Icon" );
			MainMenuStrip = MenuStrip;
			MinimumSize = new Size( 640, 480 );
			Name = "FormFileExplorer";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "ORC ToolKit";
			WindowState = FormWindowState.Maximized;
			MenuStrip.ResumeLayout( false );
			MenuStrip.PerformLayout();
			GameFileExplorerContextMenu.ResumeLayout( false );
			ResumeLayout( false );
			PerformLayout();
		}

		#endregion

		private Label GamePathLabel;
		private TextBox GamePathTextBox;
		private Button GamePathBrowseButton;
		private TreeViewFileExplorer GameFilesExplorer;
		private MenuStrip MenuStrip;
		private ToolStripMenuItem MenuStripItemFile;
		private ToolStripMenuItem MenuStripItemFileExit;
		private ToolStripMenuItem MenuStripItemSettings;
		private ToolStripMenuItem MenuStripItemSettingsLanguage;
		private ContextMenuStrip GameFileExplorerContextMenu;
		private ToolStripMenuItem GameFileExplorerContextMenuItemOpen;
		private ToolStripMenuItem MenuStripItemTools;
		private ToolStripMenuItem MenuStripItemToolsLocalizationEditor;
	}
}
