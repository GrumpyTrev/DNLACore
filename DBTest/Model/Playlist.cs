using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Playlist class contains an ordered collection of songs wrapped up in 
	/// PlaylistItems
	/// </summary>
	public partial class Playlist
	{
		/// <summary>
		/// Get the PlaylistItems and associated songs for this playlist
		/// </summary>
		/// <param name="playlist"></param>
		public async Task GetContentsAsync( List<PlaylistItem> playlistItems )
		{
			// Get all the PlaylistItem entries associated with this Playlist and then the Song entries for each of them
			PlaylistItems.AddRange( playlistItems.Where( item => item.PlaylistId == this.Id ) );

			foreach ( PlaylistItem playlistItem in PlaylistItems )
			{
				playlistItem.Song = await DbAccess.GetSongAsync( playlistItem.SongId );
				playlistItem.Artist = Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( playlistItem.Song.ArtistAlbumId ).ArtistId );
				playlistItem.Song.Artist = playlistItem.Artist;
			}

			PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Delete the specified PlayListItems from the Playlist
		/// </summary>
		/// <param name="items"></param>
		public void DeletePlaylistItems( IEnumerable<PlaylistItem> items )
		{
			foreach ( PlaylistItem item in items )
			{
				PlaylistItems.Remove( item );
			}

			DbAccess.DeleteAsync( items );
		}

		/// <summary>
		/// Clear the contents of the playlist
		/// </summary>
		/// <param name="playlistToClear"></param>
		public void Clear() => DeletePlaylistItems( new List<PlaylistItem>( PlaylistItems ) );

		/// <summary>
		/// Add a list of songs to the playlist
		/// </summary>
		/// <param name="playlist"></param>
		/// <param name="songs"></param>
		public void AddSongs( IEnumerable<Song> songs )
		{
			// For each song create a PlayListItem and add to the PlayList
			foreach ( Song song in songs )
			{
				song.Artist = Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( song.ArtistAlbumId ).ArtistId );

				PlaylistItem itemToAdd = new PlaylistItem()
				{
					Artist = song.Artist,
					PlaylistId = Id,
					Song = song,
					SongId = song.Id,
					Track = PlaylistItems.Count + 1
				};

				PlaylistItems.Add( itemToAdd );
				DbAccess.InsertAsync( itemToAdd );
			}

			new PlaylistSongsAddedMessage() { Playlist = this }.Send();
		}

		/// <summary>
		/// Move a set of selected items down and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsDown( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			PlaylistItem itemToMove = PlaylistItems[ items.Last().Track ];
			PlaylistItems.RemoveAt( items.Last().Track );
			PlaylistItems.Insert( items.First().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			AdjustTrackNumbers();
		}

		/// <summary>
		/// Move a set of selected items up and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsUp( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry above those that are selected. That entry needs to be moved to below the end of the selection
			PlaylistItem itemToMove = PlaylistItems[ items.First().Track - 2 ];
			PlaylistItems.RemoveAt( items.First().Track - 2 );
			PlaylistItems.Insert( items.Last().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			AdjustTrackNumbers();
		}

		/// <summary>
		/// Adjust the track numbers to match the indexes in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		public void AdjustTrackNumbers()
		{
			// The track numbers in the PlaylistItems must be updated to match their index in the collection
			for ( int index = 0; index < PlaylistItems.Count; ++index )
			{
				PlaylistItem itemToCheck = PlaylistItems[ index ];
				if ( itemToCheck.Track != ( index + 1 ) )
				{
					itemToCheck.Track = index + 1;

					// Update the item in the model. No need to wait for this.
					DbAccess.UpdateAsync( itemToCheck );
				}
			}
		}

		/// <summary>
		/// Change the name of this playlist
		/// </summary>
		/// <param name="newName"></param>
		public void Rename( string newName )
		{
			Name = newName;

			// Update the item in the model. No need to wait for this.
			DbAccess.UpdateAsync( this );
		}

		/// <summary>
		/// The PlaylistItems associated with this playlist
		/// </summary>
		[Ignore]
		public List<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
	}
}