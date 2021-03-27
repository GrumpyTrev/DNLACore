using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The AlbumPlaylist class contains an ordered collection of songs wrapped up in SongPlaylistItems
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

				// Get the contents of this playlist as the SongIndex processing assumes that the Songs are available
				// Don't wait
				playlistItem.Album.GetSongsAsync();
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
		}

		/// <summary>
		/// The Song last played (or started to be played) in this playlist
		/// </summary>
		internal override Song InProgressSong { get => ( SongIndex >= 0 ) ? 
				( PlaylistItems[ GetGroupFromTag( SongIndex ) ] as AlbumPlaylistItem ).Album.Songs[GetChildFromTag( SongIndex ) ] : null; }

		/// <summary>
		/// The Album last played (or started to be played) in this playlist
		/// </summary>
		internal Album InProgressAlbum { get => ( SongIndex >= 0 ) ? ( PlaylistItems[ GetGroupFromTag( SongIndex ) ] as AlbumPlaylistItem ).Album : null; }

		/// <summary>
		/// Return a list of the songs in this playlist, optionally only the songs from the SongIndex onwards
		/// </summary>
		/// <param name="resume"></param>
		/// <returns></returns>
		internal override List<Song> GetSongsForPlayback( bool resume )
		{
			List<Song> songs = new List<Song>();

			// Reset this playlist to the start if it is not being resumed
			if ( resume == false )
			{
				SongIndex = 0;

				// Report this change
				new PlaylistUpdatedMessage() { UpdatedPlaylist = this }.Send();
			}

			// For the first Album only add songs from the current song onwards
			int albumIndex = GetGroupFromTag( SongIndex );
			Album album = ( PlaylistItems[ albumIndex ] as AlbumPlaylistItem ).Album;

			int songIndex = GetChildFromTag( SongIndex );
			songs.AddRange( album.Songs.GetRange( songIndex, album.Songs.Count - songIndex ) );

			// For the rest just add all the songs
			++albumIndex;
			for ( ; albumIndex < PlaylistItems.Count; ++albumIndex )
			{
				songs.AddRange( ( PlaylistItems[ albumIndex ] as AlbumPlaylistItem ).Album.Songs );
			}

			return songs;
		}

		/// <summary>
		/// Set the SongIndex to point to the next song entry
		/// </summary>
		/// <returns></returns>
		protected override void IncrementSongIndex()
		{
			if ( SongIndex == -1 )
			{
				SongIndex = 0;
			}
			else
			{
				int playlistIndex = GetGroupFromTag( SongIndex );
				int albumSongIndex = GetChildFromTag( SongIndex );

				if ( albumSongIndex < ( ( PlaylistItems[ playlistIndex ] as AlbumPlaylistItem ).Album.Songs.Count - 1 ) )
				{
					SongIndex = FormChildTag( playlistIndex, albumSongIndex + 1 );
				}
				else if ( playlistIndex < ( PlaylistItems.Count - 1 ) )
				{
					SongIndex = FormChildTag( playlistIndex + 1, 0 );
				}
				else
				{
					SongIndex = -1;
				}
			}
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

			return ( songIndex == -1 ) ? -1 :
				( PlaylistItems[ GetGroupFromTag( songIndex ) ] as AlbumPlaylistItem ).Album.Songs[ GetChildFromTag( songIndex ) ].Id;
		}

		/// <summary>
		/// Return the Song Id of the entry referenced by the next SongIndex
		/// </summary>
		/// <returns></returns>
		protected override int NextIndexedSongIdentity()
		{
			int nextSongIndex = -1;

			if ( SongIndex == -1 )
			{
				nextSongIndex = ( PlaylistItems[ 0 ] as AlbumPlaylistItem ).Album.Songs[ 0 ].Id;
			}
			else
			{
				int playlistIndex = GetGroupFromTag( SongIndex );
				int albumSongIndex = GetChildFromTag( SongIndex );

				AlbumPlaylistItem currentPlaylistItem = ( AlbumPlaylistItem )PlaylistItems[ playlistIndex ];

				if ( albumSongIndex < ( currentPlaylistItem.Album.Songs.Count - 1 ) )
				{
					nextSongIndex = currentPlaylistItem.Album.Songs[ albumSongIndex + 1 ].Id;
				}
				else if ( playlistIndex < ( PlaylistItems.Count - 1 ) )
				{
					nextSongIndex = ( PlaylistItems[ playlistIndex + 1 ] as AlbumPlaylistItem ).Album.Songs[ 0 ].Id;
				}
			}

			return nextSongIndex;
		}

		/// <summary>
		/// Form a tag for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		private static int FormChildTag( int groupPosition, int childPosition ) => ( groupPosition << 16 ) + childPosition;

		/// <summary>
		/// Return the group number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private static int GetGroupFromTag( int tag ) => tag >> 16;

		/// <summary>
		/// Return the child number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private static int GetChildFromTag( int tag ) => ( tag & 0xFFFF );
	}
}