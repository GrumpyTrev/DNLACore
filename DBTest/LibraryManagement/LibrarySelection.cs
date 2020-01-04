using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The LibrarySelection class controls the selection of a library to be displayed
	/// </summary>
	class LibrarySelection : LibraryManagementController.IReporter
	{
		/// <summary>
		/// LibrarySelection constructor
		/// Save the supplied context
		/// </summary>
		/// <param name="bindContext"></param>
		public LibrarySelection( AppCompatActivity alertContext ) => contextForAlert = alertContext;

		/// <summary>
		/// Get the list of libraries
		/// </summary>
		public void SelectLibrary() => LibraryManagementController.GetLibrariesAsync( this );

		/// <summary>
		/// Allow the user to pick one of the available libraries to display
		/// </summary>
		/// <returns></returns>
		public void LibraryDataAvailable() => 
			new SelectLibraryDialogFragment().Show( contextForAlert.SupportFragmentManager, "fragment_library_selection" );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForAlert = null;
	}

	/// <summary>
	/// Select library dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SelectLibraryDialogFragment: DialogFragment
	{
		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SelectLibraryDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Get the names of the libraries to display and the index in thelist of the current library
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();
			int currentLibraryIndex = LibraryManagementModel.Libraries.FindIndex( lib => ( lib.Id == ConnectionDetailsModel.LibraryId ) );
			Library libraryToSelect = LibraryManagementModel.Libraries[ currentLibraryIndex ];

			return new AlertDialog.Builder( Activity )
				.SetTitle( "Select library to display" )
				.SetSingleChoiceItems( libraryNames.ToArray(), currentLibraryIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e ) {
						libraryToSelect = LibraryManagementModel.Libraries[ e.Which ];
					} ) )
				.SetPositiveButton( "Ok", delegate { LibraryManagementController.SelectLibraryAsync( libraryToSelect ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}
	}
}