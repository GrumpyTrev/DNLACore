using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The DeletePlaylistItemsCommandHandler class is used to delete songs from a playlist
	/// The same handler is used for both the Now Playing and user playlists, but they need different processing.
	/// </summary>
	class DeletePlaylistItemsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Determine the subject of this, either the Now Playing playlist or a user playlist
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// If this is a Now Playing command then the parent of the first selected song will have the NowPlayingPlaylistName
			if ( ( selectedObjects.ParentPlaylist != null ) && ( selectedObjects.ParentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName ) )
			{
				// Process this Now Playing command
				NowPlayingController.DeleteNowPlayingItems( selectedObjects.PlaylistItems );
				commandCallback.PerformAction();
			}
			else
			{
				Playlist playlistSelected = selectedObjects.Playlists.FirstOrDefault();

				// If a playlist as well as songs are selected then prompt the user to check if the playlist entry should be deleted as well
				if ( ( selectedObjects.PlaylistItemsCount > 0 ) && ( playlistSelected != null ) )
				{
					DeletePlaylistDialogFragment.ShowFragment( CommandRouter.Manager, playlistSelected, selectedObjects.PlaylistItems, DeleteSelected );
				}
				else if ( selectedObjects.PlaylistItemsCount > 0 )
				{
					// Deletion of songs from a playlist
					PlaylistsController.DeletePlaylistItems( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
					commandCallback.PerformAction();
				}
				else
				{
					// Deletion of a playlist with no songs
					PlaylistsController.DeletePlaylist( playlistSelected );
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
			if ( ( selectedObjects.ParentPlaylist != null ) && ( selectedObjects.ParentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName ) )
			{
				isValid = true;
			}
			else
			{
				// The Delete command is only available if the selected playlistitems are from a single playlist and only one playlist is selected.
				// Or if just a single empty playlist is selected
				// Remember the playlist could be empty
				bool itemsFromSinglePlaylist = ( selectedObjects.PlaylistItemsCount > 0 ) &&
					( selectedObjects.PlaylistItems.All( item => ( item.PlaylistId == selectedObjects.ParentPlaylist.Id ) ) == true );
				
				isValid = ( ( itemsFromSinglePlaylist == true ) && ( selectedObjects.PlaylistsCount < 2 ) ) || 
					( ( selectedObjects.PlaylistItemsCount == 0 ) && ( selectedObjects.PlaylistsCount == 1 ) );
			}

			return isValid;
		}

		/// <summary>
		/// Called when the user has decided whether to delete the entire playlist or just the items in it
		/// </summary>
		/// <param name="deletePlaylist"></param>
		private void DeleteSelected( bool deletePlaylist )
		{
			Playlist playlist = selectedObjects.Playlists.First();
			if ( deletePlaylist == true )
			{
				PlaylistsController.DeletePlaylist( playlist );
			}
			else
			{
				PlaylistsController.DeletePlaylistItems( playlist, selectedObjects.PlaylistItems );
			}

			commandCallback.PerformAction();
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.delete;
	}
}