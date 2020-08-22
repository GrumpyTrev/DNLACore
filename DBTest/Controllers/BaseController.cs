using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The BaseController is the Controller for common actions carried out by the Base View component
	/// Actions can only be carried out here if they do not require any model data to be accessed.
	/// </summary>
	static class BaseController
	{
		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static async void AddSongsToNowPlayingListAsync( List<Song> songsToAdd, bool clearFirst, int libraryId )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				// Before clearing it reset the selected song index to stop the current song being played
				await PlaybackAccess.SetSelectedSongAsync( -1 );
				new SongSelectedMessage() { ItemNo = -1 }.Send();

				// Now clear the Now Playing list 
				await PlaylistAccess.ClearNowPlayingListAsync( libraryId );
			}

			// Carry out the common processing to add songs to a playlist
			await PlaylistAccess.AddSongsToNowPlayingListAsync( songsToAdd, libraryId );
			new NowPlayingSongsAddedMessage().Send();

			// If the list was cleared and there are now some items in the list select the first entry
			if ( ( clearFirst == true ) & ( songsToAdd.Count > 0 ) )
			{
				await PlaybackAccess.SetSelectedSongAsync( 0 );
				new SongSelectedMessage() { ItemNo = 0 }.Send();

				// Make sure the new song is played
				new PlayCurrentSongMessage().Send();
			}
		}

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static async Task MoveItemsDownAsync( Playlist thePlaylist, List<PlaylistItem> items )
		{
			// There must be at least one PlayList entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			PlaylistItem itemToMove = thePlaylist.PlaylistItems[ items.Last().Track ];
			thePlaylist.PlaylistItems.RemoveAt( items.Last().Track );
			thePlaylist.PlaylistItems.Insert( items.First().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			await AdjustTrackNumbersAsync( thePlaylist );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static async Task MoveItemsUpAsync( Playlist thePlaylist, List<PlaylistItem> items )
		{
			// There must be at least one PlayList entry above those that are selected. That entry needs to be moved to below the end of the selection
			PlaylistItem itemToMove = thePlaylist.PlaylistItems[ items.First().Track - 2 ];
			thePlaylist.PlaylistItems.RemoveAt( items.First().Track - 2 );
			thePlaylist.PlaylistItems.Insert( items.Last().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			await AdjustTrackNumbersAsync( thePlaylist );
		}

		/// <summary>
		/// Adjust the track numbers to match the indexes in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static async Task AdjustTrackNumbersAsync( Playlist thePlaylist )
		{
			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			for ( int index = 0; index < thePlaylist.PlaylistItems.Count; ++index )
			{
				PlaylistItem itemToCheck = thePlaylist.PlaylistItems[ index ];
				if ( itemToCheck.Track != ( index + 1 ) )
				{
					itemToCheck.Track = index + 1;

					// Update the item in the model
					await PlaylistAccess.UpdatePlaylistItemAsync( itemToCheck );
				}
			}
		}

		/// <summary>
		/// Combine the specified simpe Tag and groups of tags together to provide a set of TaggedAlbums to be applied
		/// </summary>
		/// <param name="simpleTag"></param>
		/// <param name="groupTags"></param>
		/// <returns></returns>
		public static List<TaggedAlbum> CombineAlbumFilters( Tag simpleTag, List< TagGroup > groupTags )
		{
			// If any group tags have been selected combine their selected TaggedAlbum items together
			List<TaggedAlbum> albumsInFilter = new List<TaggedAlbum>();

			// It is possible that the combination of filters results in no albums, so keep track of this
			bool noMatchingAlbums = false;

			if ( groupTags.Count > 0 )
			{
				for ( int groupIndex = 0; ( noMatchingAlbums == false ) && ( groupIndex < groupTags.Count ); ++groupIndex )
				{
					List<TaggedAlbum> groupAlbums = new List<TaggedAlbum>();
					groupTags[ groupIndex ].Tags.ForEach( ta => groupAlbums.AddRange( ta.TaggedAlbums ) );

					// If this is the first group then simply copy its albums to the collection being accumulated
					if ( albumsInFilter.Count == 0 )
					{
						albumsInFilter.AddRange( groupAlbums );
					}
					else
					{
						// AND together the albums already accumulated with the albums in this group
						albumsInFilter = albumsInFilter.Intersect( groupAlbums ).ToList();
						noMatchingAlbums = ( albumsInFilter.Count == 0 );
					}
				}
			}

			if ( noMatchingAlbums == false )
			{
				// If there is a simple filter then combine it with the accumulated albums
				if ( simpleTag != null )
				{
					if ( albumsInFilter.Count == 0 )
					{
						albumsInFilter.AddRange( simpleTag.TaggedAlbums );
					}
					else
					{
						// AND together the albums already accumulated with the albums in this group
						albumsInFilter = albumsInFilter.Intersect( simpleTag.TaggedAlbums ).ToList();
					}
				}
			}

			return albumsInFilter;
		}
	}
}