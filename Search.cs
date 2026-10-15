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

			public SearchEventArgs( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
			{
				this.Direction = Direction;
				this.SearchString = SearchString;
				this.WholeWord = WholeWord;
				this.CaseSensitive = CaseSensitive;
			}
		};

		public class StringSearch
		{
			static public bool Compare( string CompareTo, string SearchString, bool WholeWord, bool CaseSensitive )
			{
				if ( WholeWord )
				{
					// use regex for whole word matching
					RegexOptions Options = RegexOptions.Compiled;
					if ( !CaseSensitive )
						Options |= RegexOptions.IgnoreCase;
					return Regex.IsMatch( CompareTo, $@"\b{SearchString}\b", Options );
				}
				else
				{
					return CompareTo.Contains( SearchString, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase );
				}
			}
		}
	}
}
