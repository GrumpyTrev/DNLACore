using Android.Support.V7.App;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanner class controls the scanning of a library.
	/// It obtains the list of available libraries and starts the ScanLibraryDialogFragment to let the user choose a library
	/// </summary>
	class LibraryScanner : LibraryManagementController.IReporter
	{
		/// <summary>
		/// LibraryScanner constructor
		/// Save the supplied context for starting the fragment
		/// </summary>
		/// <param name="bindContext"></param>
		public LibraryScanner( AppCompatActivity activity ) => contextForDialog = activity;

		/// <summary>
		/// Get the list of libraries and present them to the user in the ScanLibraryDialogFragment
		/// </summary>
		public void ScanSelection() => LibraryManagementController.GetLibrariesAsync( this );

		/// <summary>
		/// This is called when the library data is available.
		/// Display the ScanLibrary dialogue
		/// </summary>
		public void LibraryDataAvailable() => ScanLibraryDialogFragment.ShowFragment( contextForDialog.SupportFragmentManager );

		/// <summary>
		/// Called to release any resources held by this class
		/// </summary>
		public void ReleaseResources()
		{
		}

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForDialog = null;
	}
}