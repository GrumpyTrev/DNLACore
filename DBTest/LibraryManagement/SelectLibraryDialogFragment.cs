using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Select library dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SelectLibraryDialogFragment : DialogFragment
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager )
		{
			new SelectLibraryDialogFragment().Show( manager, "fragment_library_selection" );
		}

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