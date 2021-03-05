using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The DuplicatePlaylistCommandHandler class is used to duplicate a playlist in other libraries
	/// </summary>
	class DuplicatePlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Determine the subject of this, either the Now Playing playlist or a user playlist
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// If the playlist already exists in other libraries then prompt for deletion
			Playlist playlistToDuplicate = selectedObjects.Playlists[ 0 ];
			if ( PlaylistsController.CheckForOtherPlaylists( playlistToDuplicate.Name, ConnectionDetailsModel.LibraryId ) == true )
			{
				ConfirmationDialogFragment.ShowFragment( CommandRouter.Manager, 
					( bool confirm ) => { if ( confirm == true ) PlaylistsController.DuplicatePlaylistAsync( playlistToDuplicate ); }, 
					"The playlist already exists in other libraries. Are you sure you want to duplicate it?" );
			}
			else
			{
				// Duplicate the playlist in the other libraries
				PlaylistsController.DuplicatePlaylistAsync( playlistToDuplicate );
			}

			commandCallback.PerformAction();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Playlists.Count() == 1 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.duplicate;
	}
}