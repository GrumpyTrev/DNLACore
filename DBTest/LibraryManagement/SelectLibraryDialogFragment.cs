using System;

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
	internal class SelectLibraryDialogFragment : SelectionBaseDialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager ) => new SelectLibraryDialogFragment().Show( manager, "fragment_library_selection" );

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SelectLibraryDialogFragment()
		{
		}

		/// <summary>
		/// The title for this dialogue
		/// </summary>
		protected override string Title { get => "Select library to display"; }

		/// <summary>
		/// The index of the library to initially display selected
		/// </summary>
		protected override int InitallySelectedLibraryIndex { get => Libraries.Index( ConnectionDetailsModel.LibraryId ); }

		/// <summary>
		/// Carry out the action once a library has been selected
		/// </summary>
		/// <param name="libraryToClear"></param>
		protected override void LibrarySelected( Library library )
		{
			LibraryManagementController.SelectLibraryAsync( library );
		}
	}
}