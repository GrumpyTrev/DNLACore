using SQLite;

namespace CoreMP
{
	public static class ConnectionDetailsModel
	{
		/// <summary>
		/// The Identity of the currently selected library
		/// </summary>
		private static int libraryId = -1;
		public static int LibraryId
		{
			get => libraryId;
			set
			{
				libraryId = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		public static SQLiteConnection SynchConnection { get; set; } = null;
		public static SQLiteAsyncConnection AsynchConnection { get; set; } = null;
	}
}
