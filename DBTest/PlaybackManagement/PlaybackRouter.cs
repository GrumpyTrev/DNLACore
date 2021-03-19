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
			// If a playback device has already been selected in the model then select it now
			if ( PlaybackManagerModel.AvailableDevice != null )
			{
				SelectPlaybackDevice( null );
			}
		}

		/// <summary>
		/// Called when the selected library has changed
		/// Stop any current playback as the playback data is aboput to be reploaed
		/// </summary>
		public void LibraryChanged() => selectedPlayback?.Stop();

		/// <summary>
		/// Called when a request has been received via the controller to play the currently selected song
		/// </summary>
		public void PlayCurrentSong()
		{
			if ( PlaybackManagerModel.CurrentSong != null )
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
					selectedPlayback?.Deselect();
				}

				// If there is no new device then clear the selection
				if ( PlaybackManagerModel.AvailableDevice == null )
				{
					selectedPlayback = null;
				}
				else
				{
					selectedPlayback = ( PlaybackManagerModel.AvailableDevice.IsLocal == true ) ? localPlayback : remotePlayback;

					selectedPlayback.Select();
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
			if ( PlaybackManagerModel.CurrentSong != null )
			{
				selectedPlayback?.Start();
			}
		}

		/// <summary>
		/// Stop playing the current song
		/// </summary>
		public void Stop() => selectedPlayback?.Stop();

		/// <summary>
		/// Called when a new song is being played. Pass this on to the controller
		/// </summary>
		public void SongStarted() => PlaybackManagementController.SongStarted();

		/// <summary>
		/// Called when the current song has finished being played
		/// </summary>
		public void SongFinished() => PlaybackManagementController.SongFinished();

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