using Android.App;
using Android.Content;
using Android.OS;
using System;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The ScanProgressDialog is used to start the scanning process and to carry out any required actions when the scan has finished
	/// </summary>
	internal class ScanProgressDialog : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void Show( string libraryToScan, Action cancelAction, Action<Action> bindAction )
		{
			// Save the parameters so that they are available after a configuration change
			libraryName = libraryToScan;
			cancelCallback = cancelAction;
			bindCallback = bindAction;

			new ScanProgressDialog().Show( CommandRouter.Manager, "fragment_scan_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanProgressDialog() => Cancelable = false;

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

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Negative ).Click += ( _, _ ) => cancelCallback.Invoke();

			bindCallback.Invoke( Dismiss );
		}

		/// <summary>
		/// Unbind this dialogue so that it can be garbage collected if required
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			bindCallback.Invoke( null );
		}

		/// <summary>
		/// The delegate to call when a cancel has been requested
		/// </summary>
		private static Action cancelCallback = null;

		/// <summary>
		/// The delegate used to report back the ScanProgressDialog object
		/// </summary>
		private static Action<Action> bindCallback = null;

		/// <summary>
		/// The library being scanned
		/// </summary>
		private static string libraryName = "";
	}
}
