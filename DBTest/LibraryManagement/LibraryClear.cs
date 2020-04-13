using Android.Support.V7.App;

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
		public LibraryClear( AppCompatActivity activity ) => contextForDialog = activity;

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void SelectLibraryToClear() => LibraryManagementController.GetLibrariesAsync( this );

		/// <summary>
		/// Allow the user to pick one of the available libraries to display
		/// </summary>
		/// <returns></returns>
		public void LibraryDataAvailable() => ClearLibraryDialogFragment.ShowFragment( contextForDialog.SupportFragmentManager );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForDialog = null;
	}
}