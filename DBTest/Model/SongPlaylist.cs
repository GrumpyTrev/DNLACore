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
		public async Task GetContentsAsync( IEnumerable<SongPlaylistItem> playlistItems )
		{
			// Get all the SongPlaylistItem entries associated with this SongPlaylist and then the Song entries for each of them
			PlaylistItems.AddRange( playlistItems.Where( item => item.PlaylistId == this.Id ) );

			// Get all the associated Songs in one go and then link to the SongPlaylistItem items
			Dictionary<int, Song> songs = ( await DbAccess.GetSongsAsync( PlaylistItems.Select( play => ( play as SongPlaylistItem ).SongId ) ) )
				.ToDictionary( song => song.Id );

			foreach ( SongPlaylistItem playlistItem in PlaylistItems )
			{
				playlistItem.Song = songs[ playlistItem.SongId ];
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
		}

		/// <summary>
		/// Extract all the Songs from this playlist
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

		/// <summary>
		/// The Song last played (or started to be played) in this playlist
		/// </summary>
		internal override Song InProgressSong { get => ( SongIndex >= 0 ) ? ( PlaylistItems[ SongIndex ] as SongPlaylistItem ).Song : null; }

		/// <summary>
		/// Return a list of the songs in this playlist, optionally only the songs from the SongIndex onwards
		/// </summary>
		/// <param name="resume"></param>
		/// <returns></returns>
		internal override List<Song> GetSongsForPlayback( bool resume )
		{
			// Reset this playlist to the start if it is not being resumed
			if ( resume == false )
			{
				SongIndex = 0;

				// Report this change
				new PlaylistUpdatedMessage() { UpdatedPlaylist = this }.Send();
			}

			return PlaylistItems.GetRange( SongIndex, PlaylistItems.Count - SongIndex ).Select( item => ( item as SongPlaylistItem ).Song ).ToList();
		}

		/// <summary>
		/// Return the Song Id of the entry referenced by the SongIndex
		/// </summary>
		/// <returns></returns>
		protected override int IndexedSongIdentity( int songIndex )
		{
			if ( songIndex == -1 )
			{
				songIndex = SongIndex;
			}

			return ( songIndex == -1 ) ? -1 : ( PlaylistItems[ songIndex ] as SongPlaylistItem ).SongId;
		}

		/// <summary>
		/// Return the Song Id of the entry referenced by the next SongIndex
		/// </summary>
		/// <returns></returns>
		protected override int NextIndexedSongIdentity()
		{
			int nextId = -1;
			if ( SongIndex < ( PlaylistItems.Count - 1 ) )
			{
				nextId = ( PlaylistItems[ SongIndex + 1 ] as SongPlaylistItem ).SongId;
			}

			return nextId;
		}

		/// <summary>
		/// Set the SongIndex to point to the next song entry
		/// </summary>
		/// <returns></returns>
		protected override void IncrementSongIndex()
		{
			if ( SongIndex < ( PlaylistItems.Count - 1 ) )
			{
				SongIndex++;
			}
			else
			{
				SongIndex = -1;
			}
		}
	}
}