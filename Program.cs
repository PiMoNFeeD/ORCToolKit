using ORCToolKit.FileTypes;
using ORCToolKit.Properties;
using System.Globalization;
using System.Resources;

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
					ResourceManager resourceManager = new ResourceManager( typeof( Resources.ORCToolKit ) );
					try
					{
						foreach ( CultureInfo culture in CultureInfo.GetCultures( CultureTypes.AllCultures ) )
						{
							if ( culture.Equals( CultureInfo.InvariantCulture ) )
								continue;

							if ( resourceManager.GetResourceSet( culture, true, false ) != null )
								_cultures.Add( culture );
						}
					}
					catch ( CultureNotFoundException )
					{
						// ignore
					}
				}

				return _cultures;
			}
		}

		static public void SetCulture( CultureInfo culture )
		{
			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
			Settings.Default.Language = culture.Name;

			// reload localization in all forms
			foreach ( Form openedForm in Application.OpenForms )
			{
				if ( openedForm is LocalizedForm openedLocalziedForm )
					openedLocalziedForm.LocalizeComponent();
			}
		}

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main( string[] args )
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
			Form? mainForm = null;
			foreach ( string arg in args )
			{
				mainForm = TypeDeducer.GuessFromFileAndOpen( new FileInfo( args[args.Length - 1] ), silent: true );
				if ( mainForm != null )
					break;
			}

			Application.Run( mainForm ?? new FormFileExplorer() );
			Settings.Default.Save();
		}
	}
}