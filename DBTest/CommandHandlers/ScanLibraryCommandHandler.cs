namespace DBTest
{
	/// <summary>
	/// The ScanLibraryCommandHandler class is used to process a request to scan a library
	/// </summary>
	internal class ScanLibraryCommandHandler : CommandHandler, LibraryScanController.IScanReporter, LibraryScanController.IDeleteReporter
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue.
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) =>
			LibrarySelectionDialogFragment.ShowFragment( CommandRouter.Manager, "Select library to scan", -1, LibrarySelected );

		/// <summary>
		/// Delegate called when the scan process has finished
		/// </summary>
		public void ScanFinished()
		{
			// Check if any of the songs in the library have not been matched or have changed (only process if the scan was not cancelled)
			if ( ( cancelHasBeenRequested == false ) && ( LibraryScanModel.UnmatchedSongs.Count > 0 ) )
			{
				// Delete all of the unmatched songs. Don't wait for this to finish, the DeleteFinished method will be called when it has finished. 
				commandState = CommandStateType.Deleting;
				LibraryScanController.DeleteReporter = this;

				LibraryScanController.DeleteSongsAsync();
			}
			// If the ScanProgressDialogFragment is available then we can proceed with the next stage of the process
			else if ( scanProgressDialog != null )
			{
				scanProgressDialog.Dismiss();
				NotifyScanFinished();
			}
			else
			{
				commandState = CommandStateType.ScanComplete;
			}
		}

		/// <summary>
		/// Delegate called when the unmatched song deletion process has finished
		/// </summary>
		public void DeleteFinished()
		{
			// If the UI is available then get rid of the Scan In Progress dialoue 
			if ( scanProgressDialog != null )
			{
				scanProgressDialog.Dismiss();

				// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
				if ( ( LibraryScanModel.LibraryModified == true ) && ( libraryBeingScanned.Id == ConnectionDetailsModel.LibraryId ) )
				{
					new SelectedLibraryChangedMessage() { SelectedLibrary = libraryBeingScanned.Id }.Send();
				}

				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, $"Scanning of library: {LibraryScanModel.LibraryBeingScanned.Name} finished" );
				commandState = CommandStateType.Idle;
			}
			else
			{
				commandState = CommandStateType.DeleteComplete;
			} 
		}

		/// <summary>
		/// Called by the LibraryScanController to check if a cancel has been requested
		/// </summary>
		/// <returns></returns>
		public bool IsCancelRequested() => cancelHasBeenRequested;

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.scan_library;

		/// <summary>
		/// Delegate called by the scanner to check if the process has been cancelled
		/// </summary>
		/// <returns></returns>
		private void CancelRequested() => cancelHasBeenRequested = true;

		/// <summary>
		/// Called when a library has been selected by the LibrarySelectionDialogFragment
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void LibrarySelected( Library selectedLibrary )
		{
			// Save the library being scanned
			libraryBeingScanned = selectedLibrary;

			// Start the library scan process and let the user know what is happening
			cancelHasBeenRequested = false;
			commandState = CommandStateType.Scanning;

			LibraryScanController.ResetController();
			LibraryScanController.ScanReporter = this;
			LibraryScanController.ScanLibraryAsynch( libraryBeingScanned );

			ScanProgressDialogFragment.ShowFragment( CommandRouter.Manager, libraryBeingScanned.Name, CancelRequested, BindDialog );
		}

		/// <summary>
		/// Called when the ScanProgressDialogFragment dialog is displayed (OnResume)
		/// Save the reference to allow the dialogue to be dismissed
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( ScanProgressDialogFragment dialogue )
		{
			scanProgressDialog = dialogue;

			// If the post-scan processing was paused due to the UI not being available, then continue with it now
			if ( ( scanProgressDialog != null ) && ( ( commandState == CommandStateType.ScanComplete ) || ( commandState == CommandStateType.DeleteComplete ) ) )
			{
				scanProgressDialog.Dismiss();

				if ( commandState == CommandStateType.ScanComplete )
				{
					NotifyScanFinished();
				}
				else
				{
					DeleteFinished();
				}
			}
		}

		/// <summary>
		/// Let the user and other compnents know that the scan process has finished
		/// </summary>
		private void NotifyScanFinished()
		{
			// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
			if ( ( LibraryScanModel.LibraryModified == true ) && ( libraryBeingScanned.Id == ConnectionDetailsModel.LibraryId ) )
			{
				new SelectedLibraryChangedMessage() { SelectedLibrary = libraryBeingScanned.Id }.Send();
			}

			// Let the user know that the process has finished
			NotificationDialogFragment.ShowFragment( CommandRouter.Manager,
				$"Scanning of library: {libraryBeingScanned.Name} {( ( cancelHasBeenRequested == true ) ? "cancelled" : "finished" )}" );

			commandState = CommandStateType.Idle;
		}

		/// <summary>
		/// The possible states of the command handler
		/// </summary>
		private enum CommandStateType
		{
			Idle,
			// The library scan process is in progress and has not completed yet
			Scanning,
			// The library scan was completed but the next stage could not proceed due to the UI not being available
			ScanComplete,
			// The deletion of unmatched songs is in progress and has not completed yet
			Deleting,
			// The delete process has finished but could not be notified because the UI was not available
			DeleteComplete
		};

		/// <summary>
		/// The current state of the command handler
		/// </summary>
		private CommandStateType commandState = CommandStateType.Idle;

		/// <summary>
		/// ScanProgressDialogFragment reference held so that it can be dismissed
		/// </summary>
		private ScanProgressDialogFragment scanProgressDialog = null;

		/// <summary>
		/// Has a cancel been requested
		/// </summary>
		private bool cancelHasBeenRequested = false;

		/// <summary>
		/// The library selected for scanning
		/// </summary>
		private Library libraryBeingScanned = null;
	}
}
