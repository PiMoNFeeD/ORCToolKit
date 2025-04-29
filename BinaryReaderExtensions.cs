using System.Runtime.InteropServices;
using System.Text;

namespace ORCToolKit
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

		static public T ReadType<T>( this BinaryReader Reader )
		{
			GCHandle Handle = GCHandle.Alloc( Reader.ReadBytes( Marshal.SizeOf( typeof( T ) ) ), GCHandleType.Pinned );
			T Type = (T)Marshal.PtrToStructure( Handle.AddrOfPinnedObject(), typeof( T ) );
			Handle.Free();
			return Type;
		}
	}
}
