namespace ORCToolKit
{
	partial class FormSSGArchiveEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FormSSGArchiveEditor ) );
			MenuStripItemFile = new ToolStripMenuItem();
			MenuStripItemFileNew = new ToolStripMenuItem();
			MenuStripItemFileOpen = new ToolStripMenuItem();
			MenuStripItemFileSave = new ToolStripMenuItem();
			MenuStripItemFileSeparator1 = new ToolStripSeparator();
			MenuStripItemFileExit = new ToolStripMenuItem();
			StatusStrip = new StatusStrip();
			StatusStripEntriesLabel = new ToolStripStatusLabel();
			MenuStrip = new MenuStrip();
			MenuStripItemEdit = new ToolStripMenuItem();
			MenuStripItemEditSearch = new ToolStripMenuItem();
			MenuStripItemEditSearchForward = new ToolStripMenuItem();
			MenuStripItemEditSearchBackward = new ToolStripMenuItem();
			FilesListView = new ListView();
			FilesListViewHeaderName = new ColumnHeader();
			FilesListViewHeaderType = new ColumnHeader();
			FilesListViewHeaderUncompressedSize = new ColumnHeader();
			FilesListViewHeaderIsCompressed = new ColumnHeader();
			FilesListViewContextMenu = new ContextMenuStrip( components );
			FilesListViewContextMenuItemOpen = new ToolStripMenuItem();
			FilesListViewContextMenuItemExtract = new ToolStripMenuItem();
			FilesListViewContextMenuItemReplace = new ToolStripMenuItem();
			FilesListViewContextMenuItemRename = new ToolStripMenuItem();
			FilesListViewContextMenuItemDelete = new ToolStripMenuItem();
			StatusStrip.SuspendLayout();
			MenuStrip.SuspendLayout();
			FilesListViewContextMenu.SuspendLayout();
			SuspendLayout();
			// 
			// MenuStripItemFile
			// 
			MenuStripItemFile.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemFileNew, MenuStripItemFileOpen, MenuStripItemFileSave, MenuStripItemFileSeparator1, MenuStripItemFileExit } );
			MenuStripItemFile.Name = "MenuStripItemFile";
			MenuStripItemFile.Size = new Size( 37, 20 );
			MenuStripItemFile.Text = "File";
			// 
			// MenuStripItemFileNew
			// 
			MenuStripItemFileNew.Name = "MenuStripItemFileNew";
			MenuStripItemFileNew.ShortcutKeys = Keys.Control | Keys.N;
			MenuStripItemFileNew.Size = new Size( 146, 22 );
			MenuStripItemFileNew.Text = "New";
			MenuStripItemFileNew.Click += MenuStripItemFileNew_Click;
			// 
			// MenuStripItemFileOpen
			// 
			MenuStripItemFileOpen.Name = "MenuStripItemFileOpen";
			MenuStripItemFileOpen.ShortcutKeys = Keys.Control | Keys.O;
			MenuStripItemFileOpen.Size = new Size( 146, 22 );
			MenuStripItemFileOpen.Text = "Open";
			MenuStripItemFileOpen.Click += MenuStripItemFileOpen_Click;
			// 
			// MenuStripItemFileSave
			// 
			MenuStripItemFileSave.Name = "MenuStripItemFileSave";
			MenuStripItemFileSave.ShortcutKeys = Keys.Control | Keys.S;
			MenuStripItemFileSave.Size = new Size( 146, 22 );
			MenuStripItemFileSave.Text = "Save";
			MenuStripItemFileSave.Click += MenuStripItemFileSave_Click;
			// 
			// MenuStripItemFileSeparator1
			// 
			MenuStripItemFileSeparator1.Name = "MenuStripItemFileSeparator1";
			MenuStripItemFileSeparator1.Size = new Size( 143, 6 );
			// 
			// MenuStripItemFileExit
			// 
			MenuStripItemFileExit.Name = "MenuStripItemFileExit";
			MenuStripItemFileExit.ShortcutKeys = Keys.Control | Keys.Q;
			MenuStripItemFileExit.Size = new Size( 146, 22 );
			MenuStripItemFileExit.Text = "Exit";
			MenuStripItemFileExit.Click += MenuStripItemFileExit_Click;
			// 
			// StatusStrip
			// 
			StatusStrip.Items.AddRange( new ToolStripItem[] { StatusStripEntriesLabel } );
			StatusStrip.Location = new Point( 0, 539 );
			StatusStrip.Name = "StatusStrip";
			StatusStrip.Size = new Size( 784, 22 );
			StatusStrip.TabIndex = 1;
			// 
			// StatusStripEntriesLabel
			// 
			StatusStripEntriesLabel.Name = "StatusStripEntriesLabel";
			StatusStripEntriesLabel.Size = new Size( 115, 17 );
			StatusStripEntriesLabel.Text = "Number of entries: 0";
			// 
			// MenuStrip
			// 
			MenuStrip.Items.AddRange( new ToolStripItem[] { MenuStripItemFile, MenuStripItemEdit } );
			MenuStrip.Location = new Point( 0, 0 );
			MenuStrip.Name = "MenuStrip";
			MenuStrip.Size = new Size( 784, 24 );
			MenuStrip.TabIndex = 2;
			// 
			// MenuStripItemEdit
			// 
			MenuStripItemEdit.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemEditSearch, MenuStripItemEditSearchForward, MenuStripItemEditSearchBackward } );
			MenuStripItemEdit.Name = "MenuStripItemEdit";
			MenuStripItemEdit.Size = new Size( 39, 20 );
			MenuStripItemEdit.Text = "Edit";
			// 
			// MenuStripItemEditSearch
			// 
			MenuStripItemEditSearch.Name = "MenuStripItemEditSearch";
			MenuStripItemEditSearch.ShortcutKeys = Keys.Control | Keys.F;
			MenuStripItemEditSearch.Size = new Size( 188, 22 );
			MenuStripItemEditSearch.Text = "Search";
			MenuStripItemEditSearch.Click += MenuStripItemEditSearch_Click;
			// 
			// MenuStripItemEditSearchForward
			// 
			MenuStripItemEditSearchForward.Name = "MenuStripItemEditSearchForward";
			MenuStripItemEditSearchForward.ShortcutKeys = Keys.F3;
			MenuStripItemEditSearchForward.Size = new Size( 188, 22 );
			MenuStripItemEditSearchForward.Text = "Search forward";
			MenuStripItemEditSearchForward.Click += MenuStripItemEditSearchForward_Click;
			// 
			// MenuStripItemEditSearchBackward
			// 
			MenuStripItemEditSearchBackward.Name = "MenuStripItemEditSearchBackward";
			MenuStripItemEditSearchBackward.ShortcutKeys = Keys.Shift | Keys.F3;
			MenuStripItemEditSearchBackward.Size = new Size( 188, 22 );
			MenuStripItemEditSearchBackward.Text = "Search back";
			MenuStripItemEditSearchBackward.Click += MenuStripItemEditSearchBackward_Click;
			// 
			// FilesListView
			// 
			FilesListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			FilesListView.Columns.AddRange( new ColumnHeader[] { FilesListViewHeaderName, FilesListViewHeaderType, FilesListViewHeaderUncompressedSize, FilesListViewHeaderIsCompressed } );
			FilesListView.FullRowSelect = true;
			FilesListView.LabelEdit = true;
			FilesListView.Location = new Point( 0, 27 );
			FilesListView.Name = "FilesListView";
			FilesListView.Size = new Size( 784, 512 );
			FilesListView.TabIndex = 3;
			FilesListView.UseCompatibleStateImageBehavior = false;
			FilesListView.View = View.Details;
			FilesListView.AfterLabelEdit += FilesListView_AfterLabelEdit;
			FilesListView.DoubleClick += FilesListView_DoubleClick;
			FilesListView.KeyDown += FilesListView_KeyDown;
			FilesListView.MouseClick += FilesListView_MouseClick;
			// 
			// FilesListViewHeaderName
			// 
			FilesListViewHeaderName.Name = "FilesListViewHeaderName";
			FilesListViewHeaderName.Text = "Name";
			FilesListViewHeaderName.Width = 272;
			// 
			// FilesListViewHeaderType
			// 
			FilesListViewHeaderType.Name = "FilesListViewHeaderType";
			FilesListViewHeaderType.Text = "Type";
			FilesListViewHeaderType.Width = 120;
			// 
			// FilesListViewHeaderUncompressedSize
			// 
			FilesListViewHeaderUncompressedSize.Name = "FilesListViewHeaderUncompressedSize";
			FilesListViewHeaderUncompressedSize.Text = "Uncompressed size";
			FilesListViewHeaderUncompressedSize.Width = 120;
			// 
			// FilesListViewHeaderIsCompressed
			// 
			FilesListViewHeaderIsCompressed.Name = "FilesListViewHeaderIsCompressed";
			FilesListViewHeaderIsCompressed.Text = "Is compressed";
			FilesListViewHeaderIsCompressed.Width = 120;
			// 
			// FilesListViewContextMenu
			// 
			FilesListViewContextMenu.Items.AddRange( new ToolStripItem[] { FilesListViewContextMenuItemOpen, FilesListViewContextMenuItemExtract, FilesListViewContextMenuItemReplace, FilesListViewContextMenuItemRename, FilesListViewContextMenuItemDelete } );
			FilesListViewContextMenu.Name = "FilesListViewContextMenu";
			FilesListViewContextMenu.Size = new Size( 118, 114 );
			// 
			// FilesListViewContextMenuItemOpen
			// 
			FilesListViewContextMenuItemOpen.Name = "FilesListViewContextMenuItemOpen";
			FilesListViewContextMenuItemOpen.Size = new Size( 117, 22 );
			FilesListViewContextMenuItemOpen.Text = "Open";
			FilesListViewContextMenuItemOpen.Click += FilesListViewContextMenuItemOpen_Click;
			// 
			// FilesListViewContextMenuItemExtract
			// 
			FilesListViewContextMenuItemExtract.Name = "FilesListViewContextMenuItemExtract";
			FilesListViewContextMenuItemExtract.Size = new Size( 117, 22 );
			FilesListViewContextMenuItemExtract.Text = "Extract";
			FilesListViewContextMenuItemExtract.Click += FilesListViewContextMenuItemExtract_Click;
			// 
			// FilesListViewContextMenuItemReplace
			// 
			FilesListViewContextMenuItemReplace.Name = "FilesListViewContextMenuItemReplace";
			FilesListViewContextMenuItemReplace.Size = new Size( 117, 22 );
			FilesListViewContextMenuItemReplace.Text = "Replace";
			FilesListViewContextMenuItemReplace.Click += FilesListViewContextMenuItemReplace_Click;
			// 
			// FilesListViewContextMenuItemRename
			// 
			FilesListViewContextMenuItemRename.Name = "FilesListViewContextMenuItemRename";
			FilesListViewContextMenuItemRename.Size = new Size( 117, 22 );
			FilesListViewContextMenuItemRename.Text = "Rename";
			FilesListViewContextMenuItemRename.Click += FilesListViewContextMenuItemRename_Click;
			// 
			// FilesListViewContextMenuItemDelete
			// 
			FilesListViewContextMenuItemDelete.Name = "FilesListViewContextMenuItemDelete";
			FilesListViewContextMenuItemDelete.Size = new Size( 117, 22 );
			FilesListViewContextMenuItemDelete.Text = "Delete";
			FilesListViewContextMenuItemDelete.Click += FilesListViewContextMenuItemDelete_Click;
			// 
			// FormSSGArchiveEditor
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size( 784, 561 );
			Controls.Add( FilesListView );
			Controls.Add( StatusStrip );
			Controls.Add( MenuStrip );
			Icon = (Icon)resources.GetObject( "$this.Icon" );
			MainMenuStrip = MenuStrip;
			MinimumSize = new Size( 640, 480 );
			Name = "FormSSGArchiveEditor";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "ORC ToolKit";
			StatusStrip.ResumeLayout( false );
			StatusStrip.PerformLayout();
			MenuStrip.ResumeLayout( false );
			MenuStrip.PerformLayout();
			FilesListViewContextMenu.ResumeLayout( false );
			ResumeLayout( false );
			PerformLayout();
		}

		#endregion
		private StatusStrip StatusStrip;
		private ToolStripStatusLabel StatusStripEntriesLabel;
		private MenuStrip MenuStrip;
		private ToolStripMenuItem MenuStripItemFileNew;
		private ToolStripMenuItem MenuStripItemFileOpen;
		private ToolStripMenuItem MenuStripItemFileSave;
		private ToolStripMenuItem MenuStripItemEdit;
		private ToolStripSeparator MenuStripItemFileSeparator1;
		private ToolStripMenuItem MenuStripItemFileExit;
		private ToolStripMenuItem MenuStripItemEditSearch;
		private ToolStripMenuItem MenuStripItemEditSearchForward;
		private ToolStripMenuItem MenuStripItemEditSearchBackward;
		private ToolStripMenuItem MenuStripItemFile;
		private ListView FilesListView;
		private ColumnHeader FilesListViewHeaderName;
		private ColumnHeader FilesListViewHeaderType;
		private ContextMenuStrip FilesListViewContextMenu;
		private ToolStripMenuItem FilesListViewContextMenuItemOpen;
		private ToolStripMenuItem FilesListViewContextMenuItemExtract;
		private ToolStripMenuItem FilesListViewContextMenuItemReplace;
		private ToolStripMenuItem FilesListViewContextMenuItemRename;
		private ToolStripMenuItem FilesListViewContextMenuItemDelete;
		private ColumnHeader FilesListViewHeaderUncompressedSize;
		private ColumnHeader FilesListViewHeaderIsCompressed;
	}
}