using System.Linq;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The DeletePlaylistItemsCommandHandler class is used to delete songs and albums from a playlist
	/// The same handler is used for both the Now Playing and user playlists, but they need different processing.
	/// </summary>
	internal class DeletePlaylistItemsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Determine the subject of this, either the Now Playing playlist or a user playlist
		/// </summary>
		/// <param name="_"></param>
		public override void HandleCommand( int _ )
		{
			// If this is a Now Playing command then the parent of the first selected song will have the NowPlayingPlaylistName
			if ( ( selectedObjects.ParentPlaylist != null ) && ( selectedObjects.ParentPlaylist.Name == Playlist.NowPlayingPlaylistName ) )
			{
				// Process this Now Playing command
				NowPlayingController.DeleteNowPlayingItems( selectedObjects.PlaylistItems );
				commandCallback.PerformAction();
			}
			else
			{
				if ( selectedObjects.PlaylistItems.Count > 0 )
				{
					if ( selectedObjects.Playlists.Count == 1 )
					{
						// Deletion of songs and playlist - confirm first
						ConfirmationDialogFragment.Show( "Do you want to delete the Playlist?", () => PlaylistDeleteSelected( true ), () => PlaylistDeleteSelected( false ) );
					}
					else
					{
						// Deletion of items from a playlist
						PlaylistsController.DeletePlaylistItems( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
						commandCallback.PerformAction();
					}
				}
				else if ( selectedObjects.Playlists.Count == 1 )
				{
					// Deletion of a playlist with no songs
					PlaylistsController.DeletePlaylist( selectedObjects.Playlists[ 0 ] );
					commandCallback.PerformAction();
				}
			}
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// If at least one playlist item is selected and its parent is a Now Playing list then enable this command.
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ )
		{
			bool isValid = false;

			// If this is a Now Playing command then the parent of the first selected playlistitem will have the NowPlayingPlaylistName
			if ( ( selectedObjects.ParentPlaylist != null ) && ( selectedObjects.ParentPlaylist.Name == Playlist.NowPlayingPlaylistName ) )
			{
				isValid = true;
			}
			else
			{
				// The Delete command is only available if the selected items PlaylistItem are from a single Playlist and only one 
				// Playlist is selected. Or if just a single empty IPlaylist
				// Remember the playlist could be empty
				if ( ( selectedObjects.Playlists.Count > 0 ) || ( selectedObjects.PlaylistItems.Count > 0 ) )
				{
					isValid = ( selectedObjects.Playlists.Count < 2 ) && ( ( selectedObjects.PlaylistItems.Count == 0 ) ||
						( selectedObjects.PlaylistItems.All( item => ( item.PlaylistId == selectedObjects.ParentPlaylist.Id ) ) == true ) );
				}
			}

			return isValid;
		}

		/// <summary>
		/// Called when the user has decided whether to delete the entire playlist or just the items in it
		/// </summary>
		/// <param name="deletePlaylist"></param>
		private void PlaylistDeleteSelected( bool deletePlaylist )
		{
			if ( deletePlaylist == true )
			{
				PlaylistsController.DeletePlaylist( selectedObjects.Playlists[ 0 ] );
			}
			else
			{
				PlaylistsController.DeletePlaylistItems( selectedObjects.Playlists[ 0 ], selectedObjects.PlaylistItems );
			}

			commandCallback.PerformAction();
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.delete;
	}
}
