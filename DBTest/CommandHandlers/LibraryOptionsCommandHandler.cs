namespace DBTest
{
	/// <summary>
	/// The LibraryOptionsCommandHandler class is used to show library command options to the user
	/// </summary>
	class LibraryOptionsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the LibraryManagementController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => 
			LibraryOptionsDialogFragment.ShowFragment( CommandRouter.Manager, LibraryNameViewModel.LibraryName );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.toolbar_title;
	}
}