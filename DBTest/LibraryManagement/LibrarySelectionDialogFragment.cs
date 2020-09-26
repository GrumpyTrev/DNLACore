using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The LibrarySelectionDialogFragment is used to select a library to perform an action on
	/// </summary>
	internal class LibrarySelectionDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue with the specified title and initially selected library
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string title, int initialSelection, LibrarySelected selectionCallback )
		{
			// Save the parameters statically so that they survive a connfiguration change
			dialogueTitle = title;
			initialLibrary = initialSelection;
			reporter = selectionCallback;

			new LibrarySelectionDialogFragment().Show( manager, "fragment_library_selection" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public LibrarySelectionDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			return new AlertDialog.Builder( Activity )
				.SetTitle( dialogueTitle )
				.SetSingleChoiceItems( Libraries.LibraryNames.ToArray(), initialLibrary, delegate 
				{
					// Enable the OK button once a selection has been made
					( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
				} )
				.SetPositiveButton( "Ok", delegate
				{
					// Report back the selection
					reporter.Invoke( Libraries.LibraryCollection[ ( ( AlertDialog )Dialog ).ListView.CheckedItemPosition ] );
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}

		/// <summary>
		/// Enable or disable the OK button at start up and after a rotation
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			// If a library has not been selected yet then keep the OK button disabled
			AlertDialog alert = ( AlertDialog )Dialog;
			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = ( alert.ListView.CheckedItemPosition >= 0 );
		}

		/// <summary>
		/// Dialogue title 
		/// </summary>
		private static string dialogueTitle = "";

		/// <summary>
		/// The index of the library to initially display selected
		/// </summary>
		private static int initialLibrary = -1;

		/// <summary>
		/// The delegate used to report back library selections
		/// </summary>
		private static LibrarySelected reporter = null;

		/// <summary>
		/// Delegate type used to report back the selected library
		/// </summary>
		public delegate void LibrarySelected( Library selectedLibrary );
	}
}