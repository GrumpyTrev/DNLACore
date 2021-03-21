namespace DBTest
{
	/// <summary>
	/// The PlaybackManagementController is the Controller for the MediaControl. It responds to MediaControl commands and maintains media player data in the
	/// PlaybackManagerModel
	/// </summary>
	class PlaybackManagementController
	{
		/// <summary>
		/// Register for external playing list change messages
		/// </summary>
		static PlaybackManagementController()
		{
			Mediator.RegisterPermanent( DeviceAvailable, typeof( PlaybackDeviceAvailableMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( PlaySong, typeof( PlaySongMessage ) );
			Mediator.RegisterPermanent( MediaControlPause, typeof( MediaControlPauseMessage ) );
			Mediator.RegisterPermanent( MediaControlSeekTo, typeof( MediaControlSeekToMessage ) );
			Mediator.RegisterPermanent( MediaControlStart, typeof( MediaControlStartMessage ) );
		}

		/// <summary>
		/// Get the Controller data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called when the playback data is available to be displayed
		/// </summary>
		private static void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			PlaybackManagerModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the sources associated with the library
			PlaybackManagerModel.Sources = Sources.GetSourcesForLibrary( PlaybackManagerModel.LibraryId );

			PlaybackManagerModel.DataValid = true;

			// Let the views know that playback data is available
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when a new song is being played.
		/// Pass this on to the relevant controller, not this one
		/// </summary>
		/// <param name="songPlayed"></param>
		public static void SongStarted() => new SongStartedMessage() { SongPlayed = PlaybackManagerModel.CurrentSong }.Send();

		/// <summary>
		/// Called when the current song has finished being played.
		/// Select the next song to play
		/// </summary>
		/// <param name="songPlayed"></param>
		public static void SongFinished() => new SongFinishedMessage() { SongPlayed = PlaybackManagerModel.CurrentSong }.Send();

		/// <summary>
		/// Called when the PlaybackDeviceAvailableMessage message is received
		/// If this is a change then report it
		/// </summary>
		/// <param name="message"></param>
		private static void DeviceAvailable( object message )
		{
			PlaybackDevice newDevice = ( message as PlaybackDeviceAvailableMessage ).SelectedDevice;
			PlaybackDevice oldDevice = PlaybackManagerModel.AvailableDevice;

			// Check for no new device
			if ( newDevice == null )
			{
				// If there was an exisiting availabel device then repoprt this change
				if ( oldDevice != null )
				{
					PlaybackManagerModel.AvailableDevice = null;
					DataReporter?.SelectPlaybackDevice( oldDevice );
				}
			}
			// If there was no available device then save the new device and report the change
			else if ( oldDevice == null )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				DataReporter?.SelectPlaybackDevice( oldDevice );
			}
			// If the old and new are different type (local/remote) then report the change
			else if ( oldDevice.IsLocal != newDevice.IsLocal )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				DataReporter?.SelectPlaybackDevice( oldDevice );
			}
			// If both devices are remote but different then report the change
			else if ( ( oldDevice.IsLocal == false ) && ( newDevice.IsLocal == false ) && ( oldDevice.FriendlyName != newDevice.FriendlyName ) )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				DataReporter?.SelectPlaybackDevice( oldDevice );
			}
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Inform the reporter and reload the playback data
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object _ )
		{
			DataReporter?.LibraryChanged();
			StorageDataAvailable();
		}

		/// <summary>
		/// Called in response to the receipt of a PlaySongMessage
		/// </summary>
		private static void PlaySong( object message )
		{
			PlaySongMessage playSong = message as PlaySongMessage;
			PlaybackManagerModel.CurrentSong = playSong.SongToPlay;

			if ( ( PlaybackManagerModel.CurrentSong != null ) && ( playSong.DontPlay == false ) )
			{
				DataReporter?.PlayCurrentSong();
			}
			else
			{
				DataReporter?.Stop();
			}
		}

		/// <summary>
		/// Pass on a pause request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPause( object _ ) => DataReporter?.Pause();

		/// <summary>
		/// Pass on a seek request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlSeekTo( object message ) => DataReporter?.SeekTo( ( ( MediaControlSeekToMessage )message ).Position );

		/// <summary>
		/// Pass on a play previous request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlStart( object _ ) => DataReporter?.Start();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IPlaybackReporter DataReporter
		{
			get => ( IPlaybackReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IPlaybackReporter : DataReporter.IReporter
		{
			void SelectPlaybackDevice( PlaybackDevice oldSelectedDevice );
			void PlayCurrentSong();
			void Pause();
			void SeekTo( int position );
			void Start();
			void LibraryChanged();
			void Stop();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}