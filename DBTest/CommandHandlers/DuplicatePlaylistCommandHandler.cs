using System.Collections.Generic;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The DuplicatePlaylistCommandHandler class is used to duplicate a playlist in other libraries
	/// </summary>
	internal class DuplicatePlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Duplicate each selected playlist. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// Make a copy of the selected playlists and start duplicating them
			playlistsBeingDuplicated = new List<Playlist>( selectedObjects.Playlists );
			playlistIndex = -1;

			DuplicateNextPlaylist();
		}

		/// <summary>
		/// Is the command valid given the selected objects.
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Playlists.Count >= 1 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.duplicate;

		/// <summary>
		/// Called 
		/// </summary>
		/// <param name="confirm"></param>
		private void DuplicationConfirmed( bool confirm )
		{
			if ( confirm == true )
			{
				PlaylistsController.DuplicatePlaylist( playlistsBeingDuplicated[ playlistIndex ] );
			}

			DuplicateNextPlaylist();
		}

		/// <summary>
		/// Duplciate the next playlist in the list
		/// </summary>
		private void DuplicateNextPlaylist()
		{
			if ( ++playlistIndex < playlistsBeingDuplicated.Count )
			{
				Playlist nextPlaylist = playlistsBeingDuplicated[ playlistIndex ];

				// If the playlist already exists in other libraries then prompt for deletion
				if ( PlaylistsController.CheckForOtherPlaylists( nextPlaylist.Name, ConnectionDetailsModel.LibraryId ) == true )
				{
					ConfirmationDialog.Show( $"Playlist [{nextPlaylist.Name}] already exists in other libraries. Are you sure you want to duplicate it?",
						() => DuplicationConfirmed( true), () => DuplicationConfirmed( false ) );
				}
				else
				{
					// Duplicate the playlist in the other libraries
					PlaylistsController.DuplicatePlaylist( nextPlaylist );

					DuplicateNextPlaylist();
				}
			}
			else
			{
				playlistsBeingDuplicated = null;
				playlistIndex = -1;

				commandCallback.PerformAction();
			}
		}

		/// <summary>
		/// A local copy of the playlists being duplicated
		/// </summary>
		private List<Playlist> playlistsBeingDuplicated = null;

		/// <summary>
		/// Index of the playlist currently being duplicated
		/// </summary>
		private int playlistIndex = -1;
	}
}
