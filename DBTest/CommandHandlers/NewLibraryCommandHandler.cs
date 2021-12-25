namespace DBTest
{
	/// <summary>
	/// The NewLibraryCommandHandler class is used to process a request to add a new library.
	/// The NewLibraryNameDialogFragment class is used to specify the name of the new library.
	/// Once a library has been selected the SourceSelectionDialogFragment is used to display the sources associated with the library.
	/// When a source is selected the SourceEditDialogFragment is used to to display the source details and allow it to be edited.
	/// A reference to the SourceSelectionDialogFragment must be maintained to allow and edited source to be redisplayed
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
		private async void NameEntered( string libraryName, NewLibraryNameDialogFragment libraryNameFragment )
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
				if ( Libraries.LibraryNames.Contains( libraryName ) == true )
				{
					alertText = DuplicateLibraryError;
				}
				else
				{
					// Create a library with a default source and display the source editing fragment
					Library newLibrary = new() { Name = libraryName };
					await Libraries.AddLibraryAsync( newLibrary );

					// Add a source
					Source newSource = new() { Name = libraryName, AccessMethod = Source.AccessType.Local, FolderName = libraryName, LibraryId = newLibrary.Id };
					await Sources.AddSourceAsync( newSource );

					// Add an empty NowPlaying list
					Playlist nowPlaying = new SongPlaylist() { Name = Playlists.NowPlayingPlaylistName, LibraryId = newLibrary.Id };
					await Playlists.AddPlaylistAsync( nowPlaying );
					nowPlaying.SongIndex = -1;

					SourceSelectionDialogFragment.ShowFragment( CommandRouter.Manager, newLibrary, SourceSelected, BindDialog );
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
		/// Called when a source has been selected to edit.
		/// Display the SoureEditDialogFragment
		/// </summary>
		/// <param name="selectedSource"></param>
		private void SourceSelected( Source selectedSource )
		{
//			SourceEditDialogFragment.ShowFragment( CommandRouter.Manager, selectedSource, SourceChanged );
		}

		/// <summary>
		/// Called when the SourceSelectionDialogFragment dialog is displayed (OnResume)
		/// Save the reference for updating the sources if tey are edited
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( SourceSelectionDialogFragment dialogue ) => sourceSelectionDialog = dialogue;

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.new_library;

				/// <summary>
		/// SourceSelectionDialogFragment reference held so that it can be infromed of source detail changes
		/// </summary>
		private SourceSelectionDialogFragment sourceSelectionDialog = null;

		/// <summary>
		/// Possible errors due to playlist name entry
		/// </summary>
		private const string EmptyNameError = "An empty name is not valid.";
		private const string DuplicateLibraryError = "A library with that name already exists.";
	}
}
