namespace DBTest
{
	/// <summary>
	/// The EditLibraryCommandHandler class is used to process a request to edit a library
	/// The LibrarySelectionDialogFragment class is used to select the library.
	/// Once a library has been selected the SourceSelectionDialogFragment is used to display the sources associated with the library.
	/// When a source is selected the SourceEditDialogFragment is used to to display the source details and allow it to be edited.
	/// A reference to the SourceSelectionDialogFragment must be maintained to allow and edited source to be redisplayed
	/// </summary>
	class EditLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the SourceSelectionDialogFragment
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => 
			LibrarySelectionDialogFragment.ShowFragment( CommandRouter.Manager, "Select library to edit", -1, LibrarySelected );

		/// <summary>
		/// Called when a library has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void LibrarySelected( Library selectedLibrary ) => 
			SourceSelectionDialogFragment.ShowFragment( CommandRouter.Manager, selectedLibrary, SourceSelected, BindDialog );

		/// <summary>
		/// Called when a source has been selected to edit.
		/// Display the SoureEditDialogFragment
		/// </summary>
		/// <param name="selectedSource"></param>
		private void SourceSelected( Source selectedSource ) => SourceEditDialogFragment.ShowFragment( CommandRouter.Manager, selectedSource, SourceChanged );

		/// <summary>
		/// Called when the SourceSelectionDialogFragment dialog is displayed (OnResume)
		/// Save the reference for updating the sources if tey are edited
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( SourceSelectionDialogFragment dialogue ) => sourceSelectionDialog = dialogue;

		/// <summary>
		/// Called when an updated source has been submitted for saving
		/// Only update the existing source if it has been changed.
		/// </summary>
		/// <param name="originalSource"></param>
		/// <param name="newSource"></param>
		private async void SourceChanged( Source originalSource, Source newSource, SourceEditDialogFragment sourceEditDialog )
		{
			// If nothing has changed then tell the user, otherwise carry out the save operation
			if ( ( newSource.Name != originalSource.Name ) || ( newSource.FolderName != originalSource.FolderName ) ||
				( newSource.PortNo != originalSource.PortNo ) || ( newSource.IPAddress != originalSource.IPAddress ) ||
				( newSource.AccessType != originalSource.AccessType ) )
			{
				// Something has changed so update the source
				await Sources.UpdateSourceAsync( originalSource, newSource );

				// Need to tell the SourceSelectionDialogFragment that it needs to redisplay its data
				sourceSelectionDialog?.OnSourceChanged();

				// Dismiss the dialogue
				sourceEditDialog.Dismiss();
			}
			else
			{
				// Nothing has changed
				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, "No changes made to source" );
			}
		}

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