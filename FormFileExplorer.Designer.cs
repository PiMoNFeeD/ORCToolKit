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
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFileExplorer));
            GamePathLabel = new Label();
            GamePathTextBox = new TextBox();
            GamePathBrowseButton = new Button();
            GameFilesExplorer = new TreeViewFileExplorer();
            SuspendLayout();
            // 
            // GamePathLabel
            // 
            GamePathLabel.AutoSize = true;
            GamePathLabel.Location = new Point(12, 9);
            GamePathLabel.Name = "GamePathLabel";
            GamePathLabel.Size = new Size(115, 15);
            GamePathLabel.TabIndex = 0;
            GamePathLabel.Text = "Path to game folder:";
            // 
            // GamePathTextBox
            // 
            GamePathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GamePathTextBox.Location = new Point(12, 27);
            GamePathTextBox.Name = "GamePathTextBox";
            GamePathTextBox.ReadOnly = true;
            GamePathTextBox.Size = new Size(679, 23);
            GamePathTextBox.TabIndex = 1;
            // 
            // GamePathBrowseButton
            // 
            GamePathBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            GamePathBrowseButton.Location = new Point(697, 27);
            GamePathBrowseButton.Name = "GamePathBrowseButton";
            GamePathBrowseButton.RightToLeft = RightToLeft.No;
            GamePathBrowseButton.Size = new Size(75, 23);
            GamePathBrowseButton.TabIndex = 2;
            GamePathBrowseButton.Text = "Browse";
            GamePathBrowseButton.UseVisualStyleBackColor = true;
            GamePathBrowseButton.Click += GamePathBrowseButton_Click;
            // 
            // GameFilesExplorer
            // 
            GameFilesExplorer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            GameFilesExplorer.ImageIndex = 0;
            GameFilesExplorer.Location = new Point(12, 56);
            GameFilesExplorer.Name = "GameFilesExplorer";
            GameFilesExplorer.SelectedImageIndex = 0;
            GameFilesExplorer.Size = new Size(760, 493);
            GameFilesExplorer.TabIndex = 3;
            // 
            // FormFileExplorer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(GameFilesExplorer);
            Controls.Add(GamePathBrowseButton);
            Controls.Add(GamePathTextBox);
            Controls.Add(GamePathLabel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 480);
            Name = "FormFileExplorer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ORC ToolKit";
            WindowState = FormWindowState.Maximized;
            FormClosing += FormFileExplorer_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label GamePathLabel;
        private TextBox GamePathTextBox;
        private Button GamePathBrowseButton;
        private TreeViewFileExplorer GameFilesExplorer;
    }
}
