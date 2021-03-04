using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The AddSongsToNowPlayingListCommandHandler class is used to process a command to add selected songs to the Now Playing list.
	/// The songs can be selected from the Artists tab, the Albums tab and the Playlists tab. For the Artists and Albums tabs the selected items are
	/// Song objects, for the Playlists tab they are either PlaylistItem or TaggedAlbum objects
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
				BaseController.AddSongsToNowPlayingList( selectedObjects.PlaylistItems.Select( item => item.Song ), ( commandIdentity == Resource.Id.play_now ) );
			}
			// If Albums are selected then get the songs from them
			else if ( selectedObjects.TaggedAlbums.Count > 0 )
			{
				BaseController.AddSongsToNowPlayingList( selectedObjects.TaggedAlbums.SelectMany( item => item.Album.Songs ), ( commandIdentity == Resource.Id.play_now ) );
			}
			// Otherwise just use the selected songs themselves
			else
			{
				BaseController.AddSongsToNowPlayingList( selectedObjects.Songs, ( commandIdentity == Resource.Id.play_now ) );
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
		protected override bool IsSelectionValidForCommand( int _ ) => 
			( selectedObjects.PlaylistItems.Count > 0 ) || ( selectedObjects.Songs.Count > 0 ) || ( selectedObjects.TaggedAlbums.Count > 0 );

		/// <summary>
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_to_queue;
	}
}