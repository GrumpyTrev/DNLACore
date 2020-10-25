using System.Collections.Generic;
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
			if ( selectedObjects.PlaylistItems.Count() > 0 )
			{
				Playlist parentPlaylist = Playlists.GetPlaylist( selectedObjects.PlaylistItems.First().PlaylistId );
				if ( parentPlaylist != null )
				{
					if ( parentPlaylist.Name == NowPlayingController.NowPlayingPlaylistName )
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
							PlaylistsController.MoveItemsUp( parentPlaylist, selectedObjects.PlaylistItems );
						}
						else
						{
							PlaylistsController.MoveItemsDown( parentPlaylist, selectedObjects.PlaylistItems );
						}
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
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.move_up;
	}
}