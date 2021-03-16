using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The SongPlaylist class contains an ordered collection of songs wrapped up in SongPlaylistItems
	/// </summary>
	public partial class SongPlaylist : Playlist
	{
		/// <summary>
		/// Get the PlaylistItems and associated songs for this playlist
		/// </summary>
		/// <param name="playlistItems"></param>
		public async Task GetContentsAsync( IEnumerable<PlaylistItem> playlistItems )
		{
			// Get all the SongPlaylistItem entries associated with this SongPlaylist and then the Song entries for each of them
			PlaylistItems.AddRange( playlistItems.Where( item => item.PlaylistId == this.Id ) );

			foreach ( SongPlaylistItem playlistItem in PlaylistItems )
			{
				playlistItem.Song = await DbAccess.GetSongAsync( playlistItem.SongId );
				playlistItem.Artist = Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( playlistItem.Song.ArtistAlbumId ).ArtistId );
				playlistItem.Song.Artist = playlistItem.Artist;
			}

			PlaylistItems.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );
		}

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

				SongPlaylistItem itemToAdd = new SongPlaylistItem()
				{
					Artist = song.Artist,
					PlaylistId = Id,
					Song = song,
					SongId = song.Id,
					Index = PlaylistItems.Count
				};

				AddItem( itemToAdd );
			}

			new PlaylistSongsAddedMessage() { Playlist = this }.Send();
		}

		/// <summary>
		/// Extract all teh Songs from this playlist
		/// </summary>
		/// <returns></returns>
		public List<Song> GetSongs() => PlaylistItems.Select( item => ( ( SongPlaylistItem )item ).Song ).ToList();

		/// <summary>
		/// Delete any songs in this playlist that are contained in the supplied collection
		/// </summary>
		/// <param name="songIds"></param>
		public void DeleteMatchingSongs( List<int> songIds )
		{
			List<SongPlaylistItem> itemsToDelete = new List<SongPlaylistItem>();

			foreach ( SongPlaylistItem songPlaylistItem in PlaylistItems )
			{
				if ( songIds.Contains( songPlaylistItem.SongId ) == true )
				{
					itemsToDelete.Add( songPlaylistItem );
				}
			}

			DeletePlaylistItems( itemsToDelete );
		}
	}
}