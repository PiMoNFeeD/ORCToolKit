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
		}
	}
}
