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
	internal class ScanProgressDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string libraryToScan, CancelRequested callback, BindDialog bindCallback )
		{
			// Save the parameters so that they are available after a configuration change
			libraryName = libraryToScan;
			reporter = callback;
			binder = bindCallback;

			new ScanProgressDialogFragment().Show( manager, "fragment_scan_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanProgressDialogFragment()
		{
			Cancelable = false;
		}

		/// <summary>
		/// Create the dialogue. The scan process is started when the dialogue is displayed. See OnResume
		/// Set the negative button to a null delegate so that the dialogue is not dismissed when the button is selected
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Context )
				.SetTitle( string.Format( "Scanning library: {0}", libraryName ) )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

		/// <summary>
		/// Start the scanning process
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();
			binder.Invoke( this );

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => { reporter.Invoke(); };
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
		/// Delegate type used to report back a cancel request
		/// </summary>
		public delegate void CancelRequested();

		/// <summary>
		/// The delegate to call when a cancel has been requested
		/// </summary>
		private static CancelRequested reporter = null;

		/// <summary>
		/// Delegate type used to report back the ScanProgressDialogFragment object
		/// </summary>
		public delegate void BindDialog( ScanProgressDialogFragment dialogue );

		/// <summary>
		/// The delegate used to report back the ScanProgressDialogFragment object
		/// </summary>
		private static BindDialog binder = null;

		/// <summary>
		/// The library being scanned
		/// </summary>
		private static string libraryName = "";
	}
}