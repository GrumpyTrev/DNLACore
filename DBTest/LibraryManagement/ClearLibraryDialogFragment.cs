using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The ClearLibraryDialogFragment is used to allow the user to select a library to clear
	/// </summary>
	internal class ClearLibraryDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager )
		{
			new ClearLibraryDialogFragment().Show( manager, "fragment_library_clear" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ClearLibraryDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();

			return new AlertDialog.Builder( Activity )
				.SetTitle( "Select library to clear" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1, delegate 
				{
					// Enable the OK button
					( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
				} )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}

		/// <summary>
		/// Install a handler for the Ok button that gets the selected item from the internal ListView
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// If a library has not been selected yet then keep the OK button disabled
			Button okButton = alert.GetButton( ( int )DialogButtonType.Positive );
			if ( alert.ListView.CheckedItemPosition < 0 )
			{
				okButton.Enabled = false;
			}

			// Install the handler
			okButton.Click += ( sender, args ) =>
			{
				ClearSelectedLibrary( LibraryManagementModel.Libraries[ alert.ListView.CheckedItemPosition ] );
				alert.Dismiss();
			};
		}

		/// <summary>
		/// Clear the selected library
		/// First of all confirm the selection with the user.
		/// Display a progress dialogue and perform the clearance asynchronously
		/// </summary>
		/// <param name="libraryToClear"></param>
		private void ClearSelectedLibrary( Library libraryToClear )
		{
			ClearConfirmationDialogFragment.ShowFragment( Activity.SupportFragmentManager, libraryToClear );
		}
	}
}