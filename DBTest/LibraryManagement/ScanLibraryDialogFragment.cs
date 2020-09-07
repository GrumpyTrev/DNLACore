using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The ScanLibraryDialogFragment lets the user choose a library to scan and uses the ScanProgressDialogFragment to perform the scan
	/// </summary>
	internal class ScanLibraryDialogFragment : SelectionBaseDialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the available libraries
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager ) => new ScanLibraryDialogFragment().Show( manager, "fragment_scan_library" );

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanLibraryDialogFragment()
		{
		}

		/// <summary>
		/// The title for this dialogue
		/// </summary>
		protected override string Title { get => "Select library to scan"; }

		/// <summary>
		/// Carry out the action once a library has been selected
		/// </summary>
		/// <param name="libraryToClear"></param>
		protected override void LibrarySelected( Library library )
		{
			ScanProgressDialogFragment.ShowFragment( Activity.SupportFragmentManager, library );
		}
	}
}