using Android.Content;

namespace DBTest
{
	/// <summary>
	/// The PlaybackRouter is responsible for routing playback instruction to a particular playback device according to the 
	/// current selection
	/// </summary>
	class PlaybackRouter: PlaybackManagementController.IPlaybackReporter, BasePlayback.IPlaybackCallbacks
	{
		/// <summary>
		/// PlaybackRouter constructor
		/// </summary>
		public PlaybackRouter( Context context )
		{
			localPlayback = new LocalPlayback( context ) { Reporter = this };
			remotePlayback = new RemotePlayback( context ) { Reporter = this };

			// Link this router to the controller
			PlaybackManagementController.DataReporter = this;
		}

		/// <summary>
		/// Called when the owner of this router is being closed down.
		/// Pass this request on to the playback instances
		/// </summary>
		public void StopRouter()
		{
			localPlayback.StopConnection();
			remotePlayback.StopConnection();

			// As this instance is being destroyed don't leave any references hanging around
			PlaybackManagementController.DataReporter = null;
		}

		/// <summary>
		/// Called when the media data has been received or updated
		/// </summary>
		/// <param name="songsReplaced"></param>
		public void DataAvailable()
		{
			// Pass on the media data to all playback instances
			localPlayback.MediaControlDataAvailable();
			remotePlayback.MediaControlDataAvailable();

			// If a playback device has already been selected in the model then select it now
			if ( PlaybackManagerModel.AvailableDevice != null )
			{
				SelectPlaybackDevice( null );
			}
		}

		/// <summary>
		/// Called when the selected song index has been changed via the UI
		/// </summary>
		public void SongSelected()
		{
			// Pass this on to the playback instances
			localPlayback.SongSelected();
			remotePlayback.SongSelected();

			// If the new index is not set (-1) then tell the selected playback to stop playing
			// If it is set to a valid value and it should be played then the PlayRequested method will be called 
			if ( PlaybackManagerModel.CurrentSongIndex == -1 )
			{
				selectedPlayback?.Stop();
			}
		}

		/// <summary>
		/// Called when a request has been received via the controller to play the currently selected song
		/// </summary>
		public void PlayRequested()
		{
			if ( PlaybackManagerModel.CurrentSongIndex != -1 )
			{
				selectedPlayback?.Stop();
				selectedPlayback?.Play();
			}
		}

		/// <summary>
		/// Called when the selected device is available
		/// Use the device details to switch playback instances
		/// </summary>
		/// <param name="oldSelectedDevice"></param>
		public void SelectPlaybackDevice( PlaybackDevice oldSelectedDevice )
		{
			// Don't select a device if the full data has not been read in yet.
			// This can happen if a local device was last selected
			if ( PlaybackManagerModel.DataValid == true )
			{
				// Deselect the old playback instance if there was one
				if ( oldSelectedDevice != null )
				{
					selectedPlayback?.DeselectController();
				}

				// If there is no new device then clear the selection
				if ( PlaybackManagerModel.AvailableDevice == null )
				{
					selectedPlayback = null;
				}
				else
				{
					selectedPlayback = ( PlaybackManagerModel.AvailableDevice.IsLocal == true ) ? localPlayback : remotePlayback;

					selectedPlayback.SelectController();
				}
			}
		}

		/// <summary>
		/// Can the selected playback be paused
		/// </summary>
		/// <returns></returns>
		public bool CanPause() => selectedPlayback?.CanPause() ?? false;

		/// <summary>
		/// Does the selected playback support seeking forward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekBackward() => selectedPlayback?.CanSeekBackward() ?? false;

		/// <summary>
		/// Does the selected playback support seeking backward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekForward() => selectedPlayback?.CanSeekForward() ?? false;

		/// <summary>
		/// Pause the selected playback
		/// </summary>
		public void Pause() => selectedPlayback?.Pause();

		/// <summary>
		/// Seek to the specified position
		/// </summary>
		/// <param name="pos"></param>
		public void SeekTo( int pos ) => selectedPlayback?.SeekTo( pos );

		/// <summary>
		/// Start or resume playback
		/// </summary>
		public void Start()
		{
			// If no song is currently selected and there is a song available then select it
			if ( ( PlaybackManagerModel.CurrentSongIndex == -1 ) && ( ( PlaybackManagerModel.NowPlayingPlaylist?.PlaylistItems.Count ?? 0 ) > 0 ) )
			{
				PlaybackManagementController.SetSelectedSong( 0 );
			}

			if ( PlaybackManagerModel.CurrentSongIndex != -1 )
			{
				selectedPlayback?.Start();
			}
		}

		/// <summary>
		/// Called when the playback instance has changed the song index
		/// Pass this on to the controller
		/// </summary>
		public void SongIndexChanged( int songIndex ) => PlaybackManagementController.SetSelectedSong( songIndex );

		/// <summary>
		/// Called when a new song is being played. Pass this on to the controller
		/// </summary>
		/// <param name="songPlayed"></param>
		public void SongPlayed( Song songPlayed ) => PlaybackManagementController.SongPlayed( songPlayed );

		/// <summary>
		/// Called by the current playback to report the current position and duration
		/// </summary>
		/// <param name="position"></param>
		/// <param name="duration"></param>
		public void ProgressReport( int position, int duration ) => new MediaProgressMessage() { CurrentPosition = position, Duration = duration }.Send();

		/// <summary>
		/// Called when the playback has started or stopped
		/// </summary>
		public void PlayStateChanged( bool isPlaying ) => new MediaPlayingMessage() { IsPlaying = isPlaying }.Send();

		/// <summary>
		/// Play the next track
		/// </summary>
		public void PlayNext() => selectedPlayback?.PlayNext();

		/// <summary>
		/// Play the previous track
		/// </summary>
		public void PlayPrevious() => selectedPlayback?.PlayPrevious();

		/// <summary>
		/// The local playback instance
		/// </summary>
		private readonly BasePlayback localPlayback = null;

		/// <summary>
		/// The remote (DLNA) playback instance
		/// </summary>
		private readonly BasePlayback remotePlayback = null;

		/// <summary>
		/// The currently selected Playback instance
		/// </summary>
		private BasePlayback selectedPlayback = null;
	}
}