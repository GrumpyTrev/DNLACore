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
	/// The ScanDeleteDialogFragment is used to schedule the song deletion process
	/// </summary>
	internal class ScanDeleteDialogFragment : DialogFragment, LibraryScanController.IDeleteReporter
	{
		/// <summary>
		/// Show the dialogue prompting for unmatched songs to be deleted
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager )
		{
			new ScanDeleteDialogFragment().Show( manager, "fragment_scan_delete" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanDeleteDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			LibraryScanController.DeleteReporter = this;

			// Set a null delegate for the Positive button so that the dialogue is not dismissed when it is pressed
			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( string.Format( "One or more songs have been deleted. Do you want to update the library: {0}", LibraryScanModel.LibraryBeingScanned.Name ) )
				.SetPositiveButton( "Yes", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "No", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// If the delete process has already started then disable the buttons and make sure the dialogue cannot be cancelled.
		/// Otherwise install the OK delegate to perform the deletion
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			if ( LibraryScanController.DeleteInProgress == true )
			{
				DisableDialogue();
			}
			else
			{
				// Install a handler for the Ok button that performs the song deletion
				( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
				{
					DisableDialogue();
					LibraryScanController.DeleteSongsAsync();
				};
			}
		}

		/// <summary>
		/// When this dialogue is destroyed remove its link to the controller
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			LibraryScanController.DeleteReporter = null;
		}

		/// <summary>
		/// Delegate called when the scan process has finished
		/// </summary>
		public void DeleteFinished()
		{
			// No idea which thread this has come from so make sure any UI stuff, like dismissing the dialogue, is done on the UI thread.
			Activity.RunOnUiThread( () =>
			{
				// Dismiss the  dialogue
				Dialog.Dismiss();

				NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager,
					string.Format( "Scanning of library: {0} finished", LibraryScanModel.LibraryBeingScanned.Name ) );
			} );
		}

		/// <summary>
		/// Disable both buttons and prevent the dialogeu being cancelled
		/// </summary>
		private void DisableDialogue()
		{
			AlertDialog alert = ( AlertDialog )Dialog;

			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
			alert.GetButton( ( int )DialogButtonType.Negative ).Enabled = false;
			alert.SetCancelable( false );
		}
	}
}