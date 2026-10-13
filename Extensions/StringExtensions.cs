using System.IO.Hashing;
using System.Text;

namespace ORCToolKit.Extensions
{
	static public class StringExtensions
	{
		static public string NormalizeSlashes( this string String, char Slash = '\\' )
		{
			// this is more memory efficient (somehow), but sometimes slower
			return String.Replace( '\\', Slash ).Replace( '/', Slash );

			// this uses more memory, but is sometimes faster
			//StringBuilder Builder = new StringBuilder( String );
			//Builder.Replace( '\\', Slash );
			//Builder.Replace( '/', Slash );
			//return Builder.ToString();
		}

		static public CRC32 GetCRC32( this string String )
		{
			// the game doesn't invert the hash, C# does; invert it again to return the same value as the game would produce
			return ~Crc32.HashToUInt32( Encoding.UTF8.GetBytes( String ) );
		}
	}
}
