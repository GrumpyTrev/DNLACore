using System;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The ScanLibraryCommandHandler class is used to process a request to scan a library
	/// </summary>
	internal class ScanLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue.
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) =>
			LibrarySelectionDialog.Show( "Select library to scan", -1, LibraryManagementViewModel.AvailableLibraries,
				( librarySelected ) =>
				{
					// Save the library being scanned
					libraryBeingScanned = librarySelected;

					// Start the library scan process and let the user know what is happening
					cancelHasBeenRequested = false;
					commandState = CommandStateType.Scanning;

					MainApp.CommandInterface.ScanLibrary( libraryBeingScanned, 
						scanFinished: () =>
						{
							// If the ScanProgressDialog is being displayed then dismiss it and tell the user the process has finisished
							if ( dismissDialogueAction != null )
							{
								dismissDialogueAction.Invoke();
								NotifyScanFinished();
							}
							else
							{
								commandState = CommandStateType.ScanComplete;
							}
						}, 
						scanCancelledCheck: () => cancelHasBeenRequested );

					ScanProgressDialog.Show( libraryBeingScanned.Name, cancelAction: () => cancelHasBeenRequested = true, BindDialog );
				} );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.scan_library;

		/// <summary>
		/// Called when the ScanProgressDialog dialog is displayed (OnResume)
		/// Save the reference to allow the dialogue to be dismissed
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( Action dismissCallback )
		{
			dismissDialogueAction = dismissCallback;

			// If the post-scan processing was paused due to the UI not being available, then continue with it now
			if ( ( dismissDialogueAction != null ) && ( commandState == CommandStateType.ScanComplete ) )
			{
				dismissDialogueAction.Invoke();
				NotifyScanFinished();
			}
		}

		/// <summary>
		/// Let the user and other components know that the scan process has finished
		/// </summary>
		private void NotifyScanFinished()
		{
			// Let the user know that the process has finished
			NotificationDialog.Show( $"Scanning of library: {libraryBeingScanned.Name} {( ( cancelHasBeenRequested == true ) ? "cancelled" : "finished" )}" );

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
		};

		/// <summary>
		/// The current state of the command handler
		/// </summary>
		private CommandStateType commandState = CommandStateType.Idle;

		/// <summary>
		/// Dialogue dismiss action
		/// </summary>
		private Action dismissDialogueAction = null;

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
