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
			Mediator.RegisterPermanent( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
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
		public static async void GetMediaControlDataAsync( int libraryId )
		{
			// Check if the PlaylistGetMediaControlDataAsyncs details for the library have already been obtained
			if ( ( PlaybackManagerModel.NowPlayingPlaylist == null ) || ( PlaybackManagerModel.LibraryId != libraryId ) )
			{
				PlaybackManagerModel.LibraryId = libraryId;
				PlaybackManagerModel.NowPlayingPlaylist = await PlaylistAccess.GetNowPlayingListAsync( PlaybackManagerModel.LibraryId, true );

				// Sort the PlaylistItems by Track
				PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );

				// Get the selected song
				PlaybackManagerModel.CurrentSongIndex = await PlaybackAccess.GetSelectedSongAsync();

				// Get the sources associated with the library
				PlaybackManagerModel.Sources = Sources.GetSourcesForLibrary( PlaybackManagerModel.LibraryId );
			}

			// Publish this data
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
		public static async Task SetSelectedSongAsync( int songIndex )
		{
			await PlaybackAccess.SetSelectedSongAsync( songIndex );
			new SongSelectedMessage() { ItemNo = songIndex }.Send();
		}

		/// <summary>
		/// Called when a new song is being played by the service.
		/// Pass this on to the relevant controller, not this one
		/// </summary>
		/// <param name="songPlayed"></param>
		public static void SongPlayed( Song songPlayed )
		{
			new SongPlayedMessage() { SongPlayed = songPlayed }.Send();
		}

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
			PlaybackManagerModel.NowPlayingPlaylist = null;
			GetMediaControlDataAsync( PlaybackManagerModel.LibraryId );
		}

		/// <summary>
		/// Called when the PlaybackDeviceAvailableMessage message is received
		/// If this is a change then report it
		/// </summary>
		/// <param name="message"></param>
		private static void DeviceAvailable( object message )
		{
			Device newDevice = ( message as PlaybackDeviceAvailableMessage ).SelectedDevice;
			Device oldDevice = PlaybackManagerModel.AvailableDevice;

			// If there was no available device then save the new device and report the change
			if ( oldDevice == null )
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
		private static async void SelectedLibraryChanged( object message )
		{
			// Set the new library
			PlaybackManagerModel.LibraryId = ( message as SelectedLibraryChangedMessage ).SelectedLibrary;

			// Clear the now playing data and reset the selected song
			PlaybackManagerModel.NowPlayingPlaylist = null;
			await SetSelectedSongAsync( -1 );

			// Publish the data
			Reporter?.MediaControlDataAvailable();

			// Reread the data
			GetMediaControlDataAsync( PlaybackManagerModel.LibraryId );
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
			void SelectPlaybackDevice( Device oldSelectedDevice );
			void PlayRequested();
		}
	}
}