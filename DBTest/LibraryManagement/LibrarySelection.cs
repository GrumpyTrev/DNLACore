using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;

namespace DBTest
{
	/// <summary>
	/// The LibrarySelection class controls the selection of a library to be displayed
	/// </summary>
	class LibrarySelection : LibrarySelectionController.IReporter
	{
		/// <summary>
		/// LibrarySelection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public LibrarySelection( Context alertContext )
		{
			contextForAlert = alertContext;
			LibrarySelectionController.Reporter = this;
		}

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void SelectLibrary()
		{
			// Get the list of available libraries
			LibrarySelectionController.GetLibrariesAsync();
		}

		/// <summary>
		/// Allow the user to pick one of the available libraries to display
		/// </summary>
		/// <returns></returns>
		public void LibraryDataAvailable()
		{
			// Get the names of the libraries to display and the index in thelist of the current library
			List<string> libraryNames = LibrarySelectionModel.Libraries.Select( lib => lib.Name ).ToList();
			int currentLibraryIndex = LibrarySelectionModel.Libraries.FindIndex( lib => ( lib.Id == ConnectionDetailsModel.LibraryId ) );
			Library libraryToSelect = LibrarySelectionModel.Libraries[ currentLibraryIndex ];

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Select library to display" )
				.SetSingleChoiceItems( libraryNames.ToArray(), currentLibraryIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e ) {
						libraryToSelect = LibrarySelectionModel.Libraries[ e.Which ];
					} ) )
				.SetPositiveButton( "Ok", delegate { LibrarySelectionController.SelectLibrary( libraryToSelect ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();
		}

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;
	}
}