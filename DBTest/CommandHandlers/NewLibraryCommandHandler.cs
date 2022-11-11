using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The NewLibraryCommandHandler class is used to process a request to add a new library.
	/// The NewLibraryNameDialogFragment class is used to specify the name of the new library.
	/// </summary>
	internal class NewLibraryCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the SourceSelectionDialogFragment
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => NewLibraryNameDialogFragment.ShowFragment( CommandRouter.Manager, NameEntered, "New library name", "" );

		/// <summary>
		/// Called when a playlist name has been entered has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void NameEntered( string libraryName, NewLibraryNameDialogFragment libraryNameFragment )
		{
			string alertText = "";

			// An empty library name is not allowed
			if ( libraryName.Length == 0 )
			{
				alertText = EmptyNameError;
			}
			else
			{
				// Check for a duplicate
				if ( LibraryManagementViewModel.LibraryNames.Contains( libraryName ) == true )
				{
					alertText = DuplicateLibraryError;
				}
				else
				{
					// Create a new library
					MainApp.CommandInterface.CreateLibrary( libraryName );
				}
			}

			// Display an error message if the playlist name is not valid. 
			if ( alertText.Length > 0 )
			{
				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, alertText );
			}
			else
			{
				// Dismiss the library name dialogue
				libraryNameFragment.Dismiss();
			}
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.new_library;

		/// <summary>
		/// Possible errors due to playlist name entry
		/// </summary>
		private const string EmptyNameError = "An empty name is not valid.";
		private const string DuplicateLibraryError = "A library with that name already exists.";
	}
}
