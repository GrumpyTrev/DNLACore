using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using SQLiteNetExtensionsAsync.Extensions;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanner class controls the scanning or rescanning of a library
	/// </summary>
	class LibraryScanner : LibraryManagementController.IReporter
	{
		/// <summary>
		/// LibraryScanner constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public LibraryScanner( Context alertContext )
		{
			contextForAlert = alertContext;
		}

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void ScanSelection()
		{
			LibraryManagementController.GetLibrariesAsync( this );
		}

		/// <summary>
		/// This is called when the library data is available.
		/// Use the data to populate the list held by the dialogue and display the dialogue
		/// </summary>
		public void LibraryDataAvailable()
		{
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();
			Library libraryToScan = null;

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Select library to scan" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						libraryToScan = LibraryManagementModel.Libraries[ e.Which ];
						( sender as AlertDialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
					} ) )
				.SetPositiveButton( "Ok", delegate { ScanSelectedLibrary( libraryToScan ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();

			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		public void ReleaseResources()
		{
		}

		/// <summary>
		/// Scan the selected library
		/// Display a cancellable progress dialogue and start the scan process going
		/// </summary>
		/// <param name="libraryToScan"></param>
		private void ScanSelectedLibrary( Library libraryToScan )
		{
			// Reset any previous cancel request
			cancelScanRequested = false;

			// Start scanning
			ScanWorkAsync( libraryToScan  );

			scanningDialogue = new AlertDialog.Builder( contextForAlert )
				.SetTitle( string.Format( "Scanning library: {0}", libraryToScan.Name ) )
				.SetCancelable( false )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

			scanningDialogue.Show();

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			scanningDialogue.GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => { cancelScanRequested = true; };
		}

		/// <summary>
		/// Carry out the scanning operations in an async method
		/// </summary>
		/// <param name="libraryToScan"></param>
		private async void ScanWorkAsync( Library libraryToScan )
		{
			List<Song> unmatchedSongs = new List<Song>();
			bool libraryModified = false;

			await Task.Run( async () =>  
			{
				// Create a LibraryCreator instance to do the processing of any new songs found during the rescan
				// The part of the LibraryCreator that is being used here expects its chldren to be read, so do that here
				await LibraryAccess.GetLibraryChildrenAsync( libraryToScan );

				// Iterate all the sources associated with this library. Get the songs as well as we're going to need them below
				List< Source > sources = await LibraryAccess.GetSourcesAsync( libraryToScan.Id, true );

				foreach ( Source source in sources )
				{
					// Add the songs from this source to a dictionary
					Dictionary<string, Song> pathLookup = new Dictionary<string, Song>();
					foreach ( Song songToAdd in source.Songs )
					{
						pathLookup.Add( songToAdd.Path, songToAdd );
						songToAdd.ScanAction = Song.ScanActionType.NotMatched;
					}

					RescanSongStorage scanStorage = new RescanSongStorage( libraryToScan, source, pathLookup );

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
					unmatchedSongs.AddRange( pathLookup.Values.Where( song => song.ScanAction == Song.ScanActionType.NotMatched ) );

					// Keep track of any library changes
					libraryModified |= scanStorage.LibraryModified;
				}
			} );

			// Dismiss the rescanning (progress) dialogue
			scanningDialogue.Dismiss();

			// Check if any of the songs in the library have not been matched or have changed (only process if the scan was not cancelled
			if ( ( cancelScanRequested == false ) && ( unmatchedSongs.Count > 0 ) )
			{
				new AlertDialog.Builder( contextForAlert )
					.SetTitle( string.Format( "One or more songs have been deleted. Do you want to update the library: {0}", libraryToScan.Name ) )
					.SetPositiveButton( "Yes", async delegate 
					{
						await DeleteSongsAsync( unmatchedSongs );
						if ( libraryToScan.Id == ConnectionDetailsModel.LibraryId )
						{
							new SelectedLibraryChangedMessage() { SelectedLibrary = libraryToScan }.Send();
						}

						new AlertDialog.Builder( contextForAlert )
							.SetTitle( string.Format( "Scanning of library: {0} finished", libraryToScan.Name ) )
							.SetPositiveButton( "Ok", delegate { } )
							.Show();
					} )
					.SetNegativeButton( "No", delegate { } )
					.Show();
			}
			else
			{
				// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
				if ( ( libraryModified == true ) && ( libraryToScan.Id == ConnectionDetailsModel.LibraryId ) )
				{
					new SelectedLibraryChangedMessage() { SelectedLibrary = libraryToScan }.Send();
				}

				new AlertDialog.Builder( contextForAlert )
					.SetTitle( string.Format( "Scanning of library: {0} {1}", libraryToScan.Name, ( cancelScanRequested == true ) ? "cancelled" : "finished" ) )
					.SetPositiveButton( "Ok", delegate { } )
					.Show();
			}
		}

		/// <summary>
		/// Delete the list of songs from the library
		/// </summary>
		/// <param name="songsToDelete"></param>
		private async Task DeleteSongsAsync( List<Song> songsToDelete )
		{
			// Keep track of any albums that are deleted so that other controllers can be notified
			List<int> deletedAlbumIds = new List<int>();

			// Delete all the Songs
			await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( songsToDelete );

			// Delete all the PlaylistItems associated with the songs 
			// THIS IS PROBABLY ALREADY AVAILABLE IN PLAYLIST ACCESS
			IEnumerable<int> songIds = songsToDelete.Select( song => song.Id );
			await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( 
				await ConnectionDetailsModel.AsynchConnection.Table<PlaylistItem>().Where( item => songIds.Contains( item.SongId ) ).ToListAsync() );

			// Form a distinct list of all the ArtistAlbum items referenced by the deleted songs
			IEnumerable<int> artistAlbumIds = songsToDelete.Select( song => song.ArtistAlbumId ).Distinct();

			// Check if any of these ArtistAlbum items are now empty and need deleting
			foreach ( int id in artistAlbumIds )
			{
				if ( await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == id ) ).CountAsync() == 0 )
				{
					// Delete the ArtistAlbum
					ArtistAlbum artistAlbum = await ConnectionDetailsModel.AsynchConnection.GetAsync<ArtistAlbum>( id );
					await ConnectionDetailsModel.AsynchConnection.DeleteAsync( artistAlbum );

					// Does any other ArtistAlbum reference the Album
					if ( await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>()
						.Where( artAlbum => ( artAlbum.AlbumId == artistAlbum.AlbumId ) ).CountAsync() == 0 )
					{
						// Not referenced by any ArtistAlbum. so delete it
						await ConnectionDetailsModel.AsynchConnection.DeleteAllIdsAsync<Album>( ( new List<object>() { artistAlbum.AlbumId } ) );
						deletedAlbumIds.Add( artistAlbum.AlbumId );

						// Does the associated Artist have any other Albums
						if ( await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>()
							.Where( artAlbum => ( artAlbum.ArtistId == artistAlbum.ArtistId ) ).CountAsync() == 0 )
						{
							// Delete the Artist
							await ConnectionDetailsModel.AsynchConnection.DeleteAllIdsAsync<Artist>( ( new List<object>() { artistAlbum.ArtistId } ) );
						}
					}
				}
			}

			if ( deletedAlbumIds.Count > 0 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = deletedAlbumIds }.Send();
			}
		}

		/// <summary>
		/// Delegate called by the scanners to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		private bool CancelRequested() => cancelScanRequested;

		/// <summary>
		/// Has a cancel been requested
		/// </summary>
		private bool cancelScanRequested = false;

		/// <summary>
		/// Keep track of the in progress dialogue as it has to be accessed outside the method that created it
		/// </summary>
		private AlertDialog scanningDialogue = null;

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;
	}
}