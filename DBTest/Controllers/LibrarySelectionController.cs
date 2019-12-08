namespace DBTest
{
	/// <summary>
	/// The LibrarySelectionController is the Controller for the LibrarySelection. It responds to LibrarySelection commands and maintains library data in the
	/// LibrarySelectionModel
	/// </summary>
	static class LibrarySelectionController
	{
		/// <summary>
		/// Get the Library data associated with the database
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		public static async void GetLibrariesAsync()
		{
			// Check if the Playlists details for the library have already been obtained
			if ( LibrarySelectionModel.Libraries == null )
			{
				LibrarySelectionModel.Libraries = await LibraryAccess.GetLibrariesAsync();
			}

			// Let the Views know that Libraries data is available
			Reporter?.LibraryDataAvailable();
		}

		/// <summary>
		/// Update the selected libary in the database and the ConnectionDetailsModel.
		/// Notify other controllers
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public static void SelectLibrary( Library selectedLibrary )
		{
			// Only process this if the library has changed
			if ( selectedLibrary.Id != ConnectionDetailsModel.LibraryId )
			{
				PlaybackAccess.SetSelectedLibrary( selectedLibrary );
				ConnectionDetailsModel.LibraryId = selectedLibrary.Id;
				new SelectedLibraryChangedMessage() { SelectedLibrary = selectedLibrary }.Send();
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void LibraryDataAvailable();
		}
	}
}