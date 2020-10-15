using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaybackManagementController is the Controller for the MediaControl. It responds to MediaControl commands and maintains media player data in the
	/// PlaybackManagerModel
	/// </summary>
	public static class PlaybackManagementController
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
			Mediator.RegisterPermanent( PlayRequested, typeof( PlayCurrentSongMessage ) );
		}

		/// <summary>
		/// Get the media control data. This consists of the Now Playing data and the currently playing song
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetMediaControlData( int libraryId )
		{
			// Check if the PlaylistGetMediaControlDataAsyncs details for the library have already been obtained
			if ( ( PlaybackManagerModel.NowPlayingPlaylist == null ) || ( PlaybackManagerModel.LibraryId != libraryId ) )
			{
				PlaybackManagerModel.LibraryId = libraryId;

				// All Playlists are read at startup. So wait until that is available and then carry out the rest of the initialisation
				StorageController.RegisterInterestInDataAvailable( PlaylistDataAvailable );
			}

			// Publish this data unless it is still being obtained
			if ( PlaybackManagerModel.DataValid == true )
			{
				Reporter?.MediaControlDataAvailable();
			}
		}

		/// <summary>
		/// Called when the Playlist data is available to be displayed
		/// </summary>
		private static void PlaylistDataAvailable( object _ = null )
		{
			// This is getting the same list as the NowPlayingController. So let it do any sorting.
			PlaybackManagerModel.NowPlayingPlaylist = Playlists.GetNowPlayingPlaylist( PlaybackManagerModel.LibraryId );

			// Get the selected song
			PlaybackManagerModel.CurrentSongIndex = PlaybackDetails.SongIndex;

			// Get the sources associated with the library
			PlaybackManagerModel.Sources = Sources.GetSourcesForLibrary( PlaybackManagerModel.LibraryId );

			PlaybackManagerModel.DataValid = true;

			// Let the views know that playback data is available
			Reporter?.MediaControlDataAvailable();

			// If a play request has been received whilst accessing this data then process it now
			if ( playRequestPending == true )
			{
				playRequestPending = false;
				Reporter?.PlayRequested();
			}
		}

		/// <summary>
		/// Set the selected song in the database and raise the SongSelectedMessage
		/// </summary>
		public static void SetSelectedSong( int songIndex )
		{
			PlaybackDetails.SongIndex = songIndex;
			new SongSelectedMessage() { ItemNo = songIndex }.Send();
		}

		/// <summary>
		/// Called when a new song is being played by the service.
		/// Pass this on to the relevant controller, not this one
		/// </summary>
		/// <param name="songPlayed"></param>
		public static void SongPlayed( Song songPlayed ) => new SongPlayedMessage() { SongPlayed = songPlayed }.Send();

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// Update the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object message )
		{
			// Only process this if there is a valid playlist, otherwise just wait for the data to become available
			if ( PlaybackManagerModel.NowPlayingPlaylist != null )
			{
				// Update the selected song in the model and report the selection
				PlaybackManagerModel.CurrentSongIndex = ( ( SongSelectedMessage )message ).ItemNo;
				Reporter?.SongSelected();
			}
		}

		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Force the playback data to be read again and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			if ( ( ( PlaylistSongsAddedMessage )message ).Playlist == PlaybackManagerModel.NowPlayingPlaylist )
			{
				PlaybackManagerModel.NowPlayingPlaylist = null;
				GetMediaControlData( PlaybackManagerModel.LibraryId );
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
					Reporter?.SelectPlaybackDevice( oldDevice );
				}
			}
			// If there was no available device then save the new device and report the change
			else if ( oldDevice == null )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				Reporter?.SelectPlaybackDevice( oldDevice );
			}
			// If the old and new are different type (local/remote) then report the change
			else if ( oldDevice.IsLocal != newDevice.IsLocal )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				Reporter?.SelectPlaybackDevice( oldDevice );
			}
			// If both devices are remote but different then report the change
			else if ( ( oldDevice.IsLocal == false ) && ( newDevice.IsLocal == false ) && ( oldDevice.FriendlyName != newDevice.FriendlyName ) )
			{
				PlaybackManagerModel.AvailableDevice = newDevice;
				Reporter?.SelectPlaybackDevice( oldDevice );
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
			PlaybackManagerModel.LibraryId = ( message as SelectedLibraryChangedMessage ).SelectedLibrary;

			// Clear the now playing data and reset the selected song
			PlaybackManagerModel.NowPlayingPlaylist = null;
			SetSelectedSong( -1 );

			// Publish the data
			Reporter?.MediaControlDataAvailable();

			// Reread the data
			GetMediaControlData( PlaybackManagerModel.LibraryId );
		}

		/// <summary>
		/// Called in response to the receipt of a PlayCurrentSongMessage
		/// If we are in the middle of obtaining a new playing list then defer the request until the data is available
		/// </summary>
		private static void PlayRequested( object message )
		{
			// If there is a current song then report this request
			if ( PlaybackManagerModel.CurrentSongIndex != -1 )
			{
				Reporter?.PlayRequested();
			}
			else
			{
				playRequestPending = true;
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// Keep track of when a play request is received whilst the data is being read in
		/// </summary>
		private static bool playRequestPending = false;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void MediaControlDataAvailable();
			void SongSelected();
			void SelectPlaybackDevice( PlaybackDevice oldSelectedDevice );
			void PlayRequested();
		}
	}
}