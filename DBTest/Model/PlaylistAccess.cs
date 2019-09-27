using SQLite;
using System.Collections.Generic;
using SQLiteNetExtensionsAsync.Extensions;
using SQLiteNetExtensions.Extensions;
using System.Threading.Tasks;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The PlaylistAccess class is used to access and change Playlist data via the database
	/// </summary>
	class PlaylistAccess
	{
		/// <summary>
		/// Get all the playlists associated with the library 
		/// </summary>
		public static async Task< List< Playlist > > GetPlaylistDetailsAsync( string databasePath, int libraryId )
		{
			SQLiteAsyncConnection dbAsynch = new SQLiteAsyncConnection( databasePath );

			// Get all the playlist except the Now Playing list
			AsyncTableQuery<Playlist> query = dbAsynch.Table<Playlist>().
				Where( d => ( ( d.LibraryId == libraryId ) ) && ( d.Name != NowPlayingController.NowPlayingPlaylistName ) );

			return await query.ToListAsync();
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task< Playlist > GetNowPlayingListAsync( string databasePath, int libraryId )
		{
			SQLiteAsyncConnection dbAsynch = new SQLiteAsyncConnection( databasePath );

			// Get the Now Playing list
			Playlist thePlaylist = await dbAsynch.Table<Playlist>().
				Where( d => ( ( d.LibraryId == libraryId ) ) && ( d.Name == NowPlayingController.NowPlayingPlaylistName ) ).FirstAsync();

			// Get the contents
			await GetPlaylistContentsAsync( thePlaylist, databasePath );

			return thePlaylist;
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task GetPlaylistContentsAsync( Playlist thePlaylist, string databasePath )
		{
			SQLiteAsyncConnection dbAsynch = new SQLiteAsyncConnection( databasePath );

			// Get the children PlaylistItems and then the Song entries for each of them
			await dbAsynch.GetChildrenAsync<Playlist>( thePlaylist );

			foreach ( PlaylistItem playList in thePlaylist.PlaylistItems )
			{
				await dbAsynch.GetChildrenAsync<PlaylistItem>( playList );
			}
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static void GetPlaylistContents( Playlist thePlaylist, string databasePath )
		{
			using ( SQLiteConnection db = new SQLiteConnection( databasePath ) )
			{
				// Get the children PlaylistItems and then the Song entries for each of them
				db.GetChildren( thePlaylist );

				foreach ( PlaylistItem playList in thePlaylist.PlaylistItems )
				{
					db.GetChildren( playList );
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="list"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, Playlist selectedPlaylist, string databasePath )
		{
			using ( SQLiteConnection db = new SQLiteConnection( databasePath ) )
			{
				// Get the full contents of the playlist and add PlaylistItem entries to it
				db.GetChildren( selectedPlaylist );

				foreach ( Song songToAdd in songsToAdd )
				{
					PlaylistItem listItem = new PlaylistItem() {
						PlaylistId = selectedPlaylist.Id, SongId = songToAdd.Id,
						Track = selectedPlaylist.PlaylistItems.Count + 1
					};

					db.Insert( listItem );
					selectedPlaylist.PlaylistItems.Add( listItem );
				}

				db.UpdateWithChildren( selectedPlaylist );
			}
		}

		/// <summary>
		/// Clear the Now Playing list
		/// </summary>
		/// <param name="databasePath"></param>
		/// <param name="libraryId"></param>
		public static void ClearNowPlayingList( string databasePath, int libraryId )
		{
			using ( SQLiteConnection db = new SQLiteConnection( databasePath ) )
			{
				Playlist nowPlayingList = db.Table<Playlist>().
					Where( list => ( list.Name == NowPlayingController.NowPlayingPlaylistName ) && ( list.LibraryId == libraryId ) ).SingleOrDefault();

				if ( nowPlayingList != null )
				{
					// Make sure the list of PlaylistItems is read in
					db.GetChildren( nowPlayingList );

					// Delete the items from the database
					foreach ( PlaylistItem item in nowPlayingList.PlaylistItems )
					{
						db.Delete( item );
					}
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToNowPlayingList( List<Song> songsToAdd, string databasePath, int libraryId )
		{
			// Get the Now Playing playlist from the database
			Playlist nowPlayingList = null;
			using ( SQLiteConnection db = new SQLiteConnection( databasePath ) )
			{
				nowPlayingList = db.Table<Playlist>().
					Where( list => ( list.Name == NowPlayingController.NowPlayingPlaylistName ) && ( list.LibraryId == libraryId ) ).SingleOrDefault();
			}

			// Carry out the common processing to add songs to a playlist
			if ( nowPlayingList != null )
			{
				AddSongsToPlaylist( songsToAdd, nowPlayingList, databasePath );
			}
		}
	}
}