using ORCToolKit.Extensions;

namespace ORCToolKit
{
	static public class CRC32StringMap
	{
		static private readonly Dictionary<CRC32, string> _dict = new Dictionary<CRC32, string>();

		static public void Initialize()
		{
			using ( StreamReader Reader = new StreamReader( "strings.txt" ) )
			{
				string? Line;
				while ( (Line = Reader.ReadLine()) != null )
					_dict.TryAdd( Line.GetCRC32(), Line );
			}
		}

		static public CRC32 StoreString( string String )
		{
			CRC32 Result = String.GetCRC32();
			_dict.TryAdd( Result, String );
			return Result;
		}

		static public string? FindString( UInt32 CRC32 )
		{
			if ( _dict.TryGetValue( CRC32, out string? String ) )
				return String;
			return null;
		}
	}
}
