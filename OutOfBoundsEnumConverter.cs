using System.ComponentModel;
using System.Globalization;

namespace ORCToolKit
{
	// god fucking damn it whoever designed EnumConverter is a fucking moron.
	// WHY DOES IT REQUIRE A FLAGS ATTRIBUTE TO CONVERT AN OUT OF BOUNDS VALUE TO A STRING!!!
	// IT WORKS JUST FINE WITH THE STUPID EXCEPTION REMOVED???
	// can't wait to see what lies ahead in my C# journey if this is what I already have to do!!!
	public class OutOfBoundsEnumConverter: EnumConverter
	{
		public OutOfBoundsEnumConverter( Type type ) : base( type ) { }

		public override object? ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
		{
			ArgumentNullException.ThrowIfNull( destinationType );

			if ( destinationType == typeof( string ) && value != null )
				return Enum.Format( EnumType, value, "G" );

			return base.ConvertTo( context, culture, value, destinationType );
		}
	}
}
