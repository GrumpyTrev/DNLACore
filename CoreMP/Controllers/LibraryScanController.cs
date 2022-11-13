using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The LibraryScanController carries out the asynchronous actions involved in scanning a library
	/// </summary>
	public static class LibraryScanController
	{
		/// <summary>
		/// Static constructor. Register for UPnP devices
		/// </summary>
		static LibraryScanController() => CoreMPApp.RegisterPlaybackCapabilityCallback( deviceCallback );

		/// <summary>
		/// Asynchronous method called to carry out a library scan
		/// </summary>
		/// <param name="libraryToScan"></param>
		public static async void ScanLibraryAsynch( Library libraryToScan )
		{
			// Ignore this if there is already a scan in progress
			if ( scanInProgress == false )
			{
				// The scan may already have finished 
				if ( ( LibraryScanModel.LibraryBeingScanned == libraryToScan ) && ( LibraryScanModel.UnmatchedSongs != null ) )
				{
					// Report the completion back through the delegate
					ScanReporter?.ScanFinished();
				}
				else
				{
					// Prevent this from being executed twice
					scanInProgress = true;

					// Save the library being scanned
					LibraryScanModel.LibraryBeingScanned = libraryToScan;

					// Keep track of any existing songs that have not been matched in the scan
					LibraryScanModel.UnmatchedSongs = new List<Song>();

					await Task.Run( async () =>
					{
						// Iterate all the sources associated with this library. Get the songs as well as we're going to need them below
						foreach ( Source source in LibraryScanModel.LibraryBeingScanned.Sources )
						{
							source.GetSongs();
						}

						foreach ( Source source in LibraryScanModel.LibraryBeingScanned.Sources )
						{
							// Add the songs from this source to a dictionary
							Dictionary<string, Song> pathLookup = new Dictionary<string, Song>( source.Songs.ToDictionary( song => song.Path ) );

							// Reset the scan action for all Songs
							source.Songs.ForEach( song => song.ScanAction = Song.ScanActionType.NotMatched );

							// Use a SongStorage instance to check for song changes
							SongStorage scanStorage = new SongStorage( LibraryScanModel.LibraryBeingScanned.Id, source, pathLookup );

							// Check the source scanning method
							if ( source.AccessMethod == Source.AccessType.FTP )
							{
								// Scan using the generic FTPScanner but with our callbacks
								await new FTPScanner( scanStorage ) { CancelRequested = CancelRequested }.Scan( source.ScanSource );
							}
							else if ( source.AccessMethod == Source.AccessType.Local )
							{
								// Scan using the generic InternalScanner but with our callbacks
								await new InternalScanner( scanStorage ) { CancelRequested = CancelRequested }.Scan( source.ScanSource );
							}
							else if ( source.AccessMethod == Source.AccessType.UPnP )
							{
								// Scan using the generic UPnPScanner but with our callbacks
								await new UPnPScanner( scanStorage ) { CancelRequested = CancelRequested, RemoteDevices = RemoteDevices }.Scan( source.ScanSource );
							}

							// Add any unmatched and modified songs to a list that'll be processed when all sources have been scanned
							LibraryScanModel.UnmatchedSongs.AddRange( pathLookup.Values.Where( song => song.ScanAction == Song.ScanActionType.NotMatched ) );

							// Keep track of any library changes
							LibraryScanModel.LibraryModified |= scanStorage.LibraryModified;
						}
					} );

					scanInProgress = false;

					// Report the completion back through the delegate
					ScanReporter?.ScanFinished();
				}
			}
		}

		/// <summary>
		/// Reset the controller between scans
		/// </summary>
		public static void ResetController() => LibraryScanModel.ClearModel();

		/// <summary>
		/// Delete the list of songs from the library
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static async void DeleteSongsAsync()
		{
			// Prevent this from being executed twice
			DeleteInProgress = true;

			// Keep track of any albums that are deleted so that other controllers can be notified
			List<int> deletedAlbumIds = new List<int>();

			await Task.Run( () =>
			{
				// Delete all the Songs.
				Songs.DeleteSongs( LibraryScanModel.UnmatchedSongs );

				// Delete all the PlaylistItems associated with the songs. No need to wait for this
				Playlists.DeletePlaylistItems( LibraryScanModel.UnmatchedSongs.Select( song => song.Id ).ToHashSet() );

				// Form a distinct list of all the ArtistAlbum items referenced by the deleted songs
				IEnumerable<int> artistAlbumIds = LibraryScanModel.UnmatchedSongs.Select( song => song.ArtistAlbumId ).Distinct();

				// Check if any of these ArtistAlbum items are now empty and need deleting
				foreach ( int id in artistAlbumIds )
				{
					CheckForAlbumDeletion( id, out int deletedAlbumId, out Artist deletedArtist );

					if ( deletedAlbumId != -1 )
					{
						deletedAlbumIds.Add( deletedAlbumId );
					}
				}
			} );

			if ( deletedAlbumIds.Count > 0 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = deletedAlbumIds }.Send();
			}

			DeleteInProgress = false;
			DeleteReporter?.DeleteFinished();
		}

		/// <summary>
		/// Delete a single song from storage
		/// </summary>
		/// <param name="songToDelete"></param>
		/// <returns></returns>
		public static Artist DeleteSong( Song songToDelete )
		{
			// Delete the song.
			Songs.DeleteSong( songToDelete );

			// Delete all the PlaylistItems associated with the song. No need to wait for this
			Playlists.DeletePlaylistItems( new HashSet<int> { songToDelete.Id } );

			// Check if this ArtistAlbum item is now empty and need deleting
			CheckForAlbumDeletion( songToDelete.ArtistAlbumId, out int deletedAlbumId, out Artist deletedArtist );

			if ( deletedAlbumId != -1 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = new List<int> { deletedAlbumId } }.Send();
			}

			return deletedArtist;
		}

		/// <summary>
		/// Called when a new remote media device has been detected
		/// </summary>
		/// <param name="device"></param>
		private static void NewDeviceDetected( PlaybackDevice device )
		{
			// Add this device to the model if it supports content discovery
			if ( device.ContentUrl.Length > 0 )
			{
				RemoteDevices.AddDevice( device );
			}
		}

		/// <summary>
		/// Called when a remote media device is no longer available
		/// </summary>
		/// <param name="device"></param>
		private static void DeviceNotAvailable( PlaybackDevice device ) => RemoteDevices.RemoveDevice( device );

		/// <summary>
		/// Check if the specified ArtistAlbum should be deleted and any associated Album or Artist
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		private static void CheckForAlbumDeletion( int artistAlbumId, out int deletedAlbumId, out Artist deletedArtist )
		{
			// Keep track in the Tuple of any albums or artists deleted
			deletedAlbumId = -1;
			deletedArtist = null;

			// Refresh the contents of the ArtistAlbum
			ArtistAlbum artistAlbum = ArtistAlbums.GetArtistAlbumById( artistAlbumId );
			artistAlbum.Songs = Songs.GetArtistAlbumSongs( artistAlbum.Id );

			// Check if this ArtistAlbum is being referenced by any songs
			if ( artistAlbum.Songs.Count == 0 )
			{
				// Delete the ArtistAlbum as it is no longer being referenced
				ArtistAlbums.DeleteArtistAlbum( artistAlbum );

				// Remove this ArtistAlbum from the Artist
				Artist artist = Artists.GetArtistById( artistAlbum.ArtistId );
				artist.ArtistAlbums.Remove( artistAlbum );

				// Does the associated Artist have any other Albums
				if ( artist.ArtistAlbums.Count == 0 )
				{
					// Delete the Artist
					Artists.DeleteArtist( artist );
					deletedArtist = artist;
				}

				// Does any other ArtistAlbum reference the Album
				if ( ArtistAlbums.ArtistAlbumCollection.Any( art => art.AlbumId == artistAlbum.AlbumId ) == false )
				{
					// Not referenced by any ArtistAlbum. so delete it
					Albums.DeleteAlbum( artistAlbum.Album );
					deletedAlbumId = artistAlbum.AlbumId;
				}
			}
		}

		/// <summary>
		/// Flag indicating whether or not the this controller is busy scanning a library
		/// </summary>
		private static bool scanInProgress = false;

		/// <summary>
		/// Flag indicating whether or not the this controller is busy deleting unmatched songs
		/// </summary>
		public static bool DeleteInProgress { get; set; } = false;

		/// <summary>
		/// Delegate called by the scanners to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		private static bool CancelRequested() => ScanReporter?.IsCancelRequested() ?? false;

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IScanReporter ScanReporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back scan results
		/// </summary>
		public interface IScanReporter
		{
			void ScanFinished();

			bool IsCancelRequested();
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IDeleteReporter DeleteReporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back scan results
		/// </summary>
		public interface IDeleteReporter
		{
			void DeleteFinished();
		}

		/// <summary>
		/// The remote devices that have been discovered
		/// </summary>
		private static PlaybackDevices RemoteDevices { get; } = new PlaybackDevices();

		/// <summary>
		/// The single instance of the RemoteDeviceCallback class
		/// </summary>
		private static readonly RemoteDeviceCallback deviceCallback = new RemoteDeviceCallback();

		/// <summary>
		/// Implementation of the DeviceDiscovery.IDeviceDiscoveryChanges interface
		/// </summary>
		private class RemoteDeviceCallback : DeviceDiscovery.IDeviceDiscoveryChanges
		{
			/// <summary>
			/// Called to report the available devices - when registration is first made
			/// </summary>
			/// <param name="devices"></param>
			public void AvailableDevices( PlaybackDevices devices ) => 
				devices.DeviceCollection.ForEach( device => LibraryScanController.NewDeviceDetected( device ) );

			/// <summary>
			/// Called when one or more devices are no longer available
			/// </summary>
			/// <param name="devices"></param>
			public void UnavailableDevices( PlaybackDevices devices ) =>
					devices.DeviceCollection.ForEach( device => LibraryScanController.DeviceNotAvailable( device ) );

			/// <summary>
			/// Called when the wifi network state changes
			/// </summary>
			/// <param name="state"></param>
			public void NetworkState( bool state ) { }

			/// <summary>
			/// Called when a new DLNA device has been detected
			/// </summary>
			/// <param name="device"></param>
			public void NewDeviceDetected( PlaybackDevice device ) => LibraryScanController.NewDeviceDetected( device );
		}
	}
}
