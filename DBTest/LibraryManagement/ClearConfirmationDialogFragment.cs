using Android.App;
using Android.OS;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Used to allow the user to confirm the clearance of a library
	/// </summary>
	internal class ClearConfirmationDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string clearLibrary, ClearConfirmed callback )
		{
			// Save the parameters so that they are available after a configuration change
			libraryToClear = clearLibrary;
			reporter = callback;

			new ClearConfirmationDialogFragment().Show( manager, "fragment_clear_confirmation" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ClearConfirmationDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) => new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Are you sure you want to clear the {0} library", libraryToClear ) )
				.SetPositiveButton( "Ok", delegate { reporter.Invoke(); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

		/// <summary>
		/// The library to clear
		/// </summary>
		private static string libraryToClear = "";

		/// <summary>
		/// The delegate used to report back the clear confirmation
		/// </summary>
		private static ClearConfirmed reporter = null;

		/// <summary>
		/// Delegate type used to report back the clear confirmation
		/// </summary>
		public delegate void ClearConfirmed();
	}
}