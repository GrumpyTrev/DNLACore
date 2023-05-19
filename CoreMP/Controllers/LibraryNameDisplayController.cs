namespace CoreMP
{
	/// <summary>
	/// The LibraryNameDisplayController is used to obtain the name of the current library and to react to library changes
	/// </summary>
	internal class LibraryNameDisplayController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		public LibraryNameDisplayController() =>
			NotificationHandler.Register( typeof( StorageController ), () =>
			{
				StorageDataAvailable();
				NotificationHandler.Register( typeof( ConnectionDetailsModel ), StorageDataAvailable );
			} );

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private void StorageDataAvailable() => LibraryNameViewModel.LibraryName = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).Name;
	}
}
