using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// The LibraryClear class controls the clearance of a library
	/// </summary>
	class LibraryClear : LibraryManagementController.IReporter
	{
		/// <summary>
		/// LibrarySelection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public LibraryClear( Context alertContext )
		{
			contextForAlert = alertContext;
		}

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void SelectLibraryToClear()
		{
			LibraryManagementController.GetLibrariesAsync( this );
		}

		/// <summary>
		/// Allow the user to pick one of the available libraries to display
		/// </summary>
		/// <returns></returns>
		public void LibraryDataAvailable()
		{
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();
			Library libraryToClear = null;

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Select library to clear" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e ) {
						libraryToClear = LibraryManagementModel.Libraries[ e.Which ];
						( sender as AlertDialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
					} ) )
				.SetPositiveButton( "Ok", delegate { ClearSelectedLibrary( libraryToClear ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();

			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
		}

		/// <summary>
		/// Clear the selected library
		/// First of all confirm the selection with the user.
		/// Display a progress dialogue and perform the clearance asynchronously
		/// </summary>
		/// <param name="libraryToClear"></param>
		private void ClearSelectedLibrary( Library libraryToClear )
		{
			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( string.Format( "Are you sure you want to clear the {0} library", libraryToClear.Name ) )
				.SetPositiveButton( "Ok", delegate 
				{
					ClearLibrary( libraryToClear );
					clearingDialogue = new AlertDialog.Builder( contextForAlert )
											.SetTitle( string.Format( "Clearing library: {0}", libraryToClear.Name ) )
											.SetPositiveButton( "Ok", delegate {} )
											.SetCancelable( false )
											.Show();
					clearingDialogue.GetButton( ( int )DialogButtonType.Positive ).Visibility = ViewStates.Invisible;
				} )
				.SetNegativeButton( "Cancel", delegate {} )
				.Show();
		}

		/// <summary>
		/// Clear the selected library and then let the user know
		/// </summary>
		/// <param name="libraryToClear"></param>
		private async void ClearLibrary( Library libraryToClear )
		{
			await LibraryAccess.ClearLibraryAsync( libraryToClear );
			clearingDialogue.SetTitle( string.Format( "Library: {0} cleared", libraryToClear.Name ) );
			clearingDialogue.GetButton( ( int )DialogButtonType.Positive ).Visibility = ViewStates.Visible;
		}

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;

		/// <summary>
		/// Keep track of the in progress dialogue as it has to be accessed outside the method that created it
		/// </summary>
		private AlertDialog clearingDialogue = null;
	}
}