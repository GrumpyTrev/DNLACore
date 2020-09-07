namespace DBTest
{
	/// <summary>
	/// The LibraryManagementController is the Controller for the LibraryManagement. It responds to LibraryManagement commands and maintains library data in the
	/// LibraryManagementModel
	/// </summary>
	static class LibraryManagementController
	{
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
				new SelectedLibraryChangedMessage() { SelectedLibrary = selectedLibrary.Id }.Send();
			}
		}
	}
}