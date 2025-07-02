using SQLite;

namespace CoreMP
{
	public class ConnectionDetailsModel
	{
		public static SQLiteConnection SynchConnection { get; internal set; } = null;
		public static SQLiteAsyncConnection AsynchConnection { get; internal set; } = null;
	}
}
