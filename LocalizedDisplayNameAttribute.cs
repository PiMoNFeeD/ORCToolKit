using System.ComponentModel;

namespace ORCToolKit
{
	public class LocalizedDisplayNameAttribute: DisplayNameAttribute
	{
		public LocalizedDisplayNameAttribute( string Name ) : base( Name )
		{
		}

		public override string DisplayName
		{
			get
			{
				return Resources.ORCToolKit.ResourceManager.GetString( DisplayNameValue ) ?? string.Empty;
			}
		}
	}
}
