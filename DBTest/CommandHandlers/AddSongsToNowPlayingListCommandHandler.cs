using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AddSongsToNowPlayingListCommandHandler class is used to process a command to add selected songs to the Now Playing list.
	/// The songs can be selected from the Artists tab, the Albums tab and the Playlists tab. For the Artists and Albums tabs the selected items are
	/// Song objects, for the Playlists tab they are either SongPlaylistItem or AlbumPlaylistItem objects
	/// </summary>
	class AddSongsToNowPlayingListCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// If PlaylistItems are selected then get the songs from them
			if ( selectedObjects.PlaylistItems.Count > 0 )
			{
				// This includes both SongPlaylistItem and AlbumPlaylistItem entries.
				List<Song> selectedSongs = new List<Song>();
				foreach ( PlaylistItem basePlaylistItem in selectedObjects.PlaylistItems )
				{
					if ( basePlaylistItem is AlbumPlaylistItem albumPlaylistItem )
					{
						selectedSongs.AddRange( albumPlaylistItem.Album.Songs );
					}
					else
					{
						selectedSongs.Add( ( ( SongPlaylistItem )basePlaylistItem ).Song );
					}
				}

				NowPlayingController.AddSongsToNowPlayingList( selectedSongs, ( commandIdentity == Resource.Id.play_now ) );
			}
			// Otherwise just use the selected songs themselves
			else
			{
				NowPlayingController.AddSongsToNowPlayingList( selectedObjects.Songs, ( commandIdentity == Resource.Id.play_now ) );
			}

			commandCallback.PerformAction();
		}

		/// <summary>
		/// Bind this command to the router. An override is required here as this command can be launched using two identities
		/// </summary>
		public override void BindToRouter()
		{
			CommandRouter.BindHandler( CommandIdentity, this );
			CommandRouter.BindHandler( Resource.Id.play_now, this );
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.PlaylistItems.Count > 0 ) || ( selectedObjects.Songs.Count > 0 );

		/// <summary>
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_to_queue;
	}
}