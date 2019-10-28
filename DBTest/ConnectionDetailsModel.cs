using SQLite;

namespace DBTest
{
	static public class ConnectionDetailsModel
	{
		public static string DatabasePath { get; set; } = "";
		public static int LibraryId { get; set; } = -1;
		public static SQLiteConnection SynchConnection { get; set; } = null;
		public static SQLiteAsyncConnection AsynchConnection { get; set; } = null;
	}
}