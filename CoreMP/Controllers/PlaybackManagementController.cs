using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackManagementController is the Controller for the MediaControl. It responds to MediaControl commands and maintains media player data in the
	/// PlaybackManagerModel
	/// </summary>
	internal class PlaybackManagementController
	{
		/// <summary>
		/// Register for external playing list change messages
		/// </summary>
		public PlaybackManagementController()
		{
			router = new PlaybackRouter( ( playing ) =>
			{
				if ( playing == true )
				{
					new SongStartedMessage() { SongPlayed = PlaybackManagerModel.CurrentSong }.Send();
				}
				else
				{
					new SongFinishedMessage() { SongPlayed = PlaybackManagerModel.CurrentSong }.Send();
				}
			} );

			// Register for the main data available event.
			NotificationHandler.Register( typeof( StorageController ), () => StorageDataAvailable() );
			NotificationHandler.Register( typeof( DevicesModel ), "SelectedDevice", DeviceSelected );
			SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );
		}

		/// <summary>
		/// Called when the playback data is available to be displayed
		/// </summary>
		private void StorageDataAvailable()
		{
			// Register for any post start-up song index changes
			NotificationHandler.Register( typeof( Playlists ), "CurrentSongIndex", () => SongIndexChanged() );

			// Get the sources associated with the library
			PlaybackManagerModel.Sources = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).LibrarySources;

			// Use the device currently selected in the DevicesModel
			DeviceSelected();

			// Update the model with the current song (but don't actually play it)
			SongIndexChanged( true );
		}

		/// <summary>
		/// Called when a shutdown has been detected. Pass this on to the router
		/// </summary>
		public void StopRouter() => router.StopRouter();

		/// <summary>
		/// Pass the local playback device to the router
		/// </summary>
		/// <param name="localPlayer"></param>
		public void SetLocalPlayer( BasePlayback localPlayer ) => router.SetLocalPlayback( localPlayer );

		/// <summary>
		/// Pass on a pause request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		public void MediaControlPause() => router.Pause();

		/// <summary>
		/// Pass on a play previous request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		public void MediaControlStart() => router.Start();

		/// <summary>
		/// Called when the PlaybackSelectionModel SelectedDevice has changed
		/// If this is a change then report it
		/// </summary>
		/// <param name="message"></param>
		private void DeviceSelected()
		{
			// Get the old device and store the new
			PlaybackDevice oldDevice = PlaybackManagerModel.AvailableDevice;
			PlaybackManagerModel.AvailableDevice = DevicesModel.SelectedDevice;

			// Tell the router
			router.SelectPlaybackDevice( oldDevice );
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Inform the reporter and reload the playback data
		/// </summary>
		/// <param name="_"></param>
		private void SelectedLibraryChanged( int _ )
		{
			router.LibraryChanged();
			StorageDataAvailable();
		}

		/// <summary>
		/// Called when the index of the currently playing song has changed
		/// Update the Song stored in the PlaybackModel and initiate it's playing
		/// </summary>
		private void SongIndexChanged( bool dontPlay = false )
		{
			// Determine the song and save it in the PlaybackModel 
			PlaybackModel.SongPlaying = ( Playlists.CurrentSongIndex == -1 ) ? null :
				( ( SongPlaylistItem )Playlists.GetNowPlayingPlaylist().PlaylistItems[ Playlists.CurrentSongIndex ] ).Song;

			// Store it for the PlaybackRouter to use as well
			PlaybackManagerModel.CurrentSong = PlaybackModel.SongPlaying;

			// Play the song unless requested not to
			if ( dontPlay == false )
			{
				if ( PlaybackManagerModel.CurrentSong != null )
				{
					router.PlayCurrentSong();
				}
				else
				{
					router.Stop();
				}
			}
		}

		/// <summary>
		/// The PlaybackRouter used to route requests to the selected playback device
		/// </summary>
		private readonly PlaybackRouter router;
	}
}

