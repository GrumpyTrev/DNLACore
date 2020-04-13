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
		public static void ShowFragment( FragmentManager manager, Library libraryToClear )
		{
			// Save the library to clear so that it is available after a configuration change
			LibraryToClear = libraryToClear;

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
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) => 
			new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Are you sure you want to clear the {0} library", LibraryToClear.Name ) )
				.SetPositiveButton( "Ok", delegate
				{
					ClearProgressDialogFragment.ShowFragment( Activity.SupportFragmentManager, LibraryToClear );
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
			   
		/// <summary>
		/// The library to clear
		/// </summary>
		private static Library LibraryToClear { get; set; } = null;
	}
}