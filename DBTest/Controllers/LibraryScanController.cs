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
						// The part of the RescanSongStorage that is being used here expects the library's chldren to be read, so do that here
						await LibraryAccess.GetLibraryChildrenAsync( LibraryScanModel.LibraryBeingScanned );

						// Iterate all the sources associated with this library. Get the songs as well as we're going to need them below
						List<Source> sources = await LibraryAccess.GetSourcesAsync( LibraryScanModel.LibraryBeingScanned.Id, true );

						foreach ( Source source in sources )
						{
							// Add the songs from this source to a dictionary
							Dictionary<string, Song> pathLookup = new Dictionary<string, Song>( source.Songs.ToDictionary( song => song.Path ) );

							// Reset the scan action for all Songs
							source.Songs.ForEach( song => song.ScanAction = Song.ScanActionType.NotMatched );

							// Use a RescanSongStorage instance to check for song changes
							RescanSongStorage scanStorage = new RescanSongStorage( LibraryScanModel.LibraryBeingScanned, source, pathLookup );

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
		public static async Task DeleteSongsAsync()
		{
			// Prevent this from being executed twice
			DeleteInProgress = true;

			// Keep track of any albums that are deleted so that other controllers can be notified
			List<int> deletedAlbumIds = new List<int>();

			await Task.Run( async () =>
			{
				// Delete all the Songs
				await ArtistAccess.DeleteSongsAsync( LibraryScanModel.UnmatchedSongs );

				// Delete all the PlaylistItems associated with the songs 
				await PlaylistAccess.DeletePlaylistItemsAsync( LibraryScanModel.UnmatchedSongs.Select( song => song.Id ).ToList() );

				// Form a distinct list of all the ArtistAlbum items referenced by the deleted songs
				IEnumerable<int> artistAlbumIds = LibraryScanModel.UnmatchedSongs.Select( song => song.ArtistAlbumId ).Distinct();

				// Check if any of these ArtistAlbum items are now empty and need deleting
				foreach ( int id in artistAlbumIds )
				{
					// Check if this ArtistAlbum is being referenced by any songs
					if ( ( await ArtistAccess.GetSongsReferencingArtistAlbumAsync( id ) ).Count == 0 )
					{
						// Delete the ArtistAlbum as it is no longer being referenced
						ArtistAlbum artistAlbum = await ArtistAccess.GetArtistAlbumAsync( id );
						await ArtistAccess.DeleteArtistAlbumAsync( artistAlbum );

						// Does any other ArtistAlbum reference the Album
						if ( ( await ArtistAccess.GetArtistAlbumsReferencingAlbumAsync( artistAlbum.AlbumId ) ).Count == 0 )
						{
							// Not referenced by any ArtistAlbum. so delete it
							await AlbumAccess.DeleteAlbumAsync( artistAlbum.AlbumId );
							deletedAlbumIds.Add( artistAlbum.AlbumId );

							// Does the associated Artist have any other Albums
							if ( ( await ArtistAccess.GetArtistAlbumsReferencingArtistAsync( artistAlbum.ArtistId ) ).Count == 0 )
							{
								// Delete the Artist
								await ArtistAccess.DeleteArtistAsync( artistAlbum.ArtistId );
							}
						}
					}
				}

			} );

			if ( deletedAlbumIds.Count > 0 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = deletedAlbumIds }.Send();
			}

			if ( LibraryScanModel.LibraryBeingScanned.Id == ConnectionDetailsModel.LibraryId )
			{
				new SelectedLibraryChangedMessage() { SelectedLibrary = LibraryScanModel.LibraryBeingScanned }.Send();
			}

			DeleteInProgress = false;
			DeleteReporter?.DeleteFinished();
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
		private static bool CancelRequested() => ScanReporter?.CancelRequested() ?? false;

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

			bool CancelRequested();
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