using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The MoveItemsCommandHandler class is used to process a request to move items up a playlist
	/// </summary>
	class MoveItemsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command.
		/// Pass the request on to the PlaylistsController having first worked out the parent playlist
		/// If the playlist is a NowPlaying playlist then pass the request on to the NowPlayingController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			if ( selectedObjects.ParentPlaylist != null )
			{
				if ( selectedObjects.ParentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName )
				{
					if ( commandIdentity == Resource.Id.move_up )
					{
						NowPlayingController.MoveItemsUp( selectedObjects.PlaylistItems );
					}
					else
					{
						NowPlayingController.MoveItemsDown( selectedObjects.PlaylistItems );
					}
				}
				else
				{
					if ( commandIdentity == Resource.Id.move_up )
					{
						PlaylistsController.MoveItemsUp( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
					}
					else
					{
						PlaylistsController.MoveItemsDown( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
					}
				}
			}
			else if ( selectedObjects.TaggedAlbums.Count > 0 )
			{
				Tag firstTag = Tags.TagsCollection.Single( tag => tag.Id == selectedObjects.TaggedAlbums[ 0 ].TagId );

				if ( commandIdentity == Resource.Id.move_up )
				{
					PlaylistsController.MoveItemsUp( firstTag, selectedObjects.TaggedAlbums );
				}
				else
				{
					PlaylistsController.MoveItemsDown( firstTag, selectedObjects.TaggedAlbums );
				}
			}
		}

		/// <summary>
		/// Bind this command to the router. An override is required here as this command can be launched using two identities
		/// </summary>
		public override void BindToRouter()
		{
			CommandRouter.BindHandler( CommandIdentity, this );
			CommandRouter.BindHandler( Resource.Id.move_down, this );
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// If at least one playlist item is selected and its parent is a Now Playing list then enable this command.
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int commandIdentity )
		{
			bool isValid = false;

			// Check for song playlists first
			if ( ( selectedObjects.PlaylistItems.Count > 0 ) && ( selectedObjects.TaggedAlbums.Count == 0 ) )
			{
				// Determine which is the key playlistitem to check for selection
				int keyItemId = ( commandIdentity == Resource.Id.move_up ) ? selectedObjects.ParentPlaylist.PlaylistItems[ 0 ].Id :
					selectedObjects.ParentPlaylist.PlaylistItems.Last().Id;

				// If this is the NowPlaying list then the move up command is valid if the first playlist item is not selected, and the
				// move down command is valid if the last playlist item is not selected
				if ( selectedObjects.ParentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName )
				{
					isValid = ( selectedObjects.PlaylistItems.Any( item => ( item.Id == keyItemId ) ) == false );
				}
				else
				{
					// The move up / move down is available if all the songs are from a single playlist and that playlist is not selected, i.e. not all
					// of its songs are selected
					bool itemsFromSinglePlaylist = selectedObjects.PlaylistItems.Any( item => ( item.PlaylistId != selectedObjects.ParentPlaylist.Id ) ) == false;

					if ( ( itemsFromSinglePlaylist == true ) && ( selectedObjects.Playlists.Any( list => ( list.Id == selectedObjects.ParentPlaylist.Id ) ) == false ) )
					{
						isValid = ( selectedObjects.PlaylistItems.Any( item => ( item.Id == keyItemId ) ) == false );
					}
				}
			}
			else if ( ( selectedObjects.TaggedAlbums.Count > 0 ) && ( selectedObjects.PlaylistItems.Count == 0 ) )
			{
				Tag firstTag = Tags.TagsCollection.Single( tag => tag.Id == selectedObjects.TaggedAlbums[ 0 ].TagId );

				// The move up / move down is available if all TaggedAlbums are from a single Tag and that Tag is not selected.
				bool itemsfromSingleTag = selectedObjects.TaggedAlbums.Any( item => item.TagId != firstTag.Id ) == false;
				if ( ( itemsfromSingleTag == true ) && ( selectedObjects.Tags.Any( tag => tag.Id == firstTag.Id ) == false ) )
				{
					int keyItemId = ( commandIdentity == Resource.Id.move_up ) ? firstTag.TaggedAlbums[ 0 ].Id : firstTag.TaggedAlbums.Last().Id;
					isValid = ( selectedObjects.TaggedAlbums.Any( ta => ta.Id == keyItemId ) == false );
				}
			}

			return isValid;
		}

		/// <summary>
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.move_up;
	}
}