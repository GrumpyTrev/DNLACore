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
			// Don't register for the NowPlayingClearedMessage as the list is always refreshed when new songs are added
			Mediator.RegisterPermanent( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
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
			if ( ( NowPlayingViewModel.NowPlayingPlaylist == null ) || ( NowPlayingViewModel.LibraryId != libraryId ) )
			{
				NowPlayingViewModel.LibraryId = libraryId;

				// Get the list witrh artists
				NowPlayingViewModel.NowPlayingPlaylist = await PlaylistAccess.GetNowPlayingListAsync( NowPlayingViewModel.LibraryId, true );

				// Sort the PlaylistItems by Track
				NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );

				// Get the selected song
				NowPlayingViewModel.SelectedSong = PlaybackAccess.GetSelectedSong();
			}

			// Publish this data
			Reporter?.NowPlayingDataAvailable();
		}

		/// <summary>
		/// Set the selected song in the database and raise the SongSelectedMessage
		/// Don't update the model at this stage. Update it when the SongSelectedMessage is received
		/// </summary>
		public static void SetSelectedSong( int songIndex )
		{
			PlaybackAccess.SetSelectedSong( songIndex );
			new SongSelectedMessage() { ItemNo = songIndex }.Send();
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
				// Force a total refresh  by clearing the previous results
				// Hence no need to register for the playlist cleared message (this assumes that the 'added' always follows a 'cleared'????
				NowPlayingViewModel.NowPlayingPlaylist = null;
				GetNowPlayingListAsync( NowPlayingViewModel.LibraryId );
			}
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// Update the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object message )
		{
			// Only process this if the playlist has been read at least once
			if ( NowPlayingViewModel.NowPlayingPlaylist != null )
			{
				NowPlayingViewModel.SelectedSong = ( ( SongSelectedMessage )message ).ItemNo;
				Reporter?.SongSelected();
			}
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data and the filter and then reload
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message )
		{
			// Set the new library
			NowPlayingViewModel.LibraryId = ( message as SelectedLibraryChangedMessage ).SelectedLibrary.Id;

			// Clear the displayed data and filter
			NowPlayingViewModel.NowPlayingPlaylist = null;
			NowPlayingViewModel.SelectedSong = -1;

			// Publish the data
			Reporter?.NowPlayingDataAvailable();

			// Reread the data
			GetNowPlayingListAsync( NowPlayingViewModel.LibraryId );
		}

		/// <summary>
		/// The name given to the Now Playing playlist
		/// </summary>
		public const string NowPlayingPlaylistName = "Now Playing";

		// The interface instance used to report back controller results
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void NowPlayingDataAvailable();
			void SongSelected();
		}
	}
}
