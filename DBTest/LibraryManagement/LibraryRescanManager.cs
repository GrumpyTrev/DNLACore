using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace DBTest
{
	/// <summary>
	/// The LibraryRescanManager class controls the rescanning of a library
	/// </summary>
	class LibraryRescanManager : LibraryScanningController.IReporter
	{
		/// <summary>
		/// LibraryRescanManager constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public LibraryRescanManager( Context alertContext )
		{
			contextForAlert = alertContext;
			LibraryScanningController.Reporter = this;
		}

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void RescanSelection()
		{
			// Get the list of available libraries
			LibraryScanningController.GetLibrariesAsync();
		}

		/// <summary>
		/// This is called when the library data is available.
		/// Use the data to populate the list held by the dialogue and display the dialogue
		/// </summary>
		public void LibraryDataAvailable()
		{
			List<string> libraryNames = LibraryScanningModel.Libraries.Select( lib => lib.Name ).ToList();
			Library libraryToScan = null;

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Select library to rescan" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						libraryToScan = LibraryScanningModel.Libraries[ e.Which ];
						( sender as AlertDialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
					} ) )
				.SetPositiveButton( "Ok", delegate { RescanSelectedLibrary( libraryToScan ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();

			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		public void ReleaseResources() => LibraryScanningController.Reporter = null;

		/// <summary>
		/// Rescan the selected library
		/// Display a cancellable progress dialogue and start the rescan process going
		/// </summary>
		/// <param name="libraryToScan"></param>
		private void RescanSelectedLibrary( Library libraryToScan )
		{
			// Reset any previous cancel request
			cancelRescanRequested = false;

			// Start scanning
			RescanWorkAsync( libraryToScan  );

			rescanningDialogue = new AlertDialog.Builder( contextForAlert )
				.SetTitle( string.Format( "Scanning library: {0}", libraryToScan.Name ) )
				.SetCancelable( false )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

			rescanningDialogue.Show();

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			rescanningDialogue.GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => { cancelRescanRequested = true; };
		}

		/// <summary>
		/// Carry out the rescanning opersations in an async method
		/// </summary>
		/// <param name="libraryToScan"></param>
		private async void RescanWorkAsync( Library libraryToScan )
		{
			await Task.Run( async () =>  
			{
				// Create a LibraryScanner instance to do the processing of any new songs found during the rescan
				// The part of the LibraryScanner that is being used here expects its chldren to be read, so do that here
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

					// Check the source scanning method
					if ( source.ScanType == "FTP" )
					{
						// Scan using the generic FTPScanner but with our callbacks
						await new FTPScanner( new RescanSongStorage( libraryToScan, source, pathLookup ) ) {
							CancelRequested = CancelRequested }.Scan( source.ScanSource );
					}
				}
			} );

			// Dismiss the rescanning (progress) dialogue and display a done dialogue
			rescanningDialogue.Dismiss();

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( string.Format( "Scanning of library: {0} {1}", libraryToScan.Name, ( cancelRescanRequested == true ) ? "cancelled" : "finished" ) )
				.SetPositiveButton( "Ok", delegate { } )
				.Show();
		}

		private void SetProgressText()
		{
		}

		/// <summary>
		/// Delegate called by the scanners to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		private bool CancelRequested() => cancelRescanRequested;

		/// <summary>
		/// Has a cancel been requested
		/// </summary>
		private bool cancelRescanRequested = false;

		/// <summary>
		/// Keep track of the in progress dialogue as it has to be accessed outside the method that created it
		/// </summary>
		private AlertDialog rescanningDialogue = null;

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;
	}
}