namespace DBTest
{
	/// <summary>
	/// The MediaControllerController is used to maintain the model for the MediaControllerView and to process any commands from the view
	/// The MediaControllerView model data is mostly transient so the reading of storage is provided just for consistency with other controllers
	/// </summary>
	class MediaControllerController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static MediaControllerController()
		{
			MediaProgressMessage.Register( MediaProgress );
			PlaybackDeviceAvailableMessage.Register( DeviceAvailable );
			MediaPlayingMessage.Register( MediaPlaying );
			SongFinishedMessage.Register( SongFinished );
			SongSelectedMessage.Register( SongSelected );
		}

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called when the media control pause button has been pressed.
		/// Generate a MediaControlPauseMessage message
		/// </summary>
		public static void Pause() => new MediaControlPauseMessage().Send();

		/// <summary>
		/// Called when the media control seek button has been pressed.
		/// Generate a MediaControlSeekToMessage message
		/// </summary>
		public static void SeekTo( int position ) => new MediaControlSeekToMessage() { Position = position }.Send();

		/// <summary>
		/// Called when the media control start button has been pressed.
		/// Generate a MediaControlStartMessage message
		/// </summary>
		public static void Start() => new MediaControlStartMessage().Send();

		/// <summary>
		/// Called when the media control play next button has been pressed.
		/// Generate a MediaControlPlayNextMessage message
		/// </summary>
		public static void PlayNext() => new MediaControlPlayNextMessage().Send();

		/// <summary>
		/// Called when the media control play previous button has been pressed.
		/// Generate a MediaControlPlayPreviousMessage message
		/// </summary>
		public static void PlayPrevious() => new MediaControlPlayPreviousMessage().Send();

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable() => DataReporter?.DataAvailable();

		/// <summary>
		/// Called when a MediaProgressMessage has been received.
		/// Update the values held in the model and inform the DataReporter
		/// </summary>
		/// <param name="message"></param>
		private static void MediaProgress( int currentPosition, int duration )
		{
			MediaControllerViewModel.CurrentPosition = currentPosition;
			MediaControllerViewModel.Duration = duration;
			DataReporter?.MediaProgress();
		}

		/// <summary>
		/// Called when a MediaPlayingMessage is received.
		/// If this is a change of play state then let the view know
		/// </summary>
		/// <param name="message"></param>
		private static void MediaPlaying( bool isPlaying )
		{
			if ( MediaControllerViewModel.IsPlaying != isPlaying )
			{
				MediaControllerViewModel.IsPlaying = isPlaying;
				DataReporter?.PlayStateChanged();
			}
		}

		/// <summary>
		/// Called when a SongFinishedMessage has been received
		/// Update the model and report the change
		/// </summary>
		/// <param name="_"></param>
		private static void SongFinished( Song _ )
		{
			MediaControllerViewModel.SongPlaying = null;
			DataReporter?.SongPlayingChanged();
		}

		/// <summary>
		/// Called when a SongSelectedMessage has been received
		/// Get the selected Song from the Now Playing playlist and report it
		/// </summary>
		private static void SongSelected()
		{
			int currentSongIndex = Playlists.CurrentSongIndex;

			// Only pass this on if a song has been selected. If no song is selected then SongFinished should have been called.
			if ( currentSongIndex != -1 )
			{
				MediaControllerViewModel.SongPlaying = 
					( ( SongPlaylistItem )Playlists.GetNowPlayingPlaylist( ConnectionDetailsModel.LibraryId ).PlaylistItems[ currentSongIndex ] ).Song;
				DataReporter?.SongPlayingChanged();
			}
		}

		/// <summary>
		/// Called when the PlaybackDeviceAvailableMessage message is received
		/// If this is a change then report it
		/// </summary>
		/// <param name="message"></param>
		private static void DeviceAvailable( PlaybackDevice newDevice )
		{
			// If the view data is not available yet, just update the model.
			// Otherwise report to the view and then update the model
			MediaControllerViewModel.PlaybackDeviceAvailable = ( newDevice != null );

			if ( dataReporter.DataAvailable == true )
			{
				DataReporter?.DeviceAvailable();
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IMediaReporter DataReporter
		{
			get => ( IMediaReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IMediaReporter : DataReporter.IReporter
		{
			void DeviceAvailable();
			void PlayStateChanged();
			void MediaProgress();
			void SongPlayingChanged();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}