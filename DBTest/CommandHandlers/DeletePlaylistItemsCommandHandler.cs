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
			// Get the parent playlist of the first song
			Playlist parentPlaylist = ( selectedObjects.PlaylistItems.Count() == 0 ) ? null : Playlists.GetPlaylist( selectedObjects.PlaylistItems.First().PlaylistId );

			if ( ( parentPlaylist != null ) && ( parentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName ) )
			{
				// Process this Now Playing command
				NowPlayingController.DeleteNowPlayingItems( selectedObjects.PlaylistItems );
				commandCallback.PerformAction();
			}
			else
			{
				Playlist playlistSelected = selectedObjects.Playlists.FirstOrDefault();

				// If a playlist as well as songs are selected then prompt the user to check if the playlist entry should be deleted as well
				if ( ( selectedObjects.PlaylistItems.Count() > 0 ) && ( playlistSelected != null ) )
				{
					DeletePlaylistDialogFragment.ShowFragment( CommandRouter.Manager, playlistSelected, selectedObjects.PlaylistItems, DeleteSelected );
				}
				else if ( selectedObjects.PlaylistItems.Count() > 0 )
				{
					// Deletion of songs from a playlist
					PlaylistsController.DeletePlaylistItems( parentPlaylist, selectedObjects.PlaylistItems );
				}
				else
				{
					// Deletion of a playlist with no songs
					PlaylistsController.DeletePlaylist( playlistSelected );
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

			// If this is a Now Playing command then the parent of the first selected song will have the NowPlayingPlaylistName
			int songCount = selectedObjects.PlaylistItems.Count();
			int playlistCount = selectedObjects.Playlists.Count();

			// Get the parent playlist of the first song
			Playlist parentPlaylist = ( songCount == 0 ) ? null : Playlists.GetPlaylist( selectedObjects.PlaylistItems.First().PlaylistId );

			if ( ( parentPlaylist != null ) && ( parentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName ) )
			{
				isValid = true;
			}
			else
			{
				// The Delete command is only available if the selected songs are from a single playlist and only one playlist is selected.
				// Or if just a single empty playlist is selected
				// Remember the playlist could be empty
				bool singlePlaylistSongs = ( songCount > 0 ) && ( selectedObjects.PlaylistItems.Any( item => ( item.PlaylistId != parentPlaylist.Id ) ) == false );
				
				isValid = ( ( singlePlaylistSongs == true ) && ( playlistCount < 2 ) ) || ( ( songCount == 0 ) && ( playlistCount == 1 ) );
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
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.delete;
	}
}