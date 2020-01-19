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
			Mediator.RegisterPermanent( SongsCleared, typeof( NowPlayingClearedMessage ) );
			Mediator.RegisterPermanent( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( DeviceAvailable, typeof( PlaybackDeviceAvailableMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
		}

		/// <summary>
		/// Get the media control data. This consists of the Now Playing data and the currently playing song
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetMediaControlDataAsync( int libraryId, bool songsReplaced = false )
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
				PlaybackManagerModel.Sources = await LibraryAccess.GetSourcesAsync( PlaybackManagerModel.LibraryId );
			}

			// Publish this data
			Reporter?.MediaControlDataAvailable( songsReplaced );
		}

		/// <summary>
		/// Set the selected song in the database and raise the SongSelectedMessage
		/// </summary>
		public static async Task SetSelectedSongAsync( int songIndex )
		{
			await PlaybackAccess.SetSelectedSongAsync( songIndex );
			PlaybackManagerModel.CurrentSongIndex = songIndex;

			new SongSelectedMessage() { ItemNo = songIndex }.Send();
		}

		/// <summary>
		/// Called when a new song is being played.
		/// Pass this on to the relevane controller, not this one
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
			// Only process this if the playlist has been read at least once
			if ( PlaybackManagerModel.NowPlayingPlaylist != null )
			{
				// Report the old index so that change logic can be applied
				int oldIndex = PlaybackManagerModel.CurrentSongIndex;
				PlaybackManagerModel.CurrentSongIndex = ( ( SongSelectedMessage )message ).ItemNo;
				Reporter?.SongSelected( oldIndex );
			}
		}

		/// <summary>
		/// Called when the NowPlayingClearedMessage is received
		/// Clear the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongsCleared( object message )
		{
			PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Clear();
			Reporter?.SongsCleared();
		}

		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Force the playback data to be read again and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			PlaybackManagerModel.NowPlayingPlaylist = null;
			GetMediaControlDataAsync( PlaybackManagerModel.LibraryId, ( ( NowPlayingSongsAddedMessage )message ).SongsReplaced );
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
			PlaybackManagerModel.LibraryId = ( message as SelectedLibraryChangedMessage ).SelectedLibrary.Id;

			// Clear the now playing data and reset the selected song
			PlaybackManagerModel.NowPlayingPlaylist = null;
			await SetSelectedSongAsync( -1 );

			// Publish the data
			Reporter?.SongsCleared();
			Reporter?.MediaControlDataAvailable( false );

			// Reread the data
			GetMediaControlDataAsync( PlaybackManagerModel.LibraryId );
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void MediaControlDataAvailable( bool songsReplaced );
			void SongSelected( int oldIndex );
			void SongsCleared();
			void SelectPlaybackDevice( Device oldSelectedDevice );
		}
	}
}