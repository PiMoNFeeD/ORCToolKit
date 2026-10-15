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

			ResourceManager ResourceManager = new ResourceManager( GetType() );
			foreach ( LocalizedControl Entry in _localizedControls )
			{
				Type ObjectType = Entry.Object.GetType();
				string? LocalizedText = ResourceManager.GetString( $"{Entry.ObjectName}.{Entry.FieldName}" );
				Debug.Assert( LocalizedText != null );

				if ( LocalizedText != null )
				{
					// annoyingly!! members can be either fields or properties, so try both
					PropertyInfo? Property = ObjectType.GetProperty( Entry.FieldName );
					if ( Property != null )
					{
						Property.SetValue( Entry.Object, LocalizedText );
						continue;
					}

					FieldInfo? Field = ObjectType.GetField( Entry.FieldName );
					if ( Field != null )
					{
						Field.SetValue( Entry.Object, LocalizedText );
						continue;
					}

					// if we got here, it means we tried to localize something that doesn't exist
					Debug.Assert( false );
				}
			}
		}
	}
}
