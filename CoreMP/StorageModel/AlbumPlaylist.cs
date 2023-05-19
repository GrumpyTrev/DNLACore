using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The AlbumPlaylist class contains an ordered collection of songs wrapped up in SongPlaylistItems
	/// </summary>
	[Table( "AlbumPlayList" )]
	public class AlbumPlaylist : Playlist
	{
		/// <summary>
		/// Get the PlaylistItems and associated songs for this playlist
		/// </summary>
		/// <param name="playlistItems"></param>
		public void GetContents( IEnumerable<PlaylistItem> playlistItems )
		{
			// Get all the AlbumPlaylistItem entries associated with this AlbumPlaylist and then the Album entries for each of them
			List<PlaylistItem> possiblePlaylistItems = playlistItems.Where( item => item.PlaylistId == Id ).ToList();

			// Keep track of any PlaylistItems with no contents
			List<PlaylistItem> orphanPlaylistItems = new List<PlaylistItem>();

			foreach ( AlbumPlaylistItem playlistItem in possiblePlaylistItems )
			{
				playlistItem.Album = Albums.GetAlbumById( playlistItem.AlbumId );

				// If the album is not found then don't add it to the AlbumPlaylist
				if ( playlistItem.Album != null )
				{
					// If this item is empty then don't add it to the AlbumPlaylist
					if ( playlistItem.Album.Songs.Count == 0 )
					{
						orphanPlaylistItems.Add( playlistItem );
					}
					else
					{
						PlaylistItems.Add( playlistItem );
					}
				}
				else
				{
					orphanPlaylistItems.Add( playlistItem );
				}
			}

			PlaylistItems.Sort( ( a, b ) => a.Index.CompareTo( b.Index ) );

			DbAccess.DeleteItems( orphanPlaylistItems );
		}

		/// <summary>
		/// Add a list of albums to the playlist
		/// </summary>
		/// <param name="albums"></param>
		public void AddAlbums( IEnumerable<Album> albums )
		{
			// For each song create an AlbumPlayListItem and add to the PlayList
			List<AlbumPlaylistItem> albumPlaylistItems = new List<AlbumPlaylistItem>();

			foreach ( Album album in albums )
			{
				AlbumPlaylistItem itemToAdd = new AlbumPlaylistItem()
				{
					Album = album,
					PlaylistId = Id,
					AlbumId = album.Id,
					Index = PlaylistItems.Count
				};

				PlaylistItems.Add( itemToAdd );
				albumPlaylistItems.Add( itemToAdd );
			}

			DbAccess.InsertAll( albumPlaylistItems );
		}

		/// <summary>
		/// Delete any albums in this playlist that are contained in the supplied collection
		/// </summary>
		/// <param name="albumIds"></param>
		public void DeleteMatchingAlbums( HashSet<int> albumIds )
		{
			List<AlbumPlaylistItem> matchingItems = PlaylistItems.Where( item => albumIds.Contains( ( ( AlbumPlaylistItem )item ).AlbumId ) == true ).Cast<AlbumPlaylistItem>().ToList();

			if ( matchingItems.Count > 0 )
			{
				// Remove the AlbumPlaylistItems from the collection and database
				foreach ( AlbumPlaylistItem item in matchingItems )
				{
					PlaylistItems.Remove( item );
					DbAccess.DeleteAsync( item );
				}

				// Reindex the existing items and reset the index
				int itemIndex = 0;
				foreach ( AlbumPlaylistItem playlistItem in PlaylistItems )
				{
					if ( playlistItem.Index != itemIndex )
					{
						playlistItem.Index = itemIndex;

						// No need to wait for this 
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
						DbAccess.UpdateAsync( playlistItem );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					}

					itemIndex++;
				}

				// As some items have been removed reset the song index
				SongIndex = 0;
			}
		}


		/// <summary>
		/// The Song last played (or started to be played) in this playlist
		/// </summary>
		public override Song InProgressSong => ( SongIndex >= 0 ) ?
				( PlaylistItems[ GetGroupFromTag( SongIndex ) ] as AlbumPlaylistItem ).Album.Songs[ GetChildFromTag( SongIndex ) ] : null;

		/// <summary>
		/// The Album last played (or started to be played) in this playlist
		/// </summary>
		public Album InProgressAlbum => ( SongIndex >= 0 ) ? ( PlaylistItems[ GetGroupFromTag( SongIndex ) ] as AlbumPlaylistItem ).Album : null;

		/// <summary>
		/// The index of the last played song in the collection of all songs
		/// </summary>
		internal override int InProgressIndex
		{
			get
			{
				int index = 0;

				if ( SongIndex >= 0 )
				{
					int groupIndex = 0;
					while ( groupIndex < GetGroupFromTag( SongIndex ) )
					{
						index += ( PlaylistItems[ groupIndex++ ] as AlbumPlaylistItem ).Album.Songs.Count;
					}

					index += GetChildFromTag( SongIndex );
				}

				return index;
			}
		}

		/// <summary>
		/// Return a list of the songs in this playlist, optionally only the songs from the SongIndex onwards
		/// </summary>
		/// <param name="resume"></param>
		/// <returns></returns>
		internal override List<Song> GetSongsForPlayback( bool resume )
		{
			List<Song> songs = new List<Song>();

			int startingIndex = ( resume == true ) ? SongIndex : 0;

			int albumIndex = 0;
			foreach ( AlbumPlaylistItem albumPlaylistItem in PlaylistItems )
			{
				// Only add songs to the list if the correct album has been reached
				if ( albumIndex >= GetGroupFromTag( startingIndex ) )
				{
					// If this is the album containing the starting index then only select a subset of the songs
					if ( albumIndex == GetGroupFromTag( startingIndex ) )
					{
						int songIndex = GetChildFromTag( startingIndex );
						songs.AddRange( albumPlaylistItem.Album.Songs.GetRange( songIndex, albumPlaylistItem.Album.Songs.Count - songIndex ) );
					}
					else
					{
						// Add all the songs from this album
						songs.AddRange( albumPlaylistItem.Album.Songs );
					}
				}

				albumIndex++;
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
