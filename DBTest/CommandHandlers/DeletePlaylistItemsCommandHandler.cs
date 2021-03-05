using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The DeletePlaylistItemsCommandHandler class is used to delete songs and albums from a playlist
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
				if ( selectedObjects.PlaylistItems.Count > 0 )
				{
					if ( selectedObjects.Playlists.Count == 1 )
					{
						// Deletion of songs and playlist - confirm first
						ConfirmationDialogFragment.ShowFragment( CommandRouter.Manager, PlaylistDeleteSelected, "Do you want to delete the Playlist?" );
					}
					else
					{
						// Deletion of songs from a playlist
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
				else if ( selectedObjects.TaggedAlbums.Count > 0 )
				{
					if ( selectedObjects.Tags.Count == 1 )
					{
						// Deletion of TaggedAlbums and Tag - confirm first
						ConfirmationDialogFragment.ShowFragment( CommandRouter.Manager, TagDeleteSelected, "Do you want to delete the Tag?" );
					}
					else
					{
						// Deletion of TaggedAlbums from a Tag
						FilterManagementController.DeleteTaggedAlbums( Tags.GetTagById( selectedObjects.TaggedAlbums[ 0 ].TagId ), selectedObjects.TaggedAlbums );
						commandCallback.PerformAction();
					}
				}
				else
				{
					// Deletion of an empty Tag
					FilterManagementController.DeleteTag( selectedObjects.Tags[ 0 ] );
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
				// The Delete command is only available if the selected items (PlaylistItem or TaggedAlbum) are from a single Playlist or Tag and only one 
				// Playlist or Tag is selected. Or if just a single empty Playlist or Tag is selected
				// Remember the playlist could be empty

				// Check for Playlists and PlaylistItems first
				if ( ( ( selectedObjects.Playlists.Count > 0 ) || ( selectedObjects.PlaylistItems.Count > 0 ) ) &&
					( selectedObjects.Tags.Count == 0 ) && ( selectedObjects.TaggedAlbums.Count == 0 ) )
				{
					isValid = ( selectedObjects.Playlists.Count < 2 ) && ( ( selectedObjects.PlaylistItems.Count == 0 ) ||
						( selectedObjects.PlaylistItems.All( item => ( item.PlaylistId == selectedObjects.ParentPlaylist.Id ) ) == true ) );
				}
				// Now the same for Tags and TaggedAlbums
				else if ( ( ( selectedObjects.Tags.Count > 0 ) || ( selectedObjects.TaggedAlbums.Count > 0 ) ) &&
					( selectedObjects.Playlists.Count == 0 ) && ( selectedObjects.PlaylistItems.Count == 0 ) )
				{
					int parentTagId = ( selectedObjects.TaggedAlbums.Count > 0 ) ? Tags.GetTagById( selectedObjects.TaggedAlbums[ 0 ].TagId ).Id : -1;

					isValid = ( selectedObjects.Tags.Count < 2 ) && ( ( selectedObjects.TaggedAlbums.Count == 0 ) ||
						( selectedObjects.TaggedAlbums.All( item => ( item.TagId == parentTagId ) ) == true ) );
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
		/// Called when the user has decided whether to delete the entire tag or just the items in it
		/// </summary>
		/// <param name="deleteTag"></param>
		private void TagDeleteSelected( bool deleteTag )
		{
			if ( deleteTag == true )
			{
				FilterManagementController.DeleteTag( selectedObjects.Tags[ 0 ] );
			}
			else
			{
				FilterManagementController.DeleteTaggedAlbums( selectedObjects.Tags[ 0 ], selectedObjects.TaggedAlbums );
			}

			commandCallback.PerformAction();
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.delete;
	}
}