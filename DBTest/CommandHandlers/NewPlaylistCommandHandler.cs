namespace DBTest
{
	/// <summary>
	/// The NewPlaylistCommandHandler class is used to process a command to add a new playlist.
	/// The NewPlaylistNameDialogFragment class is used to provide a new playlist name.
	/// This is validated and if valid a new playlist is created using the PlaylistController.
	/// </summary>
	class NewPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the NewPlaylistNameDialogFragment.
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => 
			NewPlaylistNameDialogFragment.ShowFragment( CommandRouter.Manager, NameEntered, "New playlist", "" );

		/// <summary>
		/// Called when a library has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void NameEntered( string playlistName, NewPlaylistNameDialogFragment playlistNameFragment, bool _ )
		{
			string alertText = "";

			if ( playlistName.Length == 0 )
			{
				alertText = "An empty name is not valid.";
			}
			else if ( PlaylistsViewModel.PlaylistNames.Contains( playlistName ) == true )
			{
				alertText = "A playlist with that name already exists.";
			}
			else
			{
				// No need to wait for this as the playlist is not going to be used straight away
				PlaylistsController.AddPlaylistAsync( playlistName );
				playlistNameFragment.Dismiss();
			}

			// Display an error message if the playlist name is not valid. Do not dismiss the dialog
			if ( alertText.Length > 0 )
			{
				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, alertText );
			}
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.new_playlist;
	}
}