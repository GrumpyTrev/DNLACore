using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using SQLiteNetExtensionsAsync.Extensions;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	internal class ScanProgressDialogFragment : DialogFragment, LibraryScanController.IReporter
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library libraryToScan )
		{
			// Save the library statically to survive a rotation.
			libraryBeingScanned = libraryToScan;

			new ScanProgressDialogFragment().Show( manager, "fragment_scan_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanProgressDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( string.Format( "Scanning library: {0}", libraryBeingScanned.Name ) )
				.SetCancelable( false )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

			return alert;
		}

		/// <summary>
		/// Disable the OK button when the dialogue is first displayed
		/// Start the scanning process
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			alert.GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => { cancelScanRequested = true; };

			LibraryScanController.Reporter = this;
			LibraryScanController.ScanLibraryAsynch( libraryBeingScanned );
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			LibraryScanController.Reporter = null;
		}

		/// <summary>
		/// Delegate called by the scanner to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		public bool CancelRequested() => cancelScanRequested;

		/// <summary>
		/// Delegate called when the scan process has finished
		/// </summary>
		public void ScanFinished()
		{
			// No idea which thread this has come from so make sure any UI stuff, like dismissing the dialogue, is done on the UI thread.
			Activity.RunOnUiThread( () =>
			{
				// Dismiss the rescanning (progress) dialogue
				Dialog.Dismiss();

				// Check if any of the songs in the library have not been matched or have changed (only process if the scan was not cancelled
				if ( ( cancelScanRequested == false ) && ( LibraryScanModel.UnmatchedSongs.Count > 0 ) )
				{
					new AlertDialog.Builder( Context )
						.SetTitle( string.Format( "One or more songs have been deleted. Do you want to update the library: {0}", libraryBeingScanned.Name ) )
						.SetPositiveButton( "Yes", async delegate
						{
							await DeleteSongsAsync( LibraryScanModel.UnmatchedSongs );
							if ( libraryBeingScanned.Id == ConnectionDetailsModel.LibraryId )
							{
								new SelectedLibraryChangedMessage() { SelectedLibrary = libraryBeingScanned }.Send();
							}

							NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager,
								string.Format( "Scanning of library: {0} finished", libraryBeingScanned.Name ) );
						} )
						.SetNegativeButton( "No", delegate { } )
						.Show();
				}
				else
				{
					// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
					if ( ( LibraryScanModel.LibraryModified == true ) && ( libraryBeingScanned.Id == ConnectionDetailsModel.LibraryId ) )
					{
						new SelectedLibraryChangedMessage() { SelectedLibrary = libraryBeingScanned }.Send();
					}

					NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager,
						string.Format( "Scanning of library: {0} {1}", libraryBeingScanned.Name, ( cancelScanRequested == true ) ? "cancelled" : "finished" ) );
				}
			} );

			// Reset the controller
			LibraryScanController.ResetController();
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
		/// The library being scanned
		/// </summary>
		private static Library libraryBeingScanned = null;

		/// <summary>
		/// Has a cancel been requested
		/// </summary>
		private bool cancelScanRequested = false;
	}
}