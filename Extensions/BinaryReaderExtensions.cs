using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ORCToolKit.FileTypes;

namespace ORCToolKit.Extensions
{
	static public class BinaryReaderExtensions
	{
		static public string ReadNullTerminatedString( this BinaryReader Reader, Encoding TargetEncoding )
		{
			List<byte> StringBytes = new List<byte>();

			byte[] NullTerminator = TargetEncoding.GetBytes( "\0" );
			Int32 CharacterSize = NullTerminator.Length;

			byte[] LastCharacter;
			while ( !(LastCharacter = Reader.ReadBytes( CharacterSize )).SequenceEqual( NullTerminator ) )
			{
				if ( LastCharacter.Length != CharacterSize )
					throw new EndOfStreamException();

				StringBytes.AddRange( LastCharacter );
			}

			return TargetEncoding.GetString( StringBytes.ToArray() );
		}

		static public string ReadFixedLengthString( this BinaryReader Reader, Encoding TargetEncoding, Int32 Length )
		{
			return TargetEncoding.GetString( Reader.ReadBytes( Length ).TakeWhile( c => c != 0 ).ToArray() );
		}

		static public T ReadType<T>( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			byte[] TypeBytes = Reader.ReadBytes( Marshal.SizeOf( typeof( T ) ) );
			if ( NeedToSwapBytes( Endian ) )
			{
				FieldInfo[] TypeFields = typeof( T ).GetFields( BindingFlags.Public | BindingFlags.Instance );
				foreach ( FieldInfo TypeField in TypeFields )
					Array.Reverse( TypeBytes, Convert.ToInt32( Marshal.OffsetOf( typeof( T ), TypeField.Name ) ), Marshal.SizeOf( TypeField.FieldType ) );
			}

			GCHandle Handle = GCHandle.Alloc( TypeBytes, GCHandleType.Pinned );
			T Type = (T)Marshal.PtrToStructure( Handle.AddrOfPinnedObject(), typeof( T ) );
			Handle.Free();
			return Type;
		}

		static public T[] ReadTypeArray<T>( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			T[] Result = new T[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadType<T>( Reader, Endian );

			return Result;
		}

		static public Int16 ReadInt16( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt16LittleEndian( Reader.ReadBytes( sizeof( Int16 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt16BigEndian( Reader.ReadBytes( sizeof( Int16 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int16[] ReadInt16Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Int16[] Result = new Int16[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadInt16( Reader, Endian );

			return Result;
		}

		static public UInt16 ReadUInt16( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt16LittleEndian( Reader.ReadBytes( sizeof( UInt16 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt16BigEndian( Reader.ReadBytes( sizeof( UInt16 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt16[] ReadUInt16Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			UInt16[] Result = new UInt16[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadUInt16( Reader, Endian );

			return Result;
		}

		static public Int32 ReadInt32( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt32LittleEndian( Reader.ReadBytes( sizeof( Int32 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt32BigEndian( Reader.ReadBytes( sizeof( Int32 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int32[] ReadInt32Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Int32[] Result = new Int32[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadInt32( Reader, Endian );

			return Result;
		}

		static public UInt32 ReadUInt32( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt32LittleEndian( Reader.ReadBytes( sizeof( UInt32 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt32BigEndian( Reader.ReadBytes( sizeof( UInt32 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt32[] ReadUInt32Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			UInt32[] Result = new UInt32[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadUInt32( Reader, Endian );

			return Result;
		}

		static public Int64 ReadInt64( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadInt64LittleEndian( Reader.ReadBytes( sizeof( Int64 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadInt64BigEndian( Reader.ReadBytes( sizeof( Int64 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Int64[] ReadInt64Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Int64[] Result = new Int64[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadInt64( Reader, Endian );

			return Result;
		}

		static public UInt64 ReadUInt64( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadUInt64LittleEndian( Reader.ReadBytes( sizeof( UInt64 ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadUInt64BigEndian( Reader.ReadBytes( sizeof( UInt64 ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public UInt64[] ReadUInt64Array( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			UInt64[] Result = new UInt64[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadUInt64( Reader, Endian );

			return Result;
		}

		static public Half ReadHalf( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadHalfLittleEndian( Reader.ReadBytes( Marshal.SizeOf<Half>() ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadHalfBigEndian( Reader.ReadBytes( Marshal.SizeOf<Half>() ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Half[] ReadHalfArray( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Half[] Result = new Half[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadHalf( Reader, Endian );

			return Result;
		}

		static public Single ReadSingle( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadSingleLittleEndian( Reader.ReadBytes( sizeof( Single ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadSingleBigEndian( Reader.ReadBytes( sizeof( Single ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Single[] ReadSingleArray( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Single[] Result = new Single[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadSingle( Reader, Endian );

			return Result;
		}

		static public Double ReadDouble( this BinaryReader Reader, Endianness Endian = Endianness.Little )
		{
			switch ( Endian )
			{
				case Endianness.Little:
					return BinaryPrimitives.ReadDoubleLittleEndian( Reader.ReadBytes( sizeof( Double ) ) );
				case Endianness.Big:
					return BinaryPrimitives.ReadDoubleBigEndian( Reader.ReadBytes( sizeof( Double ) ) );
			}

			throw new ArgumentOutOfRangeException();
		}

		static public Double[] ReadDoubleArray( this BinaryReader Reader, Int32 Count, Endianness Endian = Endianness.Little )
		{
			Double[] Result = new Double[Count];

			for ( Int32 i = 0; i < Count; i++ )
				Result[i] = ReadDouble( Reader, Endian );

			return Result;
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
