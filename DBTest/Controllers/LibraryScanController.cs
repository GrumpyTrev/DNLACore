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
					Reporter?.ScanFinished();
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
						// Create a LibraryCreator instance to do the processing of any new songs found during the rescan
						// The part of the LibraryCreator that is being used here expects its chldren to be read, so do that here
						await LibraryAccess.GetLibraryChildrenAsync( LibraryScanModel.LibraryBeingScanned );

						// Iterate all the sources associated with this library. Get the songs as well as we're going to need them below
						List<Source> sources = await LibraryAccess.GetSourcesAsync( LibraryScanModel.LibraryBeingScanned.Id, true );

						foreach ( Source source in sources )
						{
							// Add the songs from this source to a dictionary
							Dictionary<string, Song> pathLookup = new Dictionary<string, Song>();
							foreach ( Song songToAdd in source.Songs )
							{
								pathLookup.Add( songToAdd.Path, songToAdd );
								songToAdd.ScanAction = Song.ScanActionType.NotMatched;
							}

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
					Reporter?.ScanFinished();
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
		/// Flag indicating whether or not the this controller is busy scanning a library
		/// </summary>
		private static bool scanInProgress = false;

		/// <summary>
		/// Delegate called by the scanners to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		private static bool CancelRequested() => Reporter?.CancelRequested() ?? false;

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void ScanFinished();

			bool CancelRequested();
		}
	}
}