using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ORCToolKit
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
	}
}
