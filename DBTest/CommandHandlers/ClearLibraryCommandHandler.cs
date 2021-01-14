namespace DBTest
{
	/// <summary>
	/// The ClearLibraryCommandHandler class is used to process a request to clear a library
	/// The process involves displaying 3 dialogues.
	/// First the LibrarySelectionDialogFragment to select the library to clear.
	/// Then the ClearConfirmationDialogFragment to confirm the clearance
	/// Then the Libarary is cleared and the ClearProgressDialogFragment is shown whilst the clearance is being carried out.
	/// When the library has been cleared the title of the dialgue is changed and the user is allowed to dismiss the dialogue.
	/// This is the only part of the process that needs to be aware of the fragment lifecycle.
	/// The handler needs access to the dialog fragment in order to inform it when the clearance has finished.
	/// The dialog needs to inform the handler when it is destroyed by the system and when it is displayed again.
	/// </summary>
	class ClearLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the ClearConfirmationDialogFragment
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) =>
			LibrarySelectionDialogFragment.ShowFragment( CommandRouter.Manager, "Select library to clear", -1, LibrarySelected );

		/// <summary>
		/// Called when a library has been selected.
		/// Confirm the clearance
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public void LibrarySelected( Library selectedLibrary )
		{
			libraryToClear = selectedLibrary;
			ClearConfirmationDialogFragment.ShowFragment( CommandRouter.Manager, libraryToClear.Name, ClearConfirmed );
		}

		/// <summary>
		/// Called when the library clearance has been confirmed
		/// </summary>
		public void ClearConfirmed()
		{
			// Start the clear process, but don't wait for it to finish
			ClearLibraryAsync();

			// Let the user know what's going on
			ClearProgressDialogFragment.ShowFragment( CommandRouter.Manager, libraryToClear.Name, BindDialog );
		}

		/// <summary>
		/// Clear the selected library and then let the user know
		/// </summary>
		private async void ClearLibraryAsync()
		{
			clearFinished = false;
			await LibraryManagementController.ClearLibraryAsync( libraryToClear );
			clearFinished = true;

			progressDialogFragment?.UpdateDialogueState( clearFinished );
		}

		/// <summary>
		/// Called when the ClearProgressDialogFragment dialog is displayed (OnResume)
		/// Save it and update its status with the command state
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( ClearProgressDialogFragment dialogue )
		{
			progressDialogFragment = dialogue;
			progressDialogFragment?.UpdateDialogueState( clearFinished );
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.clear_library;

		/// <summary>
		/// The ClearProgressDialogFragment instance though which the completion of the clearance is indicated
		/// </summary>
		private ClearProgressDialogFragment progressDialogFragment = null;

		/// <summary>
		/// The library to clear
		/// </summary>
		private Library libraryToClear = null;

		/// <summary>
		/// Flag indicating whether the clearance has completed
		/// </summary>
		private bool clearFinished = false;
	}
}