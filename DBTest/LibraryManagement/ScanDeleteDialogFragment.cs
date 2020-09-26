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
	internal class ScanDeleteDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue prompting for unmatched songs to be deleted
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, DeleteConfirmed callback, BindDialog bindCallback )
		{
			reporter = callback;
			binder = bindCallback;

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
		/// Set a null delegate for the Positive button so that the dialogue is not dismissed when it is pressed
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) => new AlertDialog.Builder( Context )
				.SetTitle( string.Format( "One or more songs have been deleted. Do you want to update the library: {0}", LibraryScanModel.LibraryBeingScanned.Name ) )
				.SetPositiveButton( "Yes", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "No", delegate { } )
				.Create();

		/// <summary>
		/// If the delete process has already started then disable the buttons and make sure the dialogue cannot be cancelled.
		/// Otherwise install the OK delegate to perform the deletion
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();
			binder.Invoke( this );

			// Install a handler for the Ok button that performs the song deletion
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
			{
				reporter.Invoke();
			};
		}

		/// <summary>
		/// Unbind this dialogue so that it can be garbage collected if required
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			binder.Invoke( null );
		}

		/// <summary>
		/// Disable both buttons and prevent the dialogue being cancelled
		/// </summary>
		public void UpdateState( bool deleteInProgress )
		{
			if ( deleteInProgress == true )
			{
				AlertDialog alert = ( AlertDialog )Dialog;

				alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
				alert.GetButton( ( int )DialogButtonType.Negative ).Enabled = false;
				alert.SetCancelable( false );
			}
		}

		/// <summary>
		/// Delegate type used to report back the user's decision
		/// </summary>
		public delegate void DeleteConfirmed();

		/// <summary>
		/// The delegate to call when the deletion has been confirmed or not
		/// </summary>
		private static DeleteConfirmed reporter = null;

		/// <summary>
		/// Delegate type used to report back the ScanDeleteDialogFragment object
		/// </summary>
		public delegate void BindDialog( ScanDeleteDialogFragment dialogue );

		/// <summary>
		/// The delegate used to report back the ScanDeleteDialogFragment object
		/// </summary>
		private static BindDialog binder = null;
	}
}