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
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( DeviceAvailable, typeof( PlaybackDeviceAvailableMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( PlayCurrentSong, typeof( PlayCurrentSongMessage ) );
			Mediator.RegisterPermanent( MediaControlPause, typeof( MediaControlPauseMessage ) );
			Mediator.RegisterPermanent( MediaControlPlayNext, typeof( MediaControlPlayNextMessage ) );
			Mediator.RegisterPermanent( MediaControlPlayPrevious, typeof( MediaControlPlayPreviousMessage ) );
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

			// This is getting the same list as the NowPlayingController. So let it do any sorting.
			PlaybackManagerModel.NowPlayingPlaylist = Playlists.GetNowPlayingPlaylist( PlaybackManagerModel.LibraryId );

			// Get the selected song
			PlaybackManagerModel.CurrentSongIndex = Playlists.CurrentSong;

			// Get the sources associated with the library
			PlaybackManagerModel.Sources = Sources.GetSourcesForLibrary( PlaybackManagerModel.LibraryId );

			PlaybackManagerModel.DataValid = true;

			// Let the views know that playback data is available
			DataReporter?.DataAvailable();

			// If a play request has been received whilst accessing this data then process it now
			if ( playRequestPending == true )
			{
				playRequestPending = false;
				DataReporter?.PlayCurrentSong();
			}
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
		public static void SongFinished()
		{
			new SongFinishedMessage() { SongPlayed = PlaybackManagerModel.CurrentSong }.Send();

			if ( PlaybackManagerModel.CurrentSongIndex < ( PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count - 1 ) )
			{
				// Play the next song
				Playlists.CurrentSong++;

				// Use a message to play the song so that the request is processed after the index has been updated
				new PlayCurrentSongMessage().Send();
			}
			else if ( ( PlaybackModeModel.RepeatOn == true ) && ( PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				// Play the first song
				Playlists.CurrentSong = 0;

				// Use a message to play the song so that the request is processed after the index has been updated
				new PlayCurrentSongMessage().Send();
			}
			else
			{
				Playlists.CurrentSong = -1;
			}
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received.
		/// This is only an update of the songs index. Any play request will follow
		/// Update the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object _ )
		{
			// Only process this if there is a valid playlist, otherwise just wait for the data to become available
			if ( PlaybackManagerModel.NowPlayingPlaylist != null )
			{
				// Update the selected song in the model and report the selection
				PlaybackManagerModel.CurrentSongIndex = Playlists.CurrentSong;

				if ( PlaybackManagerModel.CurrentSongIndex == -1 )
				{
					DataReporter?.Stop();
				}
			}
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// If it's the Now Playing list then force the playback data to be read again
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			if ( ( ( PlaylistSongsAddedMessage )message ).Playlist == PlaybackManagerModel.NowPlayingPlaylist )
			{
				PlaybackManagerModel.NowPlayingPlaylist = null;
				StorageDataAvailable();
			}
		}

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
		/// Called in response to the receipt of a PlayCurrentSongMessage
		/// If we are in the middle of obtaining a new playing list then defer the request until the data is available
		/// </summary>
		private static void PlayCurrentSong( object _ )
		{
			// If there is a current song then report this request
			if ( PlaybackManagerModel.CurrentSong != null )
			{
				DataReporter?.PlayCurrentSong();
			}
			else
			{
				playRequestPending = true;
			}
		}

		/// <summary>
		/// Pass on a pause request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPause( object _ ) => DataReporter?.Pause();

		/// <summary>
		/// Pass on a play next request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPlayNext( object _ )
		{
			DataReporter?.Stop();

			if ( PlaybackManagerModel.CurrentSongIndex < ( PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count - 1 ) )
			{
				Playlists.CurrentSong++;
			}
			else
			{
				Playlists.CurrentSong = 0;
			}

			// Use a message to play the song so that the request is processed after the index has been updated
			new PlayCurrentSongMessage().Send();
		}

		/// <summary>
		/// Pass on a play previous request to the reporter
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPlayPrevious( object _ )
		{
			DataReporter?.Stop();

			if ( PlaybackManagerModel.CurrentSongIndex > 0 )
			{
				Playlists.CurrentSong--;
			}
			else
			{
				Playlists.CurrentSong = PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count - 1;
			}

			// Use a message to play the song so that the request is processed after the index has been updated
			new PlayCurrentSongMessage().Send();
		}

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
		/// Keep track of when a play request is received whilst the data is being read in
		/// </summary>
		private static bool playRequestPending = false;

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