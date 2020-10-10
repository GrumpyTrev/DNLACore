using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Playlists class holds a collection of all the Playlist entries read from storage.
	/// It allows access to Playlist entries and automatically persists changes back to storage
	/// </summary>	
	static class Playlists
	{
		/// <summary>
		/// Get the Playlists collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( PlaylistCollection == null )
			{
				// Get the current set of albums and form the lookup tables
				PlaylistCollection = await PlaylistAccess.GetAllPlaylistsAsync();
			}
		}

		/// <summary>
		/// Get the user defined playlists for the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static List<Playlist> GetPlaylistsForLibrary( int libraryId ) =>
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && 
				( play.Name != NowPlayingController.NowPlayingPlaylistName ) ).ToList();

		/// <summary>
		/// Get the PlaylistItems and associated songs for the specified playlist
		/// </summary>
		/// <param name="playlist"></param>
		public static async Task GetPlaylistContentsAsync( Playlist playlist )
		{
			if ( playlist.PlaylistItems == null )
			{
				// Get the children PlaylistItems and then the Song entries for each of them
				await PlaylistAccess.GetPlaylistItems( playlist );

				// Keep track of the last accessed ArtistAlbumId and Artist
				int lastArtistAlbumId = -1;
				Artist lastArtist = null;

				foreach ( PlaylistItem playList in playlist.PlaylistItems )
				{
					playList.Song = await SongAccess.GetSongAsync( playList.SongId );

					playList.Artist = ( playList.Song.ArtistAlbumId == lastArtistAlbumId ) ? lastArtist
						: Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( playList.Song.ArtistAlbumId ).ArtistId );
						
					// Save these in case they are required next
					lastArtistAlbumId = playList.Song.ArtistAlbumId;
					lastArtist = playList.Artist;

					// Now that the Artist is available save it in the Song
					playList.Song.Artist = playList.Artist;
				}
			}
		}

		/// <summary>
		/// Delete the specified Playlist from the collections and from the storage
		/// </summary>
		/// <param name="playlistToDelete"></param>
		public static void DeletePlaylist( Playlist playlistToDelete )
		{
			PlaylistCollection.Remove( playlistToDelete );
			PlaylistAccess.DeletePlaylist( playlistToDelete );
		}

		/// <summary>
		/// Delete the specified PlayListItems fromthe Playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( Playlist thePlaylist, List<PlaylistItem> items )
		{
			items.ForEach( item => thePlaylist.PlaylistItems.Remove( item ) );
			PlaylistAccess.DeletePlaylistItems( items );
		}

		/// <summary>
		/// Add a playlist to the local and storage collections.
		/// No need to wait for the Playlist to be added to the storage as it's Id is not accessed
		/// straight away
		/// </summary>
		/// <param name="playlistToAdd"></param>
		public static void AddPlaylist( Playlist playlistToAdd )
		{
			PlaylistCollection.Add( playlistToAdd );
			PlaylistAccess.AddPlaylist( playlistToAdd );
		}

		/// <summary>
		/// Add a list of songs to the specified playlist
		/// </summary>
		/// <param name="playlist"></param>
		/// <param name="songs"></param>
		public static async void AddSongsToPlaylistAsync( Playlist playlist, List<Song> songs )
		{
			// First of all make sure that all of the playlist is in memeory
			await GetPlaylistContentsAsync( playlist );

			// For each song create a PlayListItem and add to the PlayList
			foreach ( Song song in songs )
			{
				PlaylistItem itemToAdd = new PlaylistItem() { Artist = song.Artist, 
					PlaylistId = playlist.Id, Song = song, SongId = song.Id, 
					Track = playlist.PlaylistItems.Count + 1 };

				playlist.PlaylistItems.Add( itemToAdd );

				PlaylistAccess.AddPlaylistItem( itemToAdd );
			}
		}

		/// <summary>
		/// The set of Playlists currently held in storage
		/// </summary>
		public static List<Playlist> PlaylistCollection { get; set; } = null;
	}
}