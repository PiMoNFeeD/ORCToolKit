using System.IO.Hashing;
using System.Text;

namespace ORCToolKit.Extensions
{
	static public class StringExtensions
	{
		static public string NormalizeSlashes( this string str, char slash = '\\' )
		{
			// this is more memory efficient (somehow), but sometimes slower
			return str.Replace( '\\', slash ).Replace( '/', slash );

			// this uses more memory, but is sometimes faster
			//StringBuilder builder = new StringBuilder( str );
			//builder.Replace( '\\', slash );
			//builder.Replace( '/', slash );
			//return builder.ToString();
		}

		static public CRC32 GetCRC32( this string str )
		{
			// the game doesn't invert the hash, C# does; invert it again to return the same value as the game would produce
			return ~Crc32.HashToUInt32( Encoding.UTF8.GetBytes( str ) );
		}
	}
}
