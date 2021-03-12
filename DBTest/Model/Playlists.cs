﻿using System.Collections.Generic;
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
				PlaylistCollection = await DbAccess.LoadAsync<Playlist>();

				// Get all the content for the playlists
				await Task.Run( async () =>
				{
					// Get all the PlaylistItems
					List<PlaylistItem> playlistItems = await DbAccess.LoadAsync<PlaylistItem>();

					foreach ( Playlist playlist in PlaylistCollection )
					{
						await playlist.GetContentsAsync( playlistItems );
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
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && ( play.Name != NowPlayingController.NowPlayingPlaylistName ) ).ToList();

		/// <summary>
		/// Get the Now Playing playlist for the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetNowPlayingPlaylist( int libraryId ) => GetPlaylist( NowPlayingController.NowPlayingPlaylistName, libraryId );

		/// <summary>
		/// Get a playlist given its name and library
		/// </summary>
		/// <param name="name"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetPlaylist( string name, int libraryId ) =>
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && ( play.Name == name ) ).FirstOrDefault();

		/// <summary>
		/// Get a pl;aylist givent its identity
		/// </summary>
		/// <param name="playlistId"></param>
		/// <returns></returns>
		public static Playlist GetPlaylist( int playlistId ) => PlaylistCollection.Where( play => play.Id == playlistId ).FirstOrDefault();

		/// <summary>
		/// Delete the specified Playlist from the collections and from the storage
		/// </summary>
		/// <param name="playlistToDelete"></param>
		public static void DeletePlaylist( Playlist playlistToDelete )
		{
			PlaylistCollection.Remove( playlistToDelete );

			// Delete the PlaylistItem entries from the database.
			// No need to wait for this to finish
			DbAccess.DeleteItemsAsync( playlistToDelete.PlaylistItems );

			// Now delete the playlist itself. No need to wait for this to finish
			DbAccess.DeleteAsync( playlistToDelete );
		}

		/// <summary>
		/// Add a playlist to the local and storage collections.
		/// Wait for the storage to complete in case the called requires access to the stored Id
		/// </summary>
		/// <param name="playlistToAdd"></param>
		public static async Task AddPlaylistAsync( Playlist playlistToAdd )
		{
			PlaylistCollection.Add( playlistToAdd );
			await DbAccess.InsertAsync( playlistToAdd );
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
		/// Access the NowPlaying playlist's SongIndex
		/// </summary>
		public static int CurrentSong
		{
			get => GetNowPlayingPlaylist( ConnectionDetailsModel.LibraryId ).SongIndex;

			set
			{
				GetNowPlayingPlaylist( ConnectionDetailsModel.LibraryId ).SongIndex = value;

				// Inform controllers about this
				new SongSelectedMessage().Send();
			}
		}

		/// <summary>
		/// The set of Playlists currently held in storage
		/// </summary>
		public static List<Playlist> PlaylistCollection { get; set; } = null;
	}
}