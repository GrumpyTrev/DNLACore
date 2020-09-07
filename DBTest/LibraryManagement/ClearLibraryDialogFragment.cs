﻿using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The ClearLibraryDialogFragment is used to allow the user to select a library to clear
	/// </summary>
	internal class ClearLibraryDialogFragment : SelectionBaseDialogFragment
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
		/// The title for this dialogue
		/// </summary>
		protected override string Title { get => "Select library to clear"; }

		/// <summary>
		/// Carry out the action once a library has been selected
		/// </summary>
		/// <param name="libraryToClear"></param>
		protected override void LibrarySelected( Library library )
		{
			ClearConfirmationDialogFragment.ShowFragment( Activity.SupportFragmentManager, library );
		}
	}
}