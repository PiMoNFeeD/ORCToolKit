namespace ORCToolKit
{
	partial class FormLocalizationEditor
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
			ToolStripMenuItem MenuStripItemFile;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FormLocalizationEditor ) );
			MenuStripItemFileNew = new ToolStripMenuItem();
			MenuStripItemFileOpen = new ToolStripMenuItem();
			MenuStripItemFileSave = new ToolStripMenuItem();
			MenuStripItemFileSeparator1 = new ToolStripSeparator();
			MenuStripItemFileExit = new ToolStripMenuItem();
			TextEntriesGridView = new DoubleBufferedDataGridView();
			StatusStrip = new StatusStrip();
			StatusStripEntriesLabel = new ToolStripStatusLabel();
			MenuStrip = new MenuStrip();
			MenuStripItemEdit = new ToolStripMenuItem();
			MenuStripItemEditInsertEntryAfter = new ToolStripMenuItem();
			MenuStripItemEditInsertEntryBefore = new ToolStripMenuItem();
			MenuStripItemEditRemoveEntry = new ToolStripMenuItem();
			MenuStripItemEditSeparator1 = new ToolStripSeparator();
			MenuStripItemEditSearch = new ToolStripMenuItem();
			MenuStripItemEditSearchForward = new ToolStripMenuItem();
			MenuStripItemEditSearchBackward = new ToolStripMenuItem();
			TextEntriesGridViewNameColumn = new DataGridViewTextBoxColumn();
			TextEntriesGridViewTextColumn = new DataGridViewTextBoxColumn();
			MenuStripItemFile = new ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)TextEntriesGridView).BeginInit();
			StatusStrip.SuspendLayout();
			MenuStrip.SuspendLayout();
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
			// TextEntriesGridView
			// 
			TextEntriesGridView.AllowUserToAddRows = false;
			TextEntriesGridView.AllowUserToDeleteRows = false;
			TextEntriesGridView.AllowUserToResizeRows = false;
			TextEntriesGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			TextEntriesGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			TextEntriesGridView.Columns.AddRange( new DataGridViewColumn[] { TextEntriesGridViewNameColumn, TextEntriesGridViewTextColumn } );
			TextEntriesGridView.Location = new Point( 0, 27 );
			TextEntriesGridView.Name = "TextEntriesGridView";
			TextEntriesGridView.RowHeadersVisible = false;
			TextEntriesGridView.ScrollBars = ScrollBars.Vertical;
			TextEntriesGridView.Size = new Size( 784, 512 );
			TextEntriesGridView.TabIndex = 0;
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
			MenuStripItemEdit.DropDownItems.AddRange( new ToolStripItem[] { MenuStripItemEditInsertEntryAfter, MenuStripItemEditInsertEntryBefore, MenuStripItemEditRemoveEntry, MenuStripItemEditSeparator1, MenuStripItemEditSearch, MenuStripItemEditSearchForward, MenuStripItemEditSearchBackward } );
			MenuStripItemEdit.Name = "MenuStripItemEdit";
			MenuStripItemEdit.Size = new Size( 39, 20 );
			MenuStripItemEdit.Text = "Edit";
			// 
			// MenuStripItemEditInsertEntryAfter
			// 
			MenuStripItemEditInsertEntryAfter.Name = "MenuStripItemEditInsertEntryAfter";
			MenuStripItemEditInsertEntryAfter.ShortcutKeys = Keys.Insert;
			MenuStripItemEditInsertEntryAfter.Size = new Size( 233, 22 );
			MenuStripItemEditInsertEntryAfter.Text = "Insert entry";
			MenuStripItemEditInsertEntryAfter.Click += MenuStripItemEditInsertEntryAfter_Click;
			// 
			// MenuStripItemEditInsertEntryBefore
			// 
			MenuStripItemEditInsertEntryBefore.Name = "MenuStripItemEditInsertEntryBefore";
			MenuStripItemEditInsertEntryBefore.ShortcutKeys = Keys.Control | Keys.Insert;
			MenuStripItemEditInsertEntryBefore.Size = new Size( 233, 22 );
			MenuStripItemEditInsertEntryBefore.Text = "Insert entry before";
			MenuStripItemEditInsertEntryBefore.Click += MenuStripItemEditInsertEntryBefore_Click;
			// 
			// MenuStripItemEditRemoveEntry
			// 
			MenuStripItemEditRemoveEntry.Name = "MenuStripItemEditRemoveEntry";
			MenuStripItemEditRemoveEntry.ShortcutKeys = Keys.Delete;
			MenuStripItemEditRemoveEntry.Size = new Size( 233, 22 );
			MenuStripItemEditRemoveEntry.Text = "Remove entry";
			MenuStripItemEditRemoveEntry.Click += MenuStripItemEditRemoveEntry_Click;
			// 
			// MenuStripItemEditSeparator1
			// 
			MenuStripItemEditSeparator1.Name = "MenuStripItemEditSeparator1";
			MenuStripItemEditSeparator1.Size = new Size( 230, 6 );
			// 
			// MenuStripItemEditSearch
			// 
			MenuStripItemEditSearch.Name = "MenuStripItemEditSearch";
			MenuStripItemEditSearch.ShortcutKeys = Keys.Control | Keys.F;
			MenuStripItemEditSearch.Size = new Size( 233, 22 );
			MenuStripItemEditSearch.Text = "Search";
			MenuStripItemEditSearch.Click += MenuStripItemEditSearch_Click;
			// 
			// MenuStripItemEditSearchForward
			// 
			MenuStripItemEditSearchForward.Name = "MenuStripItemEditSearchForward";
			MenuStripItemEditSearchForward.ShortcutKeys = Keys.F3;
			MenuStripItemEditSearchForward.Size = new Size( 233, 22 );
			MenuStripItemEditSearchForward.Text = "Search forward";
			MenuStripItemEditSearchForward.Click += MenuStripItemEditSearchForward_Click;
			// 
			// MenuStripItemEditSearchBackward
			// 
			MenuStripItemEditSearchBackward.Name = "MenuStripItemEditSearchBackward";
			MenuStripItemEditSearchBackward.ShortcutKeys = Keys.Shift | Keys.F3;
			MenuStripItemEditSearchBackward.Size = new Size( 233, 22 );
			MenuStripItemEditSearchBackward.Text = "Search back";
			MenuStripItemEditSearchBackward.Click += MenuStripItemEditSearchBackward_Click;
			// 
			// TextEntriesGridViewNameColumn
			// 
			TextEntriesGridViewNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			TextEntriesGridViewNameColumn.DataPropertyName = "Name";
			TextEntriesGridViewNameColumn.HeaderText = "Name";
			TextEntriesGridViewNameColumn.Name = "TextEntriesGridViewNameColumn";
			TextEntriesGridViewNameColumn.Resizable = DataGridViewTriState.False;
			TextEntriesGridViewNameColumn.Width = 64;
			// 
			// TextEntriesGridViewTextColumn
			// 
			TextEntriesGridViewTextColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			TextEntriesGridViewTextColumn.DataPropertyName = "Text";
			TextEntriesGridViewTextColumn.HeaderText = "Text";
			TextEntriesGridViewTextColumn.Name = "TextEntriesGridViewTextColumn";
			TextEntriesGridViewTextColumn.Resizable = DataGridViewTriState.False;
			// 
			// FormLocalizationEditor
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size( 784, 561 );
			Controls.Add( StatusStrip );
			Controls.Add( MenuStrip );
			Controls.Add( TextEntriesGridView );
			Icon = (Icon)resources.GetObject( "$this.Icon" );
			MainMenuStrip = MenuStrip;
			MinimumSize = new Size( 640, 480 );
			Name = "FormLocalizationEditor";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "ORC ToolKit";
			((System.ComponentModel.ISupportInitialize)TextEntriesGridView).EndInit();
			StatusStrip.ResumeLayout( false );
			StatusStrip.PerformLayout();
			MenuStrip.ResumeLayout( false );
			MenuStrip.PerformLayout();
			ResumeLayout( false );
			PerformLayout();
		}

		#endregion

		private DoubleBufferedDataGridView TextEntriesGridView;
		private StatusStrip StatusStrip;
		private ToolStripStatusLabel StatusStripEntriesLabel;
		private MenuStrip MenuStrip;
		private ToolStripMenuItem MenuStripItemFileNew;
		private ToolStripMenuItem MenuStripItemFileOpen;
		private ToolStripMenuItem MenuStripItemFileSave;
		private ToolStripMenuItem MenuStripItemEdit;
		private ToolStripMenuItem MenuStripItemEditInsertEntryAfter;
		private ToolStripMenuItem MenuStripItemEditInsertEntryBefore;
		private ToolStripMenuItem MenuStripItemEditRemoveEntry;
		private ToolStripSeparator MenuStripItemFileSeparator1;
		private ToolStripMenuItem MenuStripItemFileExit;
		private ToolStripSeparator MenuStripItemEditSeparator1;
		private ToolStripMenuItem MenuStripItemEditSearch;
		private ToolStripMenuItem MenuStripItemEditSearchForward;
		private ToolStripMenuItem MenuStripItemEditSearchBackward;
		private DataGridViewTextBoxColumn TextEntriesGridViewNameColumn;
		private DataGridViewTextBoxColumn TextEntriesGridViewTextColumn;
	}
}