using System;
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
		/// Called to handle the command. Show the library selection dialogue and pass on any selected librray to the SourceSelectionDialog
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => NewLibraryNameDialog.Show( "New library name", "",
			newNameAction: (string libraryName, Action dismissAction) =>
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
					NotificationDialog.Show( alertText );
				}
				else
				{
					// Dismiss the library name dialogue
					dismissAction.Invoke();
				}
			} );

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
