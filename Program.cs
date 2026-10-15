using ORCToolKit.FileTypes;
using ORCToolKit.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace ORCToolKit
{
	internal static class Program
	{
		static private List<CultureInfo> _cultures = new List<CultureInfo>();
		static public IEnumerable<CultureInfo> AvailableCultures
		{
			get
			{
				if ( _cultures.Count == 0 )
				{
					ResourceManager ResourceManager = new ResourceManager( typeof( Resources.ORCToolKit ) );
					try
					{
						foreach ( CultureInfo Culture in CultureInfo.GetCultures( CultureTypes.AllCultures ) )
						{
							if ( Culture.Equals( CultureInfo.InvariantCulture ) )
								continue;

							if ( ResourceManager.GetResourceSet( Culture, true, false ) != null )
								_cultures.Add( Culture );
						}
					}
					catch ( CultureNotFoundException ex )
					{
						// ignore
					}
				}

				return _cultures;
			}
		}

		static public void SetCulture( CultureInfo Culture )
		{
			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = Culture;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Culture;
			Settings.Default.Language = Culture.Name;

			// reload localization in all forms
			foreach ( Form OpenedForm in Application.OpenForms )
			{
				if ( OpenedForm is LocalizedForm OpenedLocalziedForm )
					OpenedLocalziedForm.LocalizeComponent();
			}
		}

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] Args )
		{
			// make sure we don't get screwed over...
			if ( string.IsNullOrWhiteSpace( Settings.Default.Language ) )
				Settings.Default.Language = "en";
			SetCulture( CultureInfo.GetCultureInfo( Settings.Default.Language, true ) );

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();

			CRC32StringMap.Initialize();

			// try to open a file that was given to us by the windows explorer
			Form? MainForm = null;
			foreach( string Arg in Args)
			{
				MainForm = TypeDeducer.GuessFromFileAndOpen( new FileInfo( Args[Args.Length - 1] ), Silent: true );
				if ( MainForm != null )
					break;
			}

			Application.Run( MainForm ?? new FormFileExplorer() );
			Settings.Default.Save();
		}
	}
}