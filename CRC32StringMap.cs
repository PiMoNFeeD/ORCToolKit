using ORCToolKit.Extensions;

namespace ORCToolKit
{
	static public class CRC32StringMap
	{
		static private readonly Dictionary<CRC32, string> _dict = new Dictionary<CRC32, string>();

		static public void Initialize()
		{
			using ( StreamReader reader = new StreamReader( "strings.txt" ) )
			{
				string? line;
				while ( (line = reader.ReadLine()) != null )
					_dict.TryAdd( line.GetCRC32(), line );
			}
		}

		static public CRC32 StoreString( string str )
		{
			CRC32 result = str.GetCRC32();
			_dict.TryAdd( result, str );
			return result;
		}

		static public string? FindString( CRC32 crc32 )
		{
			if ( _dict.TryGetValue( crc32, out string? str ) )
				return str;
			return null;
		}
	}
}
