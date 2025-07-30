using System;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The EditLibraryCommandHandler class is used to process a request to edit a library
	/// The LibrarySelectionDialog class is used to select the library.
	/// Once a library has been selected the SourceSelectionDialog is used to display the sources associated with the library.
	/// When a source is selected the SourceEditDialog is used to to display the source details and allow it to be edited.
	/// A reference to the SourceSelectionDialog must be maintained to allow and edited source to be redisplayed
	/// </summary>
	internal class EditLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the SourceSelectionDialog
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int _ ) =>
			LibrarySelectionDialog.Show( "Select library to edit", -1, LibraryManagementViewModel.AvailableLibraries,
				selectionCallback: ( selectedLibrary ) =>

					// Show a dialog so that a source can be selected for editing or a new source can be requested 
					SourceSelectionDialog.Show( selectedLibrary,

						// A source has been selected display it using the SourceEditDialog
						sourceSelectedAction: ( selectedSource ) => SourceEditDialog.Show( selectedSource, SourceChanged, SourceDeleted ),

						// The user has requested that a new source be added to the selected library
						// Add a new source and tell the SourceSelectionDialog that it needs to redisplay its data
						newSourceAction: () => MainApp.CommandInterface.CreateSourceForLibrary( selectedLibrary ) ) );

		/// <summary>
		/// Called when an updated source has been submitted for saving
		/// Only update the existing source if it has been changed.
		/// </summary>
		/// <param name="originalSource"></param>
		/// <param name="newSource"></param>
		private void SourceChanged( Source originalSource, Source newSource, Action dismissDialogAction )
		{
			// If nothing has changed then tell the user, otherwise carry out the save operation
			if ( ( newSource.Name != originalSource.Name ) || ( newSource.FolderName != originalSource.FolderName ) ||
				( newSource.PortNo != originalSource.PortNo ) || ( newSource.IPAddress != originalSource.IPAddress ) ||
				( newSource.AccessMethod != originalSource.AccessMethod ) )
			{
				// Something has changed so update the source
				originalSource.UpdateSource( newSource );

				// Dismiss the dialogue
				dismissDialogAction.Invoke();
			}
			else
			{
				// Nothing has changed
				NotificationDialog.Show( "No changes made to source" );
			}
		}

		/// <summary>
		/// Called when a request to delete a Source has been made
		/// Confirm the deletion and 
		/// </summary>
		/// <param name="sourceToDelete"></param>
		private void SourceDeleted( Source sourceToDelete, Action dismissDialogAction ) =>
			ConfirmationDialog.Show( "Are you sure you want to delete this source", () =>
				{
					// Delete the source
					MainApp.CommandInterface.DeleteSource( sourceToDelete );

					// Dismiss the dialogue
					dismissDialogAction.Invoke();
				} );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.edit_library;
	}
}
