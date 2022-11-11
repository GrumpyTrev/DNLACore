using System;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The EditLibraryCommandHandler class is used to process a request to edit a library
	/// The LibrarySelectionDialogFragment class is used to select the library.
	/// Once a library has been selected the SourceSelectionDialogFragment is used to display the sources associated with the library.
	/// When a source is selected the SourceEditDialogFragment is used to to display the source details and allow it to be edited.
	/// A reference to the SourceSelectionDialogFragment must be maintained to allow and edited source to be redisplayed
	/// </summary>
	internal class EditLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the SourceSelectionDialogFragment
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int _ ) => 
			LibrarySelectionDialogFragment.Show( "Select library to edit", -1, LibraryManagementViewModel.AvailableLibraries, 
				selectionCallback: ( selectedLibrary ) =>

					// Show a dialog so that a source can be selected for editing or a new source can be requested 
					SourceSelectionDialogFragment.ShowFragment( CommandRouter.Manager, selectedLibrary,

						// A source has been selected display it using the SourceEditDialogFragment
						callback: ( selectedSource ) => SourceEditDialogFragment.ShowFragment( CommandRouter.Manager, selectedSource, SourceChanged, SourceDeleted ),

						// The user has requested that a new source be added to the selected library
						// Add a new source and tell the SourceSelectionDialogFragment that it needs to redisplay its data
						newSourceCallback: () =>
						{
							MainApp.CommandInterface.CreateSourceForLibrary( selectedLibrary );

							// Need to tell the SourceSelectionDialogFragment that it needs to redisplay its data
							sourceSelectionDialog?.OnSourceChanged();
						},

						// Called when the SourceSelectionDialogFragment dialog is displayed( OnResume )
						// Save the reference for updating the sources if tey are edited
						bindCallback: ( dialogue ) => sourceSelectionDialog = dialogue ) );

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

				// Need to tell the SourceSelectionDialogFragment that it needs to redisplay its data
				sourceSelectionDialog?.OnSourceChanged();

				// Dismiss the dialogue
				dismissDialogAction.Invoke();
			}
			else
			{
				// Nothing has changed
				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, "No changes made to source" );
			}
		}

		/// <summary>
		/// Called when a request to delete a Source has been made
		/// Confirm the deletion and 
		/// </summary>
		/// <param name="sourceToDelete"></param>
		private void SourceDeleted( Source sourceToDelete, Action dismissDialogAction ) => 
			ConfirmationDialogFragment.Show( "Are you sure you want to delete this source", () =>
				{
					// Delete the source
					MainApp.CommandInterface.DeleteSource( sourceToDelete );

					// Need to tell the SourceSelectionDialogFragment that it needs to redisplay its data
					sourceSelectionDialog?.OnSourceChanged();

					// Dismiss the dialogue
					dismissDialogAction.Invoke();
				} );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.edit_library;

		/// <summary>
		/// SourceSelectionDialogFragment reference held so that it can be infromed of source detail changes
		/// </summary>
		private SourceSelectionDialogFragment sourceSelectionDialog = null;
	}
}
