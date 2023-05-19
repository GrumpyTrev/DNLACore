using System.Linq;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The MoveItemsCommandHandler class is used to process a request to move items up a playlist
	/// </summary>
	internal class MoveItemsCommandHandler : CommandHandler
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
				if ( selectedObjects.ParentPlaylist.Name == Playlist.NowPlayingPlaylistName )
				{
					if ( commandIdentity == Resource.Id.move_up )
					{
						MainApp.CommandInterface.MoveItemsUp( selectedObjects.PlaylistItems );
					}
					else
					{
						MainApp.CommandInterface.MoveItemsDown( selectedObjects.PlaylistItems );
					}
				}
				else
				{
					if ( commandIdentity == Resource.Id.move_up )
					{
						MainApp.CommandInterface.MoveItemsUp( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
					}
					else
					{
						MainApp.CommandInterface.MoveItemsDown( selectedObjects.ParentPlaylist, selectedObjects.PlaylistItems );
					}
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
			if ( selectedObjects.PlaylistItems.Count > 0 )
			{
				// Determine which is the key playlistitem to check for selection
				int keyItemId = ( commandIdentity == Resource.Id.move_up ) ? selectedObjects.ParentPlaylist.PlaylistItems[ 0 ].Id :
					selectedObjects.ParentPlaylist.PlaylistItems.Last().Id;

				// If this is the NowPlaying list then the move up command is valid if the first playlist item is not selected, and the
				// move down command is valid if the last playlist item is not selected
				if ( selectedObjects.ParentPlaylist.Name == Playlist.NowPlayingPlaylistName )
				{
					isValid = ( selectedObjects.PlaylistItems.Any( item => ( item.Id == keyItemId ) ) == false );
				}
				else
				{
					// The move up / move down is available if all the songs are from a single playlist and that playlist is not selected
					if ( selectedObjects.Playlists.Count == 0 )
					{
						isValid = ( selectedObjects.PlaylistItems.Any( item => ( item.PlaylistId != selectedObjects.ParentPlaylist.Id ) ) == false ) &&
							( selectedObjects.PlaylistItems.Any( item => ( item.Id == keyItemId ) ) == false );
					}
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
