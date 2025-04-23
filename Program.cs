using ORCToolKit.Properties;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

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
					ResourceManager ResourceManager = new ResourceManager( typeof( FormFileExplorer ) );
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
			private set { }
		}

		static private void RecursiveApplyMenuItemResources( ToolStripMenuItem MenuItem, ref ComponentResourceManager ResourceManager )
		{
			// fuck this shit why are windows forms so fucking retarded that I need to do this
			ResourceManager.ApplyResources( MenuItem, MenuItem.Name );
			foreach ( ToolStripMenuItem DropDownMenuItem in MenuItem.DropDownItems )
				RecursiveApplyMenuItemResources( DropDownMenuItem, ref ResourceManager );
		}

		static public void SetCulture( CultureInfo Culture )
		{
			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = Culture;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Culture;
			Settings.Default.Language = Culture.Name;

			// reload localization in all forms
			foreach ( Form OpenedForm in Application.OpenForms )
			{
				ComponentResourceManager ResourceManager = new ComponentResourceManager( OpenedForm.GetType() );
				ResourceManager.ApplyResources( OpenedForm, OpenedForm.Name );
				foreach ( Control FormControl in OpenedForm.Controls )
				{
					ResourceManager.ApplyResources( FormControl, FormControl.Name );

					// HACK: handle menus separately, their items are not controls! fucking annoying...
					MenuStrip FormMenuStrip = FormControl as MenuStrip;
					if ( FormMenuStrip != null )
					{
						ResourceManager.ApplyResources( FormMenuStrip, FormMenuStrip.Name );
						foreach ( ToolStripMenuItem MenuItem in FormMenuStrip.Items )
							RecursiveApplyMenuItemResources( MenuItem, ref ResourceManager );
					}
				}
			}
		}

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// make sure we don't get screwed over...
			if ( string.IsNullOrWhiteSpace( Settings.Default.Language ) )
				Settings.Default.Language = "en";
			SetCulture( CultureInfo.GetCultureInfo( Settings.Default.Language, true ) );

			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			Application.Run( new FormFileExplorer() );
			Settings.Default.Save();
		}
	}
}