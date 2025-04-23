using System.Configuration;

namespace ORCToolKit
{
    public partial class FormFileExplorer : Form
    {
        public FormFileExplorer()
        {
            InitializeComponent();

            Text = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
            LoadGameFolder(Properties.Settings.Default.GamePath);
        }

        private void GamePathBrowseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog FolderDialog = new FolderBrowserDialog())
            {
                FolderDialog.ShowNewFolderButton = false;
                if (FolderDialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(FolderDialog.SelectedPath))
                {
                    LoadGameFolder(FolderDialog.SelectedPath);
                }
            }
        }

        private void FormFileExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void LoadGameFolder(string GamePath)
        {
            GameFilesExplorer.LoadFolder(GamePath);
            GamePathTextBox.Text = GamePath;
            Properties.Settings.Default.GamePath = GamePath;
        }
    }
}
