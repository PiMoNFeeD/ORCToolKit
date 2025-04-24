using ORCToolKit.Properties;
using System.ComponentModel;
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

		static private void RecursiveApplyMenuItemResources( ToolStripItem StripItem, ref ComponentResourceManager ResourceManager )
		{
			// fuck this shit why are windows forms so fucking retarded that I need to do this
			ResourceManager.ApplyResources( StripItem, StripItem.Name );
			if ( StripItem is ToolStripDropDownItem StripDropDownItem )
			{
				foreach ( ToolStripItem DropDownItem in StripDropDownItem.DropDownItems )
				{
					if ( DropDownItem is ToolStripMenuItem )
						RecursiveApplyMenuItemResources( (ToolStripMenuItem)DropDownItem, ref ResourceManager );
				}
			}
		}

		static public void ReloadFormResources( Form Form )
		{
			ComponentResourceManager ResourceManager = new ComponentResourceManager( Form.GetType() );
			ResourceManager.ApplyResources( Form, Form.Name );
			foreach ( Control FormControl in Form.Controls )
			{
				ResourceManager.ApplyResources( FormControl, FormControl.Name );

				// HACK: handle strips separately, their items are not controls! fucking annoying...
				if ( FormControl is ToolStrip FormMenuStrip )
				{
					ResourceManager.ApplyResources( FormMenuStrip, FormMenuStrip.Name );
					foreach ( ToolStripItem StripItem in FormMenuStrip.Items )
						RecursiveApplyMenuItemResources( StripItem, ref ResourceManager );
				}

				// EVEN WORSE!!!
				if ( FormControl is DataGridView FormDataGridView )
				{
					foreach ( DataGridViewColumn Column in FormDataGridView.Columns )
						ResourceManager.ApplyResources( Column, Column.Name );
				}
			}
			ContextMenuStrip? FormContextMenuStrip = Form.ContextMenuStrip;
			if (FormContextMenuStrip != null)
			{
				foreach ( ToolStripItem StripItem in FormContextMenuStrip.Items )
					RecursiveApplyMenuItemResources( StripItem, ref ResourceManager );
			}
		}

		static public void SetCulture( CultureInfo Culture )
		{
			CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = Culture;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Culture;
			Settings.Default.Language = Culture.Name;

			// reload localization in all forms
			foreach ( Form OpenedForm in Application.OpenForms )
				ReloadFormResources( OpenedForm );
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