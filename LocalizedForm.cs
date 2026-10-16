using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace ORCToolKit
{
	public class LocalizedForm: Form
	{
		public struct LocalizedControl
		{
			public required object Object;
			public required string ObjectName;
			public required string FieldName;
		}

		private LocalizedControl[]? _localizedControls = null;
		public virtual LocalizedControl[] LocalizedControls { get { return Array.Empty<LocalizedControl>(); } }

		public void LocalizeComponent()
		{
			// cache it once so we don't regenerate the array every time
			if ( _localizedControls == null )
				_localizedControls = LocalizedControls;

			ResourceManager resourceManager = new ResourceManager( GetType() );
			foreach ( LocalizedControl entry in _localizedControls )
			{
				Type objectType = entry.Object.GetType();
				string? localizedText = resourceManager.GetString( $"{entry.ObjectName}.{entry.FieldName}" );
				Debug.Assert( localizedText != null );

				if ( localizedText != null )
				{
					// annoyingly!! members can be either fields or properties, so try both
					PropertyInfo? property = objectType.GetProperty( entry.FieldName );
					if ( property != null )
					{
						property.SetValue( entry.Object, localizedText );
						continue;
					}

					FieldInfo? field = objectType.GetField( entry.FieldName );
					if ( field != null )
					{
						field.SetValue( entry.Object, localizedText );
						continue;
					}

					// if we got here, it means we tried to localize something that doesn't exist
					Debug.Assert( false );
				}
			}
		}
	}
}
