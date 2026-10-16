using ORCToolKit.FileTypes;
using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ORCToolKit.Extensions
{
	static public class BinaryWriterExtensions
	{
		static public void WriteType<T>( this BinaryWriter writer, T type, Endianness endian = Endianness.Little )
		{
			Int32 sizeOfType = Marshal.SizeOf( type );
			byte[] typeBytes = new byte[sizeOfType];

			IntPtr data = Marshal.AllocHGlobal( sizeOfType );
			Marshal.StructureToPtr( type, data, true );
			Marshal.Copy( data, typeBytes, 0, sizeOfType );

			if ( NeedToSwapBytes( endian ) )
			{
				FieldInfo[] typeFields = typeof( T ).GetFields( BindingFlags.Public | BindingFlags.Instance );
				foreach ( FieldInfo typeField in typeFields )
				{
					MarshalAsAttribute? marshalAs = (MarshalAsAttribute?)typeField.GetCustomAttribute( typeof( MarshalAsAttribute ), false );
					if ( marshalAs != null && marshalAs.Value == UnmanagedType.ByValArray )
					{
						Int32 sizeOfElement = Marshal.SizeOf( typeField.FieldType.GetElementType() );
						Int32 offset = Convert.ToInt32( Marshal.OffsetOf( typeof( T ), typeField.Name ) );

						for ( Int32 i = 0; i < marshalAs.SizeConst; i++ )
						{
							Array.Reverse( typeBytes, offset, sizeOfElement );
							offset += sizeOfElement;
						}
					}
					else
					{
						if ( typeField.FieldType.IsEnum ) // hack... why is c# so fucking stupid?
							Array.Reverse( typeBytes, Convert.ToInt32( Marshal.OffsetOf( typeof( T ), typeField.Name ) ), Marshal.SizeOf( typeField.FieldType.GetEnumUnderlyingType() ) );
						else
							Array.Reverse( typeBytes, Convert.ToInt32( Marshal.OffsetOf( typeof( T ), typeField.Name ) ), Marshal.SizeOf( typeField.FieldType ) );
					}
				}
			}

			Marshal.FreeHGlobal( data );

			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Int16 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( Int16 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt16LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt16BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, UInt16 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( UInt16 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt16LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt16BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Int32 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( Int32 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt32LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt32BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, UInt32 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( UInt32 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt32LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt32BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Int64 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( Int64 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt64LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt64BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, UInt64 value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( UInt64 )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt64LittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt64BigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Half value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[Marshal.SizeOf<Half>()];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteHalfLittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteHalfBigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Single value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( Single )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteSingleLittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteSingleBigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static public void Write( this BinaryWriter writer, Double value, Endianness endian = Endianness.Little )
		{
			Span<byte> typeBytes = stackalloc byte[sizeof( Double )];
			switch ( endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteDoubleLittleEndian( typeBytes, value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteDoubleBigEndian( typeBytes, value );
					break;
			}
			writer.Write( typeBytes );
		}

		static private bool NeedToSwapBytes( Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return !BitConverter.IsLittleEndian;
				case Endianness.Big:
					return BitConverter.IsLittleEndian;
			}

			throw new ArgumentOutOfRangeException();
		}
	}
}
