namespace DBTest
{
	/// <summary>
	/// The LibraryNameDisplayController is used to obtain the name of the current library and to react to library changes
	/// </summary>
	class LibraryNameDisplayController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static LibraryNameDisplayController()
		{
			// When a library change message is received save it and report
			Mediator.RegisterPermanent( ( object _ ) => { StorageDataAvailable(); DataReporter?.DataAvailable(); } , typeof( SelectedLibraryChangedMessage ) );
		}

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable()
		{
			LibraryNameViewModel.LibraryName = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).Name;
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static DataReporter.IReporter DataReporter
		{
			get => dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}