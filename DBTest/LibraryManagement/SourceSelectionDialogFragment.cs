using Android.App;
using Android.OS;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Used to allow the user to select a library source to edit
	/// </summary>
	internal class SourceSelectionDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library libraryToDisplay )
		{
			// Save the library whose soures are being displayed
			LibraryToDisplay = libraryToDisplay;

			new SourceSelectionDialogFragment().Show( manager, "fragment_source_selection" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SourceSelectionDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) => 
			new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Sources for {0} library", LibraryToDisplay.Name ) )
				.SetPositiveButton( "Done", delegate { } )
				.Create();
			   
		/// <summary>
		/// The library to display
		/// </summary>
		private static Library LibraryToDisplay { get; set; } = null;
	}
}