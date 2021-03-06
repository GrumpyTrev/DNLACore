﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains SongPlaylist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	class PlaylistsController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		static PlaylistsController()
		{
			SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );
			SongStartedMessage.Register( SongStarted );
			PlaylistUpdatedMessage.Register( PlaylistUpdated );
			SongFinishedMessage.Register( SongFinished );
		}

		/// <summary>
		/// Get the SongPlaylist data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			// Delete the playlist and then refresh the data held by the model
			Playlists.DeletePlaylist( thePlaylist );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
		}

		/// <summary>
		/// Delete the specified SongPlaylistItem items from its parent playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( Playlist thePlaylist, IEnumerable< PlaylistItem > items )
		{
			// Delete the items from the playlist
			thePlaylist.DeletePlaylistItems( items );

			// Adjust the track numbers
			thePlaylist.AdjustTrackNumbers();

			// Report the change
			DataReporter?.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Add a new SongPlaylist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public static async Task<SongPlaylist> AddSongPlaylistAsync( string playlistName )
		{
			SongPlaylist newPlaylist = new SongPlaylist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId };

			await Playlists.AddPlaylistAsync( newPlaylist );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();

			return newPlaylist;
		}

		/// <summary>
		/// Add a new playlist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public static async Task<AlbumPlaylist> AddAlbumPlaylistAsync( string playlistName )
		{
			AlbumPlaylist newPlaylist = new AlbumPlaylist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId };

			await Playlists.AddPlaylistAsync( newPlaylist );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();

			return newPlaylist;
		}

		/// <summary>
		/// Change the name of the specified playlist
		/// </summary>
		/// <param name="playlistName"></param>
		public static void RenamePlaylist( Playlist playlist, string newName )
		{
			playlist.Rename( newName );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="playlist"></param>
		public static void AddSongsToPlaylist( IEnumerable<Song> songsToAdd, SongPlaylist playlist )
		{
			playlist.AddSongs( songsToAdd );

			// Report the change
			DataReporter?.PlaylistUpdated( playlist );
		}

		/// <summary>
		/// Add a list of Albums to a specified playlist
		/// </summary>
		/// <param name="albumsToAdd"></param>
		/// <param name="playlist"></param>
		public static void AddAlbumsToPlaylist( IEnumerable<Album> albumsToAdd, AlbumPlaylist playlist )
		{
			playlist.AddAlbums( albumsToAdd );

			// Report the change
			DataReporter?.PlaylistUpdated( playlist );
		}

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsDown( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsDown( items );

			DataReporter?.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsUp( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsUp( items );

			DataReporter?.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Check if the specified playlist exists in other libraries
		/// </summary>
		/// <param name="name"></param>
		/// <param name="playListLibrary"></param>
		/// <returns></returns>
		public static bool CheckForOtherPlaylists( string name, int playListLibrary ) =>
			Playlists.PlaylistCollection.Exists( list => ( list.Name == name ) && ( list.LibraryId != playListLibrary ) );

		/// <summary>
		/// Duplicate a playlist in the other libraries
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		public static async void DuplicatePlaylistAsync( Playlist playlistToDuplicate )
		{
			// Duplicate the playlist in all libraries except the one it is in
			foreach ( Library library in Libraries.LibraryCollection )
			{
				if ( library.Id != playlistToDuplicate.LibraryId )
				{
					// If a playlist with the same name already exists then delete it. This is being deleted rather than being reused just in case it
					// is the wrong type of playlist
					Playlist existingPlaylist = Playlists.PlaylistCollection
						.Where( playlist => ( playlist.Name == playlistToDuplicate.Name ) && ( playlist.LibraryId == library.Id ) ).SingleOrDefault();

					if ( existingPlaylist != null )
					{
						Playlists.DeletePlaylist( existingPlaylist );
					}

					if ( playlistToDuplicate is SongPlaylist )
					{
						DuplicateSongPlaylistAsync( ( SongPlaylist )playlistToDuplicate, library.Id );
					}
					else
					{
						DuplicateAlbumPlaylistAsync( ( AlbumPlaylist )playlistToDuplicate, library.Id );
					}
				}
			}
		}

		/// <summary>
		/// Duplicate the SongPlaylist in the specified library
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		/// <returns></returns>
		private static async void DuplicateSongPlaylistAsync( SongPlaylist playlistToDuplicate, int libraryId )
		{
			// Now create a new playlist in the library with the same name
			SongPlaylist duplicatedPlaylist = new SongPlaylist() { Name = playlistToDuplicate.Name, LibraryId = libraryId };
			await Playlists.AddPlaylistAsync( duplicatedPlaylist );

			// Attempt to find matching songs for each SongPlaylistItem in the SongPlaylist
			// Need to access the songs via the Sources associated with the Library
			List<Source> sources = Sources.GetSourcesForLibrary( libraryId );

			// Keep track of the matching songs
			List<Song> songsToAdd = new List<Song>();

			foreach ( SongPlaylistItem item in playlistToDuplicate.PlaylistItems )
			{
				Song matchingSong = null;
				int sourceIndex = 0;

				while ( ( matchingSong == null ) && ( sourceIndex < sources.Count ) )
				{
					// Get a list of all the songs with matching Titles in the source
					List<Song> matchingTitles = await DbAccess.GetMatchingSongAsync( item.Song.Title, sources[ sourceIndex++ ].Id );

					// Now for each song access the associated artist
					int titleIndex = 0;
					while ( ( matchingSong == null ) && ( titleIndex < matchingTitles.Count ) )
					{
						Artist nameCheck = Artists.GetArtistById(
							ArtistAlbums.GetArtistAlbumById( matchingTitles[ titleIndex ].ArtistAlbumId ).ArtistId );

						// Correct name?
						if ( nameCheck.Name == item.Artist.Name )
						{
							matchingSong = matchingTitles[ titleIndex ];
							songsToAdd.Add( matchingSong );

							// Make sure that the Artist is stored with the song
							matchingSong.Artist = nameCheck;
						}

						titleIndex++;
					}
				}
			}

			if ( songsToAdd.Count > 0 )
			{
				// Add the songs to the new SongPlaylist.
				duplicatedPlaylist.AddSongs( songsToAdd );
			}
		}

		/// <summary>
		/// Duplicate the AlbumPlaylist in the specified library
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		/// <param name="libararyId"></param>
		private static async void DuplicateAlbumPlaylistAsync( AlbumPlaylist playlistToDuplicate, int libraryId )
		{
			// Now create a new playlist in the library with the same name
			AlbumPlaylist duplicatedPlaylist = new AlbumPlaylist() { Name = playlistToDuplicate.Name, LibraryId = libraryId };
			await Playlists.AddPlaylistAsync( duplicatedPlaylist );

			List<Album> albumsToAdd = new List<Album>();
			foreach ( AlbumPlaylistItem item in playlistToDuplicate.PlaylistItems )
			{
				// Find a matching Album name with the same Artist name
				Album matchingAlbum = Albums.AlbumCollection.Where( album => ( album.LibraryId == libraryId ) && ( album.Name == item.Album.Name ) 
					&& ( album.ArtistName == item.Album.ArtistName ) ).FirstOrDefault();
				if ( matchingAlbum != null )
				{
					albumsToAdd.Add( matchingAlbum );
				}
			}

			if ( albumsToAdd.Count > 0 )
			{
				// Add the songs to the new SongPlaylist.
				duplicatedPlaylist.AddAlbums( albumsToAdd );
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static async void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			PlaylistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the Playlists and playlist names. Make sure a copy of the list is used as we're going to sort it 
			PlaylistsViewModel.Playlists = Playlists.GetPlaylistsForLibrary( PlaylistsViewModel.LibraryId ).ToList();
			PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();

			// To generate the data to be displayed the Playlists need to be sorted. Not a simple sort of course, but the SongPlaylists followed by the 
			// AlbumPlaylists
			await Task.Run( () =>
			{
				PlaylistsViewModel.AlbumPlaylists.Clear();
				PlaylistsViewModel.SongPlaylists.Clear();

				foreach ( Playlist playlist in PlaylistsViewModel.Playlists )
				{
					if ( playlist is SongPlaylist songPlaylist )
					{
						PlaylistsViewModel.SongPlaylists.Add( songPlaylist );
					}
					else
					{
						PlaylistsViewModel.AlbumPlaylists.Add( ( AlbumPlaylist )playlist );
					}
				}

				// Sort the playlists by name
				PlaylistsViewModel.SongPlaylists.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );
				PlaylistsViewModel.AlbumPlaylists.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );

				// Now copy to the combined list
				PlaylistsViewModel.Playlists.Clear();
				PlaylistsViewModel.Playlists.AddRange( PlaylistsViewModel.SongPlaylists );
				PlaylistsViewModel.Playlists.AddRange( PlaylistsViewModel.AlbumPlaylists );
			} );

			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data then reload
		/// </summary>
		/// <param name="_"></param>
		private static void SelectedLibraryChanged( int _ )
		{
			// Clear the displayed data
			PlaylistsViewModel.ClearModel();

			// Reread the data
			StorageDataAvailable();
		}

		/// <summary>
		/// Called when the SongStartedMessage has been received
		/// </summary>
		/// <param name="message"></param>
		private static void SongStarted( Song songStarted )
		{
			// Update the song index for any playlists for which the previous song and the current song are adjacent 
			Playlists.CheckForAdjacentSongEntries( previousSongIdentity, songStarted.Id );

			previousSongIdentity = songStarted.Id;
		}

		/// <summary>
		/// Called when the SongFinishedMessage has been received
		/// </summary>
		/// <param name="songPlayed"></param>
		private static void SongFinished( Song songPlayed ) => Playlists.SongFinished( songPlayed.Id );

		/// <summary>
		/// Called when a PlaylistUpdatedMessage has been received. Pass it on to the reporter
		/// </summary>
		/// <param name="message"></param>
		private static void PlaylistUpdated( Playlist updatedPlaylist ) => DataReporter?.PlaylistUpdated( updatedPlaylist );

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IPlaylistsReporter DataReporter
		{
			get => ( IPlaylistsReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IPlaylistsReporter : DataReporter.IReporter
		{
			void PlaylistUpdated( Playlist playlist );
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );

		/// <summary>
		/// The previous song id that has been played
		/// </summary>
		private static int previousSongIdentity = -1;
	}
}