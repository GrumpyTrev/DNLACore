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
				// Get the current set of Playlists
				PlaylistCollection = await PlaylistAccess.GetAllPlaylistsAsync();

				// Get all the content for the playlists
				await Task.Run( async () =>
				{
					foreach ( Playlist playlist in PlaylistCollection )
					{
						await playlist.GetContentsAsync();
					}
				} );
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
		/// Get the Now Playing playlist for the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetNowPlayingPlaylist( int libraryId ) =>
			GetPlaylist( NowPlayingController.NowPlayingPlaylistName, libraryId );

		/// <summary>
		/// Get a playlist given its name and library
		/// </summary>
		/// <param name="name"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetPlaylist( string name, int libraryId ) =>
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && ( play.Name == name ) ).FirstOrDefault();

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
		/// Delete any PlaylistItem objects associated with the list of songs
		/// </summary>
		/// <param name="songIds"></param>
		/// <returns></returns>
		public static void DeletePlaylistItems( List<int> songIds )
		{
			foreach ( Playlist playlist in PlaylistCollection )
			{
				playlist.DeletePlaylistItems( playlist.PlaylistItems.Where( item => songIds.Contains( item.SongId ) == true ).ToList() );
			}
		}

		/// <summary>
		/// The set of Playlists currently held in storage
		/// </summary>
		public static List<Playlist> PlaylistCollection { get; set; } = null;
	}

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
		public async Task GetContentsAsync()
		{
			if ( PlaylistItems == null )
			{
				// Get the children PlaylistItems and then the Song entries for each of them
				await PlaylistAccess.GetPlaylistItems( this );

				foreach ( PlaylistItem playlistItem in PlaylistItems )
				{
					playlistItem.Song = await SongAccess.GetSongAsync( playlistItem.SongId );
					playlistItem.Artist = Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( playlistItem.Song.ArtistAlbumId ).ArtistId );
					playlistItem.Song.Artist = playlistItem.Artist;
				}

				PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}
		}

		/// <summary>
		/// Delete the specified PlayListItems from the Playlist
		/// </summary>
		/// <param name="items"></param>
		public void DeletePlaylistItems( List<PlaylistItem> items )
		{
			items.ForEach( item => PlaylistItems.Remove( item ) );
			PlaylistAccess.DeletePlaylistItems( items );
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
		public void AddSongs( List<Song> songs )
		{
			// For each song create a PlayListItem and add to the PlayList
			foreach ( Song song in songs )
			{
				PlaylistItem itemToAdd = new PlaylistItem()
				{
					Artist = song.Artist,
					PlaylistId = Id,
					Song = song,
					SongId = song.Id,
					Track = PlaylistItems.Count + 1
				};

				PlaylistItems.Add( itemToAdd );
				PlaylistAccess.AddPlaylistItem( itemToAdd );
			}
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
					PlaylistAccess.UpdatePlaylistItemAsync( itemToCheck );
				}
			}
		}
	}
}