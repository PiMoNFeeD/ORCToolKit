using System.Text.RegularExpressions;

namespace ORCToolKit
{
	namespace Search
	{
		public enum SearchDirection
		{
			Forward,
			Backward
		}
		public delegate void SearchEventHandler( object sender, SearchEventArgs e );

		public class SearchEventArgs: EventArgs
		{
			public SearchDirection Direction;
			public string SearchString;
			public bool WholeWord;
			public bool CaseSensitive;

			public SearchEventArgs( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
			{
				Direction = direction;
				SearchString = searchString;
				WholeWord = wholeWord;
				CaseSensitive = caseSensitive;
			}
		};

		public class StringSearch
		{
			static public bool Compare( string compareTo, string searchString, bool wholeWord, bool caseSensitive )
			{
				if ( wholeWord )
				{
					// use regex for whole word matching
					RegexOptions options = RegexOptions.Compiled;
					if ( !caseSensitive )
						options |= RegexOptions.IgnoreCase;
					return Regex.IsMatch( compareTo, $@"\b{searchString}\b", options );
				}
				else
				{
					return compareTo.Contains( searchString, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase );
				}
			}
		}
	}
}
