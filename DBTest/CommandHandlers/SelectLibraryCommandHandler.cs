namespace DBTest
{
	/// <summary>
	/// The SelectLibraryCommandHandler class is used to process a request to select a new library
	/// </summary>
	class SelectLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the LibraryManagementController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => 
			LibrarySelectionDialogFragment.ShowFragment( CommandRouter.Manager, "Select library to display", Libraries.Index( ConnectionDetailsModel.LibraryId ),
				LibrarySelected );

		/// <summary>
		/// Called when a library has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public void LibrarySelected( Library selectedLibrary ) => LibraryManagementController.SelectLibraryAsync( selectedLibrary );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.select_library;
	}
}