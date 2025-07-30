using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The ClearLibraryCommandHandler class is used to process a request to clear a library
	/// The process involves displaying 3 dialogues.
	/// First the LibrarySelectionDialog to select the library to clear.
	/// Then the ConfirmationDialog to confirm the clearance
	/// Then the Libarary is cleared and the ClearProgressDialog is shown whilst the clearance is being carried out.
	/// When the library has been cleared the title of the dialogue is changed and the user is allowed to dismiss the dialogue.
	/// This is the only part of the process that needs to be aware of the fragment lifecycle.
	/// The handler needs access to the dialog fragment in order to inform it when the clearance has finished.
	/// The dialog needs to inform the handler when it is destroyed by the system and when it is displayed again.
	/// </summary>
	internal class ClearLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the ClearConfirmationDialogFragment
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int _ ) =>
			LibrarySelectionDialog.Show( "Select library to clear", -1, LibraryManagementViewModel.AvailableLibraries,
				selectionCallback: ( selectedLibrary ) =>

				// When a library has been selected, confirm the clearance
				ConfirmationDialog.Show( $"Are you sure you want to clear the {selectedLibrary.Name} library",
					positiveCallback: () =>
					{
						// When clearance is confirmed start the clearance operation and display the progress dialogue
						// The ClearProgressDialog instance though which the completion of the clearance is indicated
						ClearProgressDialog progressDialogFragment = null;

						// Let the user know what's going on
						bool clearFinished = false;
						ClearProgressDialog.Show( selectedLibrary.Name, clearance: true,
							callback: ( dialogue ) =>
							{
								// Save a reference to the dialogue and update it's status
								progressDialogFragment = dialogue;
								progressDialogFragment?.UpdateDialogueState( clearFinished );
							} );

						// Start the clear process, but don't wait for it to finish
						MainApp.CommandInterface.ClearLibraryAsync( selectedLibrary,
							() =>
							{
								clearFinished = true;
								progressDialogFragment.UpdateDialogueState( clearFinished );
							} );

					} ) );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.clear_library;
	}
}
