using System.ComponentModel;

namespace ORCToolKit
{
	public class LocalizedDescriptionAttribute: DescriptionAttribute
	{
		public LocalizedDescriptionAttribute( string description ) : base( description )
		{
		}

		public override string Description
		{
			get
			{
				return Resources.ORCToolKit.ResourceManager.GetString( DescriptionValue ) ?? string.Empty;
			}
		}
	}
}
