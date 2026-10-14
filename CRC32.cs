using ORCToolKit.Extensions;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ORCToolKit
{
	[TypeConverter( typeof( CRC32Converter ) ), StructLayout( LayoutKind.Sequential )]
	public class CRC32: IComparable, IComparable<CRC32>, IComparable<UInt32>, IComparable<string>,
						IEquatable<CRC32>, IEquatable<UInt32>, IEquatable<string>,
						IConvertible
	{
		private UInt32 _value;
		
		public CRC32() {}
		public CRC32( UInt32 Value ) { _value = Value; }
		public CRC32( string Value ) { _value = CRC32StringMap.StoreString( Value ); }

		static public implicit operator UInt32( CRC32 Value ) { return Value._value; }
		static public implicit operator CRC32( UInt32 Value ) { return new CRC32( Value ); }
		static public bool operator ==( CRC32 Value1, CRC32 Value2 ) { return Value1.Equals( Value2 ); }
		static public bool operator ==( CRC32 ValueNum, string ValueString ) { return ValueNum == ValueString.GetCRC32(); }
		static public bool operator !=( CRC32 Value1, CRC32 Value2 ) { return !Value1.Equals( Value2 ); }
		static public bool operator !=( CRC32 ValueNum, string ValueString ) { return ValueNum != ValueString.GetCRC32(); }

		public override string ToString()
		{
			// TODO: this is extremely bad, you can't actually distinguish
			// between a string that is a number and an actual number...
			return CRC32StringMap.FindString( _value ) ?? _value.ToString();
		}
		public override Int32 GetHashCode() { return _value.GetHashCode(); }

		// IComparable
		public Int32 CompareTo( object? other ) { return _value.CompareTo( other ); }
		public Int32 CompareTo( CRC32? other ) { return _value.CompareTo( other?._value ); }
		public Int32 CompareTo( UInt32 other ) { return _value.CompareTo( other ); }
		public Int32 CompareTo( string? other ) { return _value.CompareTo( other ); }

		// IEquatable
		public Boolean Equals( CRC32? other ) { return _value.Equals( other?._value ); }
		public Boolean Equals( UInt32 other ) { return _value.Equals( other ); }
		public Boolean Equals( string? other ) { return Equals( other?.GetCRC32() ); }

		// IConvertible
		public TypeCode GetTypeCode() { return _value.GetTypeCode(); }
		public Boolean ToBoolean( IFormatProvider? provider ) { return Convert.ToBoolean( _value, provider ); }
		public Byte ToByte( IFormatProvider? provider ) { return Convert.ToByte( _value, provider ); }
		public Char ToChar( IFormatProvider? provider ) { return Convert.ToChar( _value, provider ); }
		public DateTime ToDateTime( IFormatProvider? provider ) { return Convert.ToDateTime( _value, provider ); }
		public Decimal ToDecimal( IFormatProvider? provider ) { return Convert.ToDecimal( _value, provider ); }
		public Double ToDouble( IFormatProvider? provider ) { return Convert.ToDouble( _value, provider ); }
		public Int16 ToInt16( IFormatProvider? provider ) { return Convert.ToInt16( _value, provider ); }
		public Int32 ToInt32( IFormatProvider? provider ) { return Convert.ToInt32( _value, provider ); }
		public Int64 ToInt64( IFormatProvider? provider ) { return Convert.ToInt64( _value, provider ); }
		public SByte ToSByte( IFormatProvider? provider ) { return Convert.ToSByte( _value, provider ); }
		public Single ToSingle( IFormatProvider? provider ) { return Convert.ToSingle( _value, provider ); }
		public String ToString( IFormatProvider? provider ) { return _value.ToString( provider ); }
		public Object ToType( Type conversionType, IFormatProvider? provider ) { return Convert.ChangeType( _value, conversionType, provider ); }
		public UInt16 ToUInt16( IFormatProvider? provider ) { return Convert.ToUInt16( _value, provider ); }
		public UInt32 ToUInt32( IFormatProvider? provider ) { return Convert.ToUInt32( _value, provider ); }
		public UInt64 ToUInt64( IFormatProvider? provider ) { return Convert.ToUInt64( _value, provider ); }
	}

	public class CRC32Converter: UInt32Converter
	{
		public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType )
		{
			return sourceType == typeof( UInt32 ) || base.CanConvertFrom( context, sourceType );
		}

		public override bool CanConvertTo( ITypeDescriptorContext? context, [NotNullWhen( true )] Type? destinationType )
		{
			return destinationType == typeof( CRC32 ) || base.CanConvertTo( context, destinationType );
		}

		public override object? ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object value )
		{
			if ( value is UInt32 )
				return new CRC32( (UInt32)value );

			if ( value is string )
			{
				if ( UInt32.TryParse( (string)value, NumberStyles.Integer, culture, out UInt32 Number ) )
					return new CRC32( Number );

				// if this isn't a valid number, instead treat it as string
				return new CRC32( (string)value );
			}

			object? Result = base.ConvertFrom( context, culture, value );
			if ( Result is UInt32 )
				return new CRC32( (UInt32)Result );

			return base.ConvertFrom( context, culture, value );
		}

		public override object? ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
		{
			if ( value is CRC32 Value )
				return base.ConvertTo( context, culture, Value, destinationType );
			return base.ConvertTo( context, culture, value, destinationType );
		}
	}
}
