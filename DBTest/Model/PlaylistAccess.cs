﻿using SQLite;
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
		public static async Task< List< Playlist > > GetPlaylistDetailsAsync( int libraryId )
		{
			// Get all the playlist except the Now Playing list
			AsyncTableQuery<Playlist> query = ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( d => ( ( d.LibraryId == libraryId ) ) && ( d.Name != NowPlayingController.NowPlayingPlaylistName ) );

			return await query.ToListAsync();
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task< Playlist > GetNowPlayingListAsync( int libraryId )
		{
			// Get the Now Playing list
			Playlist thePlaylist = await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().
				Where( d => ( ( d.LibraryId == libraryId ) ) && ( d.Name == NowPlayingController.NowPlayingPlaylistName ) ).FirstAsync();

			// Get the contents
			await GetPlaylistContentsAsync( thePlaylist );

			return thePlaylist;
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static async Task GetPlaylistContentsAsync( Playlist thePlaylist )
		{
			// Get the children PlaylistItems and then the Song entries for each of them
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync<Playlist>( thePlaylist );

			foreach ( PlaylistItem playList in thePlaylist.PlaylistItems )
			{
				await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync<PlaylistItem>( playList );
			}
		}

		/// <summary>
		/// Get the songs in the Now Playing playlist associated with the library 
		/// </summary>
		public static void GetPlaylistContents( Playlist thePlaylist )
		{
			// Get the children PlaylistItems and then the Song entries for each of them
			ConnectionDetailsModel.SynchConnection.GetChildren( thePlaylist );

			foreach ( PlaylistItem playList in thePlaylist.PlaylistItems )
			{
				ConnectionDetailsModel.SynchConnection.GetChildren( playList );
			}
		}


		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="list"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, string selectedPlaylist, int libraryId )
		{
			Playlist selectedList = ConnectionDetailsModel.SynchConnection.Table<Playlist>().
				Where( list => ( list.Name == selectedPlaylist ) && ( list.LibraryId == libraryId ) ).SingleOrDefault();

			if ( selectedList != null )
			{
				// Get the full contents of the playlist and add PlaylistItem entries to it
				ConnectionDetailsModel.SynchConnection.GetChildren( selectedList );

				foreach ( Song songToAdd in songsToAdd )
				{
					PlaylistItem listItem = new PlaylistItem() {
						PlaylistId = selectedList.Id, SongId = songToAdd.Id,
						Track = selectedList.PlaylistItems.Count + 1
					};

					ConnectionDetailsModel.SynchConnection.Insert( listItem );
					selectedList.PlaylistItems.Add( listItem );
				}

				ConnectionDetailsModel.SynchConnection.UpdateWithChildren( selectedList );
			}
		}

		/// <summary>
		/// Clear the Now Playing list
		/// </summary>
		/// <param name="databasePath"></param>
		/// <param name="libraryId"></param>
		public static void ClearNowPlayingList( int libraryId )
		{
			Playlist nowPlayingList = ConnectionDetailsModel.SynchConnection.Table<Playlist>().
				Where( list => ( list.Name == NowPlayingController.NowPlayingPlaylistName ) && ( list.LibraryId == libraryId ) ).SingleOrDefault();

			if ( nowPlayingList != null )
			{
				// Make sure the list of PlaylistItems is read in
				ConnectionDetailsModel.SynchConnection.GetChildren( nowPlayingList );

				// Delete the items from the database
				foreach ( PlaylistItem item in nowPlayingList.PlaylistItems )
				{
					ConnectionDetailsModel.SynchConnection.Delete( item );
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToNowPlayingList( List<Song> songsToAdd, int libraryId )
		{
			AddSongsToPlaylist( songsToAdd, NowPlayingController.NowPlayingPlaylistName, libraryId );
		}
	}
}