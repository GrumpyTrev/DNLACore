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

			// Initialise the rest of the database if required
			InitialiseDatabase();
		}

		/// <summary>
		/// Make sure that the database exists
		/// </summary>
		private void InitialiseDatabase()
		{
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
			}
			catch ( SQLite.SQLiteException )
			{
			}
		}
	}
}
