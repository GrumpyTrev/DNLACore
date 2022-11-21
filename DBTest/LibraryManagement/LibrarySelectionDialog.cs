using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using CoreMP;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The LibrarySelectionDialogFragment is used to select a library to perform an action on
	/// </summary>
	internal class LibrarySelectionDialog : DialogFragment
	{
		/// <summary>
		/// Show the dialogue with the specified title and initially selected library
		/// </summary>
		/// <param name="manager"></param>
		public static void Show( string title, int initialSelection, List<Library> libraries, Action< Library > selectionCallback )
		{
			// Save the parameters statically so that they survive a configuration change
			dialogueTitle = title;
			initialLibrary = initialSelection;
			availableLibraries = libraries;
			reporter = selectionCallback;

			new LibrarySelectionDialog().Show( CommandRouter.Manager, "fragment_library_selection" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public LibrarySelectionDialog()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Activity )
				.SetTitle( dialogueTitle )
				.SetSingleChoiceItems( availableLibraries.Select( lib => lib.Name ).ToArray(), initialLibrary, delegate
				{
					// Report back the selection
					reporter.Invoke( availableLibraries[ ( ( AlertDialog )Dialog ).ListView.CheckedItemPosition ] );
					Dialog.Dismiss();
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

		/// <summary>
		/// Dialogue title 
		/// </summary>
		private static string dialogueTitle = "";

		/// <summary>
		/// The index of the library to initially display selected
		/// </summary>
		private static int initialLibrary = -1;

		/// <summary>
		/// The libraries to choose from
		/// </summary>
		private static List<Library> availableLibraries = null;

		/// <summary>
		/// The delegate used to report back library selections
		/// </summary>
		private static Action<Library> reporter = null;
	}
}
