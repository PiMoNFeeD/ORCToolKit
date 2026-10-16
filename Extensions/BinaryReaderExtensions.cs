using ORCToolKit.FileTypes;
using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ORCToolKit.Extensions
{
	static public class BinaryReaderExtensions
	{
		static public string ReadNullTerminatedString( this BinaryReader reader, Encoding targetEncoding )
		{
			List<byte> stringBytes = new List<byte>();

			byte[] nullTerminator = targetEncoding.GetBytes( "\0" );

			byte[] lastCharacter;
			while ( !(lastCharacter = reader.ReadBytes( nullTerminator.Length )).SequenceEqual( nullTerminator ) )
			{
				if ( lastCharacter.Length != nullTerminator.Length )
					throw new EndOfStreamException();

				stringBytes.AddRange( lastCharacter );
			}

			return targetEncoding.GetString( stringBytes.ToArray() );
		}

		static public string ReadFixedLengthString( this BinaryReader reader, Encoding targetEncoding, Int32 length )
		{
			return targetEncoding.GetString( reader.ReadBytes( length ).TakeWhile( c => c != 0 ).ToArray() );
		}

		static public T ReadType<T>( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			byte[] typeBytes = reader.ReadBytes( Marshal.SizeOf( typeof( T ) ) );
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

			GCHandle handle = GCHandle.Alloc( typeBytes, GCHandleType.Pinned );
			T type = (T)Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
			handle.Free();
			return type;
		}

		static public T[] ReadTypeArray<T>( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			T[] result = new T[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadType<T>( reader, endian );

			return result;
		}

		static public Int16 ReadInt16( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt16LittleEndian( reader.ReadBytes( sizeof( Int16 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt16BigEndian( reader.ReadBytes( sizeof( Int16 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int16[] ReadInt16Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Int16[] result = new Int16[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadInt16( reader, endian );

			return result;
		}

		static public UInt16 ReadUInt16( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt16LittleEndian( reader.ReadBytes( sizeof( UInt16 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt16BigEndian( reader.ReadBytes( sizeof( UInt16 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt16[] ReadUInt16Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			UInt16[] result = new UInt16[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadUInt16( reader, endian );

			return result;
		}

		static public Int32 ReadInt32( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt32LittleEndian( reader.ReadBytes( sizeof( Int32 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt32BigEndian( reader.ReadBytes( sizeof( Int32 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int32[] ReadInt32Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Int32[] result = new Int32[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadInt32( reader, endian );

			return result;
		}

		static public UInt32 ReadUInt32( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt32LittleEndian( reader.ReadBytes( sizeof( UInt32 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt32BigEndian( reader.ReadBytes( sizeof( UInt32 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt32[] ReadUInt32Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			UInt32[] result = new UInt32[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadUInt32( reader, endian );

			return result;
		}

		static public Int64 ReadInt64( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt64LittleEndian( reader.ReadBytes( sizeof( Int64 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt64BigEndian( reader.ReadBytes( sizeof( Int64 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int64[] ReadInt64Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Int64[] result = new Int64[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadInt64( reader, endian );

			return result;
		}

		static public UInt64 ReadUInt64( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt64LittleEndian( reader.ReadBytes( sizeof( UInt64 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt64BigEndian( reader.ReadBytes( sizeof( UInt64 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt64[] ReadUInt64Array( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			UInt64[] result = new UInt64[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadUInt64( reader, endian );

			return result;
		}

		static public Half ReadHalf( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadHalfLittleEndian( reader.ReadBytes( Marshal.SizeOf<Half>() ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadHalfBigEndian( reader.ReadBytes( Marshal.SizeOf<Half>() ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Half[] ReadHalfArray( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Half[] result = new Half[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadHalf( reader, endian );

			return result;
		}

		static public Single ReadSingle( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadSingleLittleEndian( reader.ReadBytes( sizeof( Single ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadSingleBigEndian( reader.ReadBytes( sizeof( Single ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Single[] ReadSingleArray( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Single[] result = new Single[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadSingle( reader, endian );

			return result;
		}

		static public Double ReadDouble( this BinaryReader reader, Endianness endian = Endianness.Little )
		{
			switch ( endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadDoubleLittleEndian( reader.ReadBytes( sizeof( Double ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadDoubleBigEndian( reader.ReadBytes( sizeof( Double ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Double[] ReadDoubleArray( this BinaryReader reader, Int32 count, Endianness endian = Endianness.Little )
		{
			Double[] result = new Double[count];

			for ( Int32 i = 0; i < count; i++ )
				result[i] = ReadDouble( reader, endian );

			return result;
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
