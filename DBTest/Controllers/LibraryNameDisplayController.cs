namespace DBTest
{
	/// <summary>
	/// The LibraryNameDisplayController is used to obtain the name of the current library and to react to library changes
	/// </summary>
	class LibraryNameDisplayController : BaseController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static LibraryNameDisplayController()
		{
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );

			instance = new LibraryNameDisplayController();
		}

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		protected override void StorageDataAvailable( object _ = null )
		{
			// Save the current library name
			LibraryNameViewModel.LibraryName = Libraries.GetLibraryById( ConnectionDetailsModel.LibraryId ).Name;

			// Call the base class
			base.StorageDataAvailable();
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Use the StorageDataAvailable method to store the new librray name and report it
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object _ ) => instance.StorageDataAvailable();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter DataReporter
		{
			set => instance.Reporter = value;
		}

		/// <summary>
		/// The one and only LibraryNameDisplayController instance
		/// </summary>
		private static readonly LibraryNameDisplayController instance = null;
	}
}