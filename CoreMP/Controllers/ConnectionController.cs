using SQLite;

namespace CoreMP
{
	internal class ConnectionController
	{
		public void InitialiseConnection(string storageName )
		{
			// The synchronous and aynchronous connectionn
			ConnectionDetailsModel.SynchConnection = new SQLiteConnection( storageName );
			ConnectionDetailsModel.AsynchConnection = new SQLiteAsyncConnection( storageName )
			{
				// Tracing when required
				Tracer = ( message ) => Logger.Log( message ),

				// Tracing currently required
				Trace = true
			};

			// Initialise the rest of the ConnectionDetailsModel if required
			ConnectionDetailsModel.LibraryId = InitialiseDatabase();
		}

		/// <summary>
		/// Make sure that the database exists and extract the current library
		/// </summary>
		private int InitialiseDatabase()
		{
			int currentLibraryId = -1;

			bool createTables = false;

			try
			{
				if ( createTables == true )
				{
					// Create the tables if they don't already exist
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Library>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Source>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Artist>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Album>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Song>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<ArtistAlbum>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylist>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylistItem>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<Tag>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<TaggedAlbum>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylist>();
					_ = ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylistItem>();
				}

				// Check for a Playback record which will tell us the currently selected library
				currentLibraryId = ConnectionDetailsModel.SynchConnection.Table<SQLitePlayback>().FirstOrDefault().LibraryId;
			}
			catch ( SQLite.SQLiteException )
			{
			}

			return currentLibraryId;
		}
	}
}
