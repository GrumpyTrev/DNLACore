namespace DBTest
{
	/// <summary>
	/// The NowPlayingController is the Controller for the NowPlayingView. It responds to NowPlayingView commands and maintains Now Playing data in the
	/// NowPlayingViewModel
	/// </summary>
	static class NowPlayingController
	{
		/// <summary>
		/// Register for external Now Playing list change messages
		/// </summary>
		static NowPlayingController()
		{
			Mediator.RegisterPermanent( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
		}

		/// <summary>
		/// Get the Now Playing data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetNowPlayingListAsync( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( ( NowPlayingViewModel.NowPlayingPlaylist == null ) || ( LibraryId != libraryId ) )
			{
				LibraryId = libraryId;
				NowPlayingViewModel.NowPlayingPlaylist = await PlaylistAccess.GetNowPlayingListAsync( DatabasePath, LibraryId );

				// Sort the PlaylistItems by Track
				NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}

			// Publish this data
			new NowPlayingDataAvailableMessage().Send();
		}


		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Make the new contents available if already accessed
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			if ( NowPlayingViewModel.NowPlayingPlaylist != null )
			{
				NowPlayingViewModel.NowPlayingPlaylist = null;
				GetNowPlayingListAsync( LibraryId );
			}
		}

		/// <summary>
		/// The database file path
		/// </summary>
		public static string DatabasePath { private get; set; }

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		private static int LibraryId { get; set; } = -1;

		public const string NowPlayingPlaylistName = "Now Playing";
	}
}