using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The RenamePlaylistCommandHandler class is used to process a command to rename a playlist.
	/// The NewPlaylistNameDialogFragment class is used to provide a new playlist name.
	/// This is validated and if valid the playlist is renamed using the PlaylistController.
	/// </summary>
	class RenamePlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the NewPlaylistNameDialogFragment.
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => 
			NewPlaylistNameDialogFragment.ShowFragment( CommandRouter.Manager, NameEntered, "Rename playlist", Resource.Layout.rename_playlist_dialogue_layout,
				selectedObjects.Playlists.First().Name );

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Playlists.Count() == 1 );

		/// <summary>
		/// Called when a library has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void NameEntered( string playlistName, NewPlaylistNameDialogFragment playlistNameFragment )
		{
			string alertText = "";

			if ( playlistName.Length == 0 )
			{
				alertText = "An empty name is not valid.";
			}
			else if ( playlistName == selectedObjects.Playlists.First().Name )
			{
				alertText = "Name not changed.";
			}
			else if ( PlaylistsViewModel.PlaylistNames.Contains( playlistName ) == true )
			{
				alertText = "A playlist with that name already exists.";
			}
			else
			{
				PlaylistsController.RenamePlaylist( selectedObjects.Playlists.First(), playlistName );
				playlistNameFragment.Dismiss();
				commandCallback.PerformAction();
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
		protected override int CommandIdentity { get; } = Resource.Id.rename;
	}
}