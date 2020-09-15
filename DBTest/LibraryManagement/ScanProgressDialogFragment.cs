using Android.App;
using Android.Content;
using Android.OS;
using System;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The ScanProgressDialogFragment is used to start the scanning process and to carry out any required actions when the scan has finished
	/// </summary>
	internal class ScanProgressDialogFragment : DialogFragment, LibraryScanController.IScanReporter
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library libraryToScan )
		{
			// Save the library statically to survive a rotation.
			LibraryBeingScanned = libraryToScan;

			// Reset the controller at the start of the scan process
			LibraryScanController.ResetController();

			// Reset the delete pending
			deletePending = false;

			new ScanProgressDialogFragment().Show( manager, "fragment_scan_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanProgressDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue. The scan process is started when the dialogue is displayed. See OnResume
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			// Set the negative button to a null delegate so that the dialogue is not dismissed when the button is selected
			new AlertDialog.Builder( Context )
				.SetTitle( string.Format( "Scanning library: {0}", LibraryBeingScanned.Name ) )
				.SetCancelable( false )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

		/// <summary>
		/// Start the scanning process
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			// If the delete process failed last time and is now pending then try it again
			if ( deletePending == true )
			{
				deletePending = false;
				Dialog.Dismiss();

				// Use the ScanDeleteDialogFragment to perform the deletion
				ScanDeleteDialogFragment.ShowFragment( Activity.SupportFragmentManager );
			}
			else
			{
				// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
				( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => { cancelScanRequested = true; };

				LibraryScanController.ScanReporter = this;
				LibraryScanController.ScanLibraryAsynch( LibraryBeingScanned );
			}
		}

		/// <summary>
		/// When this dialogue is destroyed remove its link to the controller
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			LibraryScanController.ScanReporter = null;
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
				// Check if any of the songs in the library have not been matched or have changed (only process if the scan was not cancelled)
				if ( ( cancelScanRequested == false ) && ( LibraryScanModel.UnmatchedSongs.Count > 0 ) )
				{
					// Use the ScanDeleteDialogFragment to perform the deletion
					if ( ScanDeleteDialogFragment.ShowFragment( Activity.SupportFragmentManager ) == true )
					{
						Dialog.Dismiss();
					}
					else
					{
						deletePending = true;
					}
				}
				else
				{
					Dialog.Dismiss();

					// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
					if ( ( LibraryScanModel.LibraryModified == true ) && ( LibraryBeingScanned.Id == ConnectionDetailsModel.LibraryId ) )
					{
						new SelectedLibraryChangedMessage() { SelectedLibrary = LibraryBeingScanned.Id }.Send();
					}

					NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager,
						string.Format( "Scanning of library: {0} {1}", LibraryBeingScanned.Name, ( cancelScanRequested == true ) ? "cancelled" : "finished" ) );
				}
			} );
		}

		/// <summary>
		/// The library being scanned
		/// </summary>
		private static Library LibraryBeingScanned { get; set; } = null;

		/// <summary>
		/// Has a cancel been requested
		/// </summary>
		private bool cancelScanRequested = false;

		/// <summary>
		/// The delete operation failed probably due to the app no longer being displayed
		/// </summary>
		private static bool deletePending = false;
	}
}