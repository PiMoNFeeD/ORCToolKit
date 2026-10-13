using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using ORCToolKit.FileTypes;

namespace ORCToolKit.Extensions
{
	static public class BinaryWriterExtensions
	{
		static public void WriteType<T>( this BinaryWriter Writer, T Type, Endianness Endian = Endianness.Little )
		{
			Int32 SizeOfType = Marshal.SizeOf( Type );
			byte[] TypeBytes = new byte[SizeOfType];

			IntPtr Data = Marshal.AllocHGlobal( SizeOfType );
			Marshal.StructureToPtr( Type, Data, true );
			Marshal.Copy( Data, TypeBytes, 0, SizeOfType );

			if ( NeedToSwapBytes( Endian ) )
			{
				FieldInfo[] TypeFields = typeof( T ).GetFields( BindingFlags.Public | BindingFlags.Instance );
				foreach ( FieldInfo TypeField in TypeFields )
				{
					MarshalAsAttribute? MarshalAs = (MarshalAsAttribute?)TypeField.GetCustomAttribute( typeof( MarshalAsAttribute ), false );
					if ( MarshalAs != null && MarshalAs.Value == UnmanagedType.ByValArray )
					{
						Int32 SizeOfElement = Marshal.SizeOf( TypeField.FieldType.GetElementType() );
						Int32 Offset = Convert.ToInt32( Marshal.OffsetOf( typeof( T ), TypeField.Name ) );

						for ( Int32 i = 0; i < MarshalAs.SizeConst; i++ )
						{
							Array.Reverse( TypeBytes, Offset, SizeOfElement );
							Offset += SizeOfElement;
						}
					}
					else
					{
						if ( TypeField.FieldType.IsEnum ) // hack... why is c# so fucking stupid?
							Array.Reverse( TypeBytes, Convert.ToInt32( Marshal.OffsetOf( typeof( T ), TypeField.Name ) ), Marshal.SizeOf( TypeField.FieldType.GetEnumUnderlyingType() ) );
						else
							Array.Reverse( TypeBytes, Convert.ToInt32( Marshal.OffsetOf( typeof( T ), TypeField.Name ) ), Marshal.SizeOf( TypeField.FieldType ) );
					}
				}
			}

			Marshal.FreeHGlobal( Data );

			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Int16 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( Int16 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt16LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt16BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, UInt16 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( UInt16 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt16LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt16BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Int32 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( Int32 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt32LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt32BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, UInt32 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( UInt32 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt32LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt32BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Int64 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( Int64 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteInt64LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteInt64BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, UInt64 Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( UInt64 )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteUInt64LittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteUInt64BigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Half Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[Marshal.SizeOf<Half>()];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteHalfLittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteHalfBigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Single Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( Single )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteSingleLittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteSingleBigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static public void Write( this BinaryWriter Writer, Double Value, Endianness Endian = Endianness.Little )
		{
			Span<byte> TypeBytes = stackalloc byte[sizeof( Double )];
			switch ( Endian )
			{
				case Endianness.Little:
					BinaryPrimitives.WriteDoubleLittleEndian( TypeBytes, Value );
					break;
				case Endianness.Big:
					BinaryPrimitives.WriteDoubleBigEndian( TypeBytes, Value );
					break;
			}
			Writer.Write( TypeBytes );
		}

		static private bool NeedToSwapBytes( Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
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
