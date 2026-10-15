namespace ORCToolKit
{
	partial class FormSWB2DatabaseEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FormSWB2DatabaseEditor ) );
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
			EntryTreeView = new TreeViewSWB2EntryExplorer();
			EntryPropertyGrid = new PropertyGrid();
			SplitContainer = new SplitContainer();
			MenuStripItemFile = new ToolStripMenuItem();
			StatusStrip.SuspendLayout();
			MenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)SplitContainer).BeginInit();
			SplitContainer.Panel1.SuspendLayout();
			SplitContainer.Panel2.SuspendLayout();
			SplitContainer.SuspendLayout();
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
			StatusStripEntriesLabel.Text = "Number of tables: 0";
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
			// EntryTreeView
			// 
			EntryTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			EntryTreeView.HideSelection = false;
			EntryTreeView.Location = new Point( 0, 0 );
			EntryTreeView.Name = "EntryTreeView";
			EntryTreeView.Size = new Size( 580, 512 );
			EntryTreeView.TabIndex = 3;
			EntryTreeView.AfterSelect += EntryTreeView_AfterSelect;
			// 
			// EntryPropertyGrid
			// 
			EntryPropertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			EntryPropertyGrid.Location = new Point( 0, 0 );
			EntryPropertyGrid.Name = "EntryPropertyGrid";
			EntryPropertyGrid.PropertySort = PropertySort.NoSort;
			EntryPropertyGrid.Size = new Size( 200, 512 );
			EntryPropertyGrid.TabIndex = 4;
			EntryPropertyGrid.ToolbarVisible = false;
			EntryPropertyGrid.PropertyValueChanged += EntryPropertyGrid_PropertyValueChanged;
			// 
			// SplitContainer
			// 
			SplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			SplitContainer.Location = new Point( 0, 27 );
			SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			SplitContainer.Panel1.Controls.Add( EntryTreeView );
			// 
			// SplitContainer.Panel2
			// 
			SplitContainer.Panel2.Controls.Add( EntryPropertyGrid );
			SplitContainer.Size = new Size( 784, 512 );
			SplitContainer.SplitterDistance = 580;
			SplitContainer.TabIndex = 0;
			// 
			// FormSWB2DatabaseEditor
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size( 784, 561 );
			Controls.Add( SplitContainer );
			Controls.Add( StatusStrip );
			Controls.Add( MenuStrip );
			Icon = (Icon)resources.GetObject( "$this.Icon" );
			MainMenuStrip = MenuStrip;
			MinimumSize = new Size( 640, 480 );
			Name = "FormSWB2DatabaseEditor";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "ORC ToolKit";
			StatusStrip.ResumeLayout( false );
			StatusStrip.PerformLayout();
			MenuStrip.ResumeLayout( false );
			MenuStrip.PerformLayout();
			SplitContainer.Panel1.ResumeLayout( false );
			SplitContainer.Panel2.ResumeLayout( false );
			((System.ComponentModel.ISupportInitialize)SplitContainer).EndInit();
			SplitContainer.ResumeLayout( false );
			ResumeLayout( false );
			PerformLayout();
		}

		#endregion

		private StatusStrip StatusStrip;
		private ToolStripStatusLabel StatusStripEntriesLabel;
		private ToolStripMenuItem MenuStripItemFileNew;
		private ToolStripMenuItem MenuStripItemFileOpen;
		private ToolStripMenuItem MenuStripItemFileSave;
		private ToolStripMenuItem MenuStripItemEdit;
		private ToolStripSeparator MenuStripItemFileSeparator1;
		private ToolStripMenuItem MenuStripItemFileExit;
		private ToolStripMenuItem MenuStripItemEditSearch;
		private ToolStripMenuItem MenuStripItemEditSearchForward;
		private ToolStripMenuItem MenuStripItemEditSearchBackward;
		private MenuStrip MenuStrip;
		private TreeViewSWB2EntryExplorer EntryTreeView;
		private PropertyGrid EntryPropertyGrid;
		private SplitContainer SplitContainer;
	}
}