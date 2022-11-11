namespace CoreMP
{
	/// <summary>
	/// The LibraryNameDisplayController is used to obtain the name of the current library and to react to library changes
	/// </summary>
	public class LibraryNameDisplayController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// when a library change message is received save it and report
		/// </summary>
		static LibraryNameDisplayController() => SelectedLibraryChangedMessage.Register( ( int _ ) => StorageDataAvailable() );

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => new DataReporter( StorageDataAvailable ).GetData();

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable() => LibraryNameViewModel.LibraryName = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).Name;
	}
}
