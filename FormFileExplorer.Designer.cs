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
			MenuStripItemExit = new ToolStripMenuItem();
			MenuStripItemSettings = new ToolStripMenuItem();
			MenuStripItemLanguage = new ToolStripMenuItem();
			MenuStrip.SuspendLayout();
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
			// 
			// MenuStrip
			// 
			MenuStrip.BackColor = SystemColors.Control;
			MenuStrip.Items.AddRange( new ToolStripItem[] { MenuStripItemFile, MenuStripItemSettings } );
			MenuStrip.Location = new Point( 0, 0 );
			MenuStrip.Name = "MenuStrip";
			MenuStrip.Size = new Size( 784, 24 );
			MenuStrip.TabIndex = 4;
			// 
			// MenuStripItemFile
			// 
			MenuStripItemFile.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemExit } );
			MenuStripItemFile.Name = "MenuStripItemFile";
			MenuStripItemFile.Size = new Size( 37, 20 );
			MenuStripItemFile.Text = "File";
			// 
			// MenuStripItemExit
			// 
			MenuStripItemExit.Name = "MenuStripItemExit";
			MenuStripItemExit.Size = new Size( 93, 22 );
			MenuStripItemExit.Text = "Exit";
			MenuStripItemExit.Click += MenuStripItemExit_Click;
			// 
			// MenuStripItemSettings
			// 
			MenuStripItemSettings.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemLanguage } );
			MenuStripItemSettings.Name = "MenuStripItemSettings";
			MenuStripItemSettings.Size = new Size( 61, 20 );
			MenuStripItemSettings.Text = "Settings";
			// 
			// MenuStripItemLanguage
			// 
			MenuStripItemLanguage.Name = "MenuStripItemLanguage";
			MenuStripItemLanguage.Size = new Size( 126, 22 );
			MenuStripItemLanguage.Text = "Language";
			MenuStripItemLanguage.DropDownItemClicked += MenuStripItemLanguage_DropDownItemClicked;
			// 
			// FormFileExplorer
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size( 784, 561 );
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
		private ToolStripMenuItem MenuStripItemExit;
		private ToolStripMenuItem MenuStripItemSettings;
		private ToolStripMenuItem MenuStripItemLanguage;
	}
}
