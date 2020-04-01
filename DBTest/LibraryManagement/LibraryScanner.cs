using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.App;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanner class controls the scanning of a library
	/// </summary>
	class LibraryScanner : LibraryManagementController.IReporter
	{
		/// <summary>
		/// LibraryScanner constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public LibraryScanner( AppCompatActivity activity )
		{
			contextForDialog = activity;
		}

		/// <summary>
		/// Get the list of libraries and present them to the user
		/// </summary>
		public void ScanSelection()
		{
			LibraryManagementController.GetLibrariesAsync( this );
		}

		/// <summary>
		/// This is called when the library data is available.
		/// Display the ScanLibrary dialogue
		/// </summary>
		public void LibraryDataAvailable()
		{
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();
			ScanLibraryDialogFragment.ShowFragment( contextForDialog.SupportFragmentManager, libraryNames );
		}

		/// <summary>
		/// Called to release any resources held by the fragment
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