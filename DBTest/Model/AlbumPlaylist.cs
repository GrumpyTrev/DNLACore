using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The SongPlaylist class contains an ordered collection of songs wrapped up in SongPlaylistItems
	/// </summary>
	public partial class AlbumPlaylist : Playlist
	{
		/// <summary>
		/// Get the PlaylistItems and associated songs for this playlist
		/// </summary>
		/// <param name="playlistItems"></param>
		public void GetContents( IEnumerable<PlaylistItem> playlistItems )
		{
			// Get all the AlbumPlaylistItem entries associated with this AlbumPlaylist and then the Album entries for each of them
			PlaylistItems.AddRange( playlistItems.Where( item => item.PlaylistId == this.Id ) );

			foreach ( AlbumPlaylistItem playlistItem in PlaylistItems )
			{
				playlistItem.Album = Albums.GetAlbumById( playlistItem.AlbumId );
			}

			PlaylistItems.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );
		}

		/// <summary>
		/// Add a list of albums to the playlist
		/// </summary>
		/// <param name="albums"></param>
		public void AddAlbums( IEnumerable<Album> albums )
		{
			// For each song create an AlbumPlayListItem and add to the PlayList
			foreach ( Album album in albums )
			{
				AlbumPlaylistItem itemToAdd = new AlbumPlaylistItem()
				{
					Album = album,
					PlaylistId = Id,
					AlbumId = album.Id,
					Index = PlaylistItems.Count
				};

				AddItem( itemToAdd );
			}

			new PlaylistSongsAddedMessage() { Playlist = this }.Send();
		}
	}
}