namespace DBTest
{
	/// <summary>
	/// The LibraryManagementController is the Controller for the LibraryManagement. It responds to LibraryManagement commands and maintains library data in the
	/// LibraryManagementModel
	/// </summary>
	static class LibraryManagementController
	{
		/// <summary>
		/// Get the Library data associated with the database
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		public static async void GetLibrariesAsync( IReporter reporter )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( LibraryManagementModel.Libraries == null )
			{
				LibraryManagementModel.Libraries = await LibraryAccess.GetLibrariesAsync();
			}

			// Let the Views know that Libraries data is available
			reporter.LibraryDataAvailable();
		}

		/// <summary>
		/// Update the selected libary in the database and the ConnectionDetailsModel.
		/// Notify other controllers
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public static async void SelectLibraryAsync( Library selectedLibrary )
		{
			// Only process this if the library has changed
			if ( selectedLibrary.Id != ConnectionDetailsModel.LibraryId )
			{
				await PlaybackAccess.SetSelectedLibraryAsync( selectedLibrary );
				ConnectionDetailsModel.LibraryId = selectedLibrary.Id;
				new SelectedLibraryChangedMessage() { SelectedLibrary = selectedLibrary }.Send();
			}
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void LibraryDataAvailable();
		}
	}
}