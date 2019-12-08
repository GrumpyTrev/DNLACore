namespace DBTest
{
	/// <summary>
	/// The LibraryScanningController is the ControllerLibraryScanningController for the LibraryScanning. It responds to LibraryScanning commands and maintains library data in the
	/// LibraryScanningModel
	/// /// </summary>
	static class LibraryScanningController
	{
		/// <summary>
		/// Get the Library data associated with the database
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		public static async void GetLibrariesAsync()
		{
			// Check if the Playlists details for the library have already been obtained
			if ( LibraryScanningModel.Libraries == null )
			{
				LibraryScanningModel.Libraries = await LibraryAccess.GetLibrariesAsync();
			}

			// Let the Views know that Libraries data is available
			Reporter?.LibraryDataAvailable();
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