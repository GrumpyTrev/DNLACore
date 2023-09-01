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

			NotificationHandler.Register( typeof( PlaybackSelectionModel ), "SelectedDevice", DeviceAvailable );
			SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );
			PlaySongMessage.Register( PlaySong );
		}

		/// <summary>
		/// Called when the playback data is available to be displayed
		/// </summary>
		private void StorageDataAvailable()
		{
			// Get the sources associated with the library
			PlaybackManagerModel.Sources = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).LibrarySources.ToList();

			PlaybackManagerModel.DataValid = true;

			// Let the views know that playback data is available
			router.DataAvailable();
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
		private void DeviceAvailable()
		{
			PlaybackDevice oldDevice = PlaybackManagerModel.AvailableDevice;

			// Check for no new device
			if ( PlaybackSelectionModel.SelectedDevice == null )
			{
				// If there was an exisiting availabel device then repoprt this change
				if ( oldDevice != null )
				{
					PlaybackManagerModel.AvailableDevice = null;
					router.SelectPlaybackDevice( oldDevice );
				}
			}
			// If there was no available device then save the new device and report the change
			else if ( oldDevice == null )
			{
				PlaybackManagerModel.AvailableDevice = PlaybackSelectionModel.SelectedDevice;
				router.SelectPlaybackDevice( oldDevice );
			}
			// If the old and new are different type (local/remote) then report the change
			else if ( oldDevice.IsLocal != PlaybackSelectionModel.SelectedDevice.IsLocal )
			{
				PlaybackManagerModel.AvailableDevice = PlaybackSelectionModel.SelectedDevice;
				router.SelectPlaybackDevice( oldDevice );
			}
			// If both devices are remote but different then report the change
			else if ( ( oldDevice.IsLocal == false ) && ( PlaybackSelectionModel.SelectedDevice.IsLocal == false ) &&
				( oldDevice.FriendlyName != PlaybackSelectionModel.SelectedDevice.FriendlyName ) )
			{
				PlaybackManagerModel.AvailableDevice = PlaybackSelectionModel.SelectedDevice;
				router.SelectPlaybackDevice( oldDevice );
			}
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
		/// Called in response to the receipt of a PlaySongMessage
		/// </summary>
		private void PlaySong( Song songToPlay, bool dontPlay )
		{
			PlaybackManagerModel.CurrentSong = songToPlay;

			if ( ( PlaybackManagerModel.CurrentSong != null ) && ( dontPlay == false ) )
			{
				router.PlayCurrentSong();
			}
			else
			{
				router.Stop();
			}
		}

		/// <summary>
		/// The PlaybackRouter used to route requests to the selected playback device
		/// </summary>
		private readonly PlaybackRouter router;
	}
}

