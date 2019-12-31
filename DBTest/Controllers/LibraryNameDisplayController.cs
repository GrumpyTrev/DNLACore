namespace DBTest
{
	/// <summary>
	/// The LibraryNameDisplayController is used to obtain the name of the current library and to react to library changes
	/// </summary>
	static class LibraryNameDisplayController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static LibraryNameDisplayController() => Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );

		/// <summary>
		/// Get the name of the currently displayed library and report it back via the delegate
		/// </summary>
		public static async void GetCurrentLibraryNameAsync() => 
			Reporter?.LibraryNameAvailable( await LibraryAccess.GetLibraryNameAsync( ConnectionDetailsModel.LibraryId ) );

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Report the library name back to the delegate
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message ) => 
			Reporter?.LibraryNameAvailable( ( message as SelectedLibraryChangedMessage ).SelectedLibrary.Name );

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void LibraryNameAvailable( string libraryName );
		}
	}
}