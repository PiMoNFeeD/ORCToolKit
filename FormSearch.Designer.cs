namespace ORCToolKit
{
	partial class FormSearch
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
			SearchTextBox = new TextBox();
			SearchLabel = new Label();
			SearchBackwardButton = new Button();
			SearchForwardButton = new Button();
			WholeWordCheckBox = new CheckBox();
			CaseSensitiveCheckBox = new CheckBox();
			SuspendLayout();
			// 
			// SearchTextBox
			// 
			SearchTextBox.Location = new Point( 12, 27 );
			SearchTextBox.Name = "SearchTextBox";
			SearchTextBox.Size = new Size( 260, 23 );
			SearchTextBox.TabIndex = 0;
			SearchTextBox.KeyPress += SearchTextBox_KeyPress;
			// 
			// SearchLabel
			// 
			SearchLabel.AutoSize = true;
			SearchLabel.Location = new Point( 12, 9 );
			SearchLabel.Name = "SearchLabel";
			SearchLabel.Size = new Size( 63, 15 );
			SearchLabel.TabIndex = 1;
			SearchLabel.Text = "Search for:";
			// 
			// SearchBackwardButton
			// 
			SearchBackwardButton.Location = new Point( 147, 135 );
			SearchBackwardButton.Name = "SearchBackwardButton";
			SearchBackwardButton.Size = new Size( 125, 23 );
			SearchBackwardButton.TabIndex = 2;
			SearchBackwardButton.Text = "Search back";
			SearchBackwardButton.UseVisualStyleBackColor = true;
			SearchBackwardButton.Click += SearchBackwardButton_Click;
			// 
			// SearchForwardButton
			// 
			SearchForwardButton.Location = new Point( 147, 106 );
			SearchForwardButton.Name = "SearchForwardButton";
			SearchForwardButton.Size = new Size( 125, 23 );
			SearchForwardButton.TabIndex = 2;
			SearchForwardButton.Text = "Search forward";
			SearchForwardButton.UseVisualStyleBackColor = true;
			SearchForwardButton.Click += SearchForwardButton_Click;
			// 
			// WholeWordCheckBox
			// 
			WholeWordCheckBox.AutoSize = true;
			WholeWordCheckBox.Location = new Point( 12, 56 );
			WholeWordCheckBox.Name = "WholeWordCheckBox";
			WholeWordCheckBox.Size = new Size( 125, 19 );
			WholeWordCheckBox.TabIndex = 3;
			WholeWordCheckBox.Text = "Match whole word";
			WholeWordCheckBox.UseVisualStyleBackColor = true;
			// 
			// CaseSensitiveCheckBox
			// 
			CaseSensitiveCheckBox.AutoSize = true;
			CaseSensitiveCheckBox.Location = new Point( 12, 81 );
			CaseSensitiveCheckBox.Name = "CaseSensitiveCheckBox";
			CaseSensitiveCheckBox.Size = new Size( 86, 19 );
			CaseSensitiveCheckBox.TabIndex = 4;
			CaseSensitiveCheckBox.Text = "Match case";
			CaseSensitiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// FormSearch
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size( 284, 170 );
			Controls.Add( CaseSensitiveCheckBox );
			Controls.Add( WholeWordCheckBox );
			Controls.Add( SearchForwardButton );
			Controls.Add( SearchBackwardButton );
			Controls.Add( SearchLabel );
			Controls.Add( SearchTextBox );
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Margin = new Padding( 4, 3, 4, 3 );
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormSearch";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Search";
			TopMost = true;
			FormClosing += FormSearch_FormClosing;
			VisibleChanged += FormSearch_VisibleChanged;
			ResumeLayout( false );
			PerformLayout();
		}

		#endregion

		private TextBox SearchTextBox;
		private Label SearchLabel;
		private Button SearchBackwardButton;
		private Button SearchForwardButton;
		private CheckBox WholeWordCheckBox;
		private CheckBox CaseSensitiveCheckBox;
	}
}