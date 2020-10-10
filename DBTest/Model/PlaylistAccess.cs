﻿using System.Collections.Generic;
using SQLiteNetExtensionsAsync.Extensions;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistAccess class is used to access and change Playlist data via the database
	/// </summary>
	class PlaylistAccess
	{
		/// <summary>
		/// Get all the playlists except the Now Playing list
		/// </summary>
		public static async Task<List<Playlist>> GetPlaylistDetailsAsync( int libraryId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( d => ( ( d.LibraryId == libraryId ) ) && ( d.Name != NowPlayingController.NowPlayingPlaylistName ) ).ToListAsync();

		/// <summary>
		/// Get all the playlists in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Playlist>> GetAllPlaylistsAsync() =>
			await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().ToListAsync();

		/// <summary>
		/// Get the PlayListItem entries associated with the specified Playlist
		/// </summary>
		/// <param name="playlist"></param>
		public static async Task GetPlaylistItems( Playlist playlist ) =>
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( playlist );

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task<Playlist> GetNowPlayingListAsync( int libraryId, bool withArtists = false )
		{
			// Get the Now Playing list
			Playlist thePlaylist = await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( d => ( d.LibraryId == libraryId ) && ( d.Name == NowPlayingController.NowPlayingPlaylistName ) ).FirstOrDefaultAsync();

			if ( thePlaylist == null )
			{
				// If there is no NowPlaying list create one
				thePlaylist = new Playlist() { Name = NowPlayingController.NowPlayingPlaylistName, LibraryId = libraryId };
				await ConnectionDetailsModel.AsynchConnection.InsertAsync( thePlaylist );
			}

			// Get the children PlaylistItems and then the Song entries for each of them
			await GetPlaylistContentsWithArtistsAsync( thePlaylist, withArtists );

			return thePlaylist;
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task GetPlaylistContentsWithArtistsAsync( Playlist thePlaylist, bool withArtists = true )
		{
			// Get the children PlaylistItems and then the Song entries for each of them
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( thePlaylist );

			// Keep track of the last accessed ArtistAlbumId and Artist
			int lastArtistAlbumId = -1;
			Artist lastArtist = null;

			foreach ( PlaylistItem playList in thePlaylist.PlaylistItems )
			{
				playList.Song = await ConnectionDetailsModel.AsynchConnection.GetAsync<Song>( playList.SongId );

				if ( withArtists == true )
				{
					// Now the Song entries are available get the Artist via the ArtistAlbum 
					if ( playList.Song != null )
					{
						if ( playList.Song.ArtistAlbumId == lastArtistAlbumId )
						{
							playList.Artist = lastArtist;
						}
						else
						{
							ArtistAlbum artistAlbum = await ConnectionDetailsModel.AsynchConnection.GetAsync<ArtistAlbum>( playList.Song.ArtistAlbumId );
							playList.Artist = await ConnectionDetailsModel.AsynchConnection.GetAsync<Artist>( artistAlbum.ArtistId );

							// Save these in case they are required next
							lastArtistAlbumId = playList.Song.ArtistAlbumId;
							lastArtist = playList.Artist;
						}

						// Now that the Artist is available save it in the Song
						playList.Song.Artist = playList.Artist;
					}
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="list"></param>
		public static async Task AddSongsToPlaylistAsync( List<Song> songsToAdd, string selectedPlaylist, int libraryId )
		{
			Playlist selectedList = await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( d => ( d.LibraryId == libraryId ) && ( d.Name == selectedPlaylist ) ).FirstOrDefaultAsync();

			if ( selectedList != null )
			{
				// Get the full contents of the playlist and add PlaylistItem entries to it
				await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( selectedList );

				foreach ( Song songToAdd in songsToAdd )
				{
					PlaylistItem listItem = new PlaylistItem() {
						PlaylistId = selectedList.Id, SongId = songToAdd.Id,
						Track = selectedList.PlaylistItems.Count + 1
					};

					await ConnectionDetailsModel.AsynchConnection.InsertAsync( listItem );
					selectedList.PlaylistItems.Add( listItem );
				}

				await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( selectedList );
			}
		}

		/// <summary>
		/// Clear the Now Playing list
		/// </summary>
		/// <param name="libraryId"></param>
		public static async Task ClearNowPlayingListAsync( int libraryId )
		{
			Playlist nowPlayingList = await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( list => ( list.Name == NowPlayingController.NowPlayingPlaylistName ) && ( list.LibraryId == libraryId ) ).FirstOrDefaultAsync();

			if ( nowPlayingList != null )
			{
				// Get the full contents of the playlist and add PlaylistItem entries to it
				await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( nowPlayingList );

				// Delete the items from the database
				foreach ( PlaylistItem item in nowPlayingList.PlaylistItems )
				{
					await ConnectionDetailsModel.AsynchConnection.DeleteAsync( item );
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static async Task AddSongsToNowPlayingListAsync( List<Song> songsToAdd, int libraryId ) =>
			await AddSongsToPlaylistAsync( songsToAdd, NowPlayingController.NowPlayingPlaylistName, libraryId );

		/// <summary>
		/// Delete the specified playlist from the database
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <returns></returns>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			if ( thePlaylist.PlaylistItems != null )
			{
				// Delete the PlaylistItem entries from the database.
				// No need to wait for this to finish
				thePlaylist.PlaylistItems.ForEach( item => ConnectionDetailsModel.AsynchConnection.DeleteAsync( item ) );
			}

			// Now delete the playlist itself. No need to wait for this to finish
			ConnectionDetailsModel.AsynchConnection.DeleteAsync( thePlaylist );
		}

		/// <summary>
		/// Delete the specified PlaylistItem items
		/// </summary>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( List<PlaylistItem> items ) =>
			items.ForEach( item => ConnectionDetailsModel.AsynchConnection.DeleteAsync( item ) );

		/// <summary>
		/// Add the specified playlist to the database
		/// </summary>
		/// <param name="playlistName"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static void AddPlaylist( Playlist playlistToAdd ) => 
			ConnectionDetailsModel.AsynchConnection.InsertAsync( playlistToAdd );

		/// <summary>
		/// Update a modified PlaylistItem
		/// </summary>
		/// <param name="itemToUpdate"></param>
		/// <returns></returns>
		public static async void UpdatePlaylistItemAsync( PlaylistItem itemToUpdate ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( itemToUpdate );

		/// <summary>
		/// Add a new PlaylistItem to the database. No need to wait for this to complete
		/// </summary>
		/// <param name="itemToAdd"></param>
		public static void AddPlaylistItem( PlaylistItem itemToAdd ) =>
			ConnectionDetailsModel.AsynchConnection.InsertAsync( itemToAdd );

		/// <summary>
		/// Get a list of all the playlists in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Playlist>> GetAllPlaylists() => await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().ToListAsync();

		/// <summary>
		/// Delete any Playlist items associated with the list of songs
		/// </summary>
		/// <param name="songIds"></param>
		/// <returns></returns>
		public static async void DeletePlaylistItemsAsync( List< int > songIds ) => await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync(
			await ConnectionDetailsModel.AsynchConnection.Table<PlaylistItem>().Where( item => songIds.Contains( item.SongId ) ).ToListAsync() );
	}
};