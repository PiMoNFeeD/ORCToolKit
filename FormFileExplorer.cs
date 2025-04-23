using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace ORCToolKit
{
	public partial class FormFileExplorer: Form
	{
		public FormFileExplorer()
		{
			InitializeComponent();

			Text = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
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

				MenuStripItemLanguage.DropDownItems.Add( LanguageItem );
			}
		}

		private void GamePathBrowseButton_Click( object sender, EventArgs e )
		{
			using ( FolderBrowserDialog FolderDialog = new FolderBrowserDialog() )
			{
				FolderDialog.ShowNewFolderButton = false;
				if ( FolderDialog.ShowDialog( this ) == DialogResult.OK && !string.IsNullOrWhiteSpace( FolderDialog.SelectedPath ) )
				{
					LoadGameFolder( FolderDialog.SelectedPath );
				}
			}
		}

		private void MenuStripItemExit_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}

		private void MenuStripItemLanguage_DropDownItemClicked( object sender, ToolStripItemClickedEventArgs e )
		{
			ToolStripMenuItem? SelectedLanguageItem = e.ClickedItem as ToolStripMenuItem;
			if ( SelectedLanguageItem != null )
			{
				Program.SetCulture( (CultureInfo)SelectedLanguageItem.Tag );

				foreach ( ToolStripMenuItem LanguageItem in MenuStripItemLanguage.DropDownItems )
					LanguageItem.Checked = SelectedLanguageItem.Equals( LanguageItem );
			}
		}

		private void LoadGameFolder( string GamePath )
		{
			GameFilesExplorer.LoadFolder( GamePath );
			GamePathTextBox.Text = GamePath;
			Properties.Settings.Default.GamePath = GamePath;
		}
	}
}
