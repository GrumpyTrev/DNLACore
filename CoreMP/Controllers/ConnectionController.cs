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
					ConnectionDetailsModel.SynchConnection.CreateTable<Library>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Source>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Artist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Album>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Song>();
					ConnectionDetailsModel.SynchConnection.CreateTable<ArtistAlbum>();
					ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylistItem>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Tag>();
					ConnectionDetailsModel.SynchConnection.CreateTable<TaggedAlbum>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Autoplay>();
					ConnectionDetailsModel.SynchConnection.CreateTable<GenrePopulation>();
					ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylistItem>();
				}

				// Check for a Playback record which will tell us the currently selected library
				currentLibraryId = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault().DBLibraryId;
			}
			catch ( SQLite.SQLiteException )
			{
			}

			return currentLibraryId;
		}
	}
}
