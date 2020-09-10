using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The EditLibraryDialogFragment is used to allow the user to select a library to edit
	/// </summary>
	internal class EditLibraryDialogFragment : SelectionBaseDialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager )
		{
			new EditLibraryDialogFragment().Show( manager, "fragment_library_edit" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public EditLibraryDialogFragment()
		{
		}

		/// <summary>
		/// The title for this dialogue
		/// </summary>
		protected override string Title { get => "Select library to edit"; }

		/// <summary>
		/// Carry out the action once a library has been selected
		/// </summary>
		/// <param name="libraryToClear"></param>
		protected override void LibrarySelected( Library library )
		{
			SourceSelectionDialogFragment.ShowFragment( Activity.SupportFragmentManager, library );
		}
	}
}