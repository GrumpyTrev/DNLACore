using Android.Support.V7.App;

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
		public LibrarySelection( AppCompatActivity activity ) => contextForDialog = activity;

		/// <summary>
		/// Get the list of libraries
		/// </summary>
		public void SelectLibrary() => LibraryManagementController.GetLibrariesAsync( this );

		/// <summary>
		/// Allow the user to pick one of the available libraries to display
		/// </summary>
		/// <returns></returns>
		public void LibraryDataAvailable() => SelectLibraryDialogFragment.ShowFragment( contextForDialog.SupportFragmentManager );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForDialog = null;
	}
}