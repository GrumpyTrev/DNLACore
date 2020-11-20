using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The LibraryOptionsDialogFragment displays the current libaray and presents the user with further library options
	/// </summary>
	internal class LibraryOptionsDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string libaryName )
		{
			// Save the parameters so that they are available after a configuration change
			LibraryOptionsDialogFragment.currentLibraryName = libaryName;

			new LibraryOptionsDialogFragment().Show( manager, "fragment_library_options" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public LibraryOptionsDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Create the custom view and get references to the editable fields
			View dialogView = LayoutInflater.From( Context ).Inflate( Resource.Layout.library_options_dialogue_layout, null );

			// Create handlers for all the command buttons
			dialogView.FindViewById<Button>( Resource.Id.select_library ).Click += ( sender, args ) => { CommandRouter.HandleCommand( Resource.Id.select_library ); Dismiss(); };
			dialogView.FindViewById<Button>( Resource.Id.edit_library ).Click += ( sender, args ) => { CommandRouter.HandleCommand( Resource.Id.edit_library ); Dismiss(); };
			dialogView.FindViewById<Button>( Resource.Id.scan_library ).Click += ( sender, args ) => { CommandRouter.HandleCommand( Resource.Id.scan_library ); Dismiss(); };
			dialogView.FindViewById<Button>( Resource.Id.clear_library ).Click += ( sender, args ) => { CommandRouter.HandleCommand( Resource.Id.clear_library ); Dismiss(); };

			// Create the AlertDialog with no Save handler (and no dismiss on Save)
			return new AlertDialog.Builder( Activity )
			.SetTitle( $"Library:  {currentLibraryName}" )
			.SetView( dialogView )
			.SetNegativeButton( "Cancel", delegate { } )
			.Create();
		}

		/// <summary>
		/// The name of the currently selected library to edit
		/// </summary>
		private static string currentLibraryName = "";
	}
}