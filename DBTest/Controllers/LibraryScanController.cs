using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanController carries out the asynchronous actions involved in scanning a library
	/// </summary>
	public static class LibraryScanController
	{
		/// <summary>
		/// Asynchronous method called to carry out a library scan
		/// </summary>
		/// <param name="libraryToScan"></param>
		public static async void ScanLibraryAsynch( Library libraryToScan )
		{
			// Ignore this if there is already a scan in progress - report and error if tehre is a scan in progress but for a different library
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

					LibraryScanModel.UnmatchedSongs = new List<Song>();

					await Task.Run( async () =>
					{
						// Iterate all the sources associated with this library. Get the songs as well as we're going to need them below
						List<Source> sources = await Sources.GetSourcesAndSongsForLibraryAsync( LibraryScanModel.LibraryBeingScanned.Id );

						foreach ( Source source in sources )
						{
							// Add the songs from this source to a dictionary
							Dictionary<string, Song> pathLookup = new Dictionary<string, Song>( source.Songs.ToDictionary( song => song.Path ) );

							// Reset the scan action for all Songs
							source.Songs.ForEach( song => song.ScanAction = Song.ScanActionType.NotMatched );

							// Use a SongStorage instance to check for song changes
							SongStorage scanStorage = new SongStorage( LibraryScanModel.LibraryBeingScanned.Id, source, pathLookup );

							// Check the source scanning method
							if ( source.ScanType == "FTP" )
							{
								// Scan using the generic FTPScanner but with our callbacks
								await new FTPScanner( scanStorage ) { CancelRequested = CancelRequested }.Scan( source.ScanSource );
							}
							else if ( source.ScanType == "Local" )
							{
								// Scan using the generic InternalScanner but with our callbacks
								await new InternalScanner( scanStorage ) { CancelRequested = CancelRequested }.Scan( source.ScanSource );
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
		public static void ResetController()
		{
			LibraryScanModel.ClearModel();
		}

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

			await Task.Run( async () =>
			{
				// Delete all the Songs.
				DbAccess.DeleteAsync( LibraryScanModel.UnmatchedSongs );

				// Delete all the PlaylistItems associated with the songs. No need to wait for this
				Playlists.DeletePlaylistItems( LibraryScanModel.UnmatchedSongs.Select( song => song.Id ).ToList() );

				// Form a distinct list of all the ArtistAlbum items referenced by the deleted songs
				IEnumerable<int> artistAlbumIds = LibraryScanModel.UnmatchedSongs.Select( song => song.ArtistAlbumId ).Distinct();

				// Check if any of these ArtistAlbum items are now empty and need deleting
				foreach ( int id in artistAlbumIds )
				{
					Tuple<int, Artist> deletionResults = await CheckForAlbumDeletionAsync( id );

					if ( deletionResults.Item1 != -1 )
					{
						deletedAlbumIds.Add( deletionResults.Item1 );
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
		public static async Task<Artist> DeleteSongAsync( Song songToDelete )
		{
			// Delete the song.
			DbAccess.DeleteAsync( songToDelete );

			// Delete all the PlaylistItems associated with the song. No need to wait for this
			Playlists.DeletePlaylistItems( new List<int> { songToDelete.Id } );

			// Check if this ArtistAlbum item is now empty and need deleting
			Tuple<int, Artist> deletionResults = await CheckForAlbumDeletionAsync( songToDelete.ArtistAlbumId );

			if ( deletionResults.Item1 != -1 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = new List<int> { deletionResults.Item1 } }.Send();
			}

			return deletionResults.Item2;
		}

		/// <summary>
		/// Check if the specified ArtistAlbum should be deleted and any associated Album or Artist
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		private static async Task<Tuple<int, Artist >> CheckForAlbumDeletionAsync( int artistAlbumId )
		{
			// Keep track in the Tuple of any albums or artists deleted
			int deletedAlbumId = -1;
			Artist deletedArtist = null;

			// Refresh the contents of the ArtistAlbum
			ArtistAlbum artistAlbum = ArtistAlbums.GetArtistAlbumById( artistAlbumId );
			artistAlbum.Songs = await DbAccess.GetArtistAlbumSongsAsync( artistAlbum.Id );

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

			return new Tuple<int, Artist>( deletedAlbumId, deletedArtist );
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
	}
}