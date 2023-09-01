using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains SongPlaylist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	internal class PlaylistsController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		public PlaylistsController()
		{
			NotificationHandler.Register( typeof( StorageController ), () =>
			{
				StorageDataAvailable();

				SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );
				SongStartedMessage.Register( SongStarted );
				PlaylistUpdatedMessage.Register( PlaylistUpdated );
				SongFinishedMessage.Register( SongFinished );
			} );
		}

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public void DeletePlaylist( Playlist thePlaylist )
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
		public void DeletePlaylistItems( Playlist thePlaylist, IEnumerable< PlaylistItem > items )
		{
			// Delete the items from the playlist
			thePlaylist.DeletePlaylistItems( items.ToList() );

			// Adjust the track numbers
			thePlaylist.AdjustTrackNumbers();

			// Report the change
			PlaylistsViewModel.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Add a new SongPlaylist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public async Task<SongPlaylist> AddSongPlaylistAsync( string playlistName )
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
		public async Task<AlbumPlaylist> AddAlbumPlaylistAsync( string playlistName )
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
		public void RenamePlaylist( Playlist playlist, string newName )
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
		public void AddSongsToPlaylist( IEnumerable<Song> songsToAdd, SongPlaylist playlist )
		{
			playlist.AddSongs( songsToAdd );

			// Report the change
			PlaylistsViewModel.PlaylistUpdated( playlist );
		}

		/// <summary>
		/// Create a new songs playlist and add a list of Songs to it
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="playlistName"></param>
		public async void AddSongsToNewPlaylistAsync( IEnumerable<Song> songsToAdd, string playlistName )
		{
			SongPlaylist newPlaylist = new SongPlaylist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId };

			await Playlists.AddPlaylistAsync( newPlaylist );

			newPlaylist.AddSongs( songsToAdd );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
		}


		/// <summary>
		/// Add a list of Albums to a specified playlist
		/// </summary>
		/// <param name="albumsToAdd"></param>
		/// <param name="playlist"></param>
		public void AddAlbumsToPlaylist( IEnumerable<Album> albumsToAdd, AlbumPlaylist playlist )
		{
			playlist.AddAlbums( albumsToAdd );

			// Report the change
			PlaylistsViewModel.PlaylistUpdated( playlist );
		}

		/// <summary>
		/// Create a new albums playlist and add a list of Albums to it
		/// </summary>
		/// <param name="albumsToAdd"></param>
		/// <param name="playlistName"></param>
		public async void AddAlbumsToNewPlaylist( IEnumerable<Album> albumsToAdd, string playlistName )
		{
			AlbumPlaylist newPlaylist = new AlbumPlaylist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId };

			await Playlists.AddPlaylistAsync( newPlaylist );

			newPlaylist.AddAlbums( albumsToAdd );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
		}

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public void MoveItemsDown( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsDown( items );

			PlaylistsViewModel.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public void MoveItemsUp( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsUp( items );

			PlaylistsViewModel.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Check if the specified playlist exists in other libraries
		/// </summary>
		/// <param name="name"></param>
		/// <param name="playListLibrary"></param>
		/// <returns></returns>
		public bool CheckForOtherPlaylists( string name, int playListLibrary ) =>
			Playlists.PlaylistCollection.Exists( list => ( list.Name == name ) && ( list.LibraryId != playListLibrary ) );

		/// <summary>
		/// Duplicate a playlist in the other libraries
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		public void DuplicatePlaylist( Playlist playlistToDuplicate )
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

					if ( playlistToDuplicate is SongPlaylist playlist )
					{
						DuplicateSongPlaylistAsync( playlist, library.Id );
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
		private async void DuplicateSongPlaylistAsync( SongPlaylist playlistToDuplicate, int libraryId )
		{
			// Attempt to find matching songs for each SongPlaylistItem in the SongPlaylist
			// Need to access the songs via the Sources associated with the Library
			List<Source> sources = Libraries.GetLibraryById( libraryId ).LibrarySources.ToList(); ;

			// Keep track of the matching songs
			List<Song> songsToAdd = new List<Song>();

			foreach ( SongPlaylistItem item in playlistToDuplicate.PlaylistItems )
			{
				Song matchingSong = null;
				int sourceIndex = 0;

				while ( ( matchingSong == null ) && ( sourceIndex < sources.Count ) )
				{
					// Get a list of all the songs with matching Titles in the source
					List<Song> matchingTitles = Songs.GetSourceSongsWithName( sources[ sourceIndex++ ].Id, item.Song.Title );

					// Now for each song access the associated artist
					int titleIndex = 0;
					while ( ( matchingSong == null ) && ( titleIndex < matchingTitles.Count ) )
					{
						Artist nameCheck = Artists.GetArtistById( ArtistAlbums.GetArtistAlbumById( matchingTitles[ titleIndex ].ArtistAlbumId ).ArtistId );

						// Correct name? 
						if ( nameCheck.Name.ToUpper() == item.Artist.Name.ToUpper() )
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

			// Only create the playlist if at least one of the songs was found
			if ( songsToAdd.Count > 0 )
			{
				SongPlaylist duplicatedPlaylist = new SongPlaylist() { Name = playlistToDuplicate.Name, LibraryId = libraryId };

				// Wait for the playlist to be added as we're going to use its id
				await Playlists.AddPlaylistAsync( duplicatedPlaylist );

				// Add the songs to the new SongPlaylist.
				duplicatedPlaylist.AddSongs( songsToAdd );

				// If all the songs in the playlist were found then set the song index as well
				if ( duplicatedPlaylist.PlaylistItems.Count == playlistToDuplicate.PlaylistItems.Count )
				{
					duplicatedPlaylist.SongIndex = playlistToDuplicate.SongIndex;
				}
			}
		}

		/// <summary>
		/// Duplicate the AlbumPlaylist in the specified library
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		/// <param name="libararyId"></param>
		private async void DuplicateAlbumPlaylistAsync( AlbumPlaylist playlistToDuplicate, int libraryId )
		{
			List<Album> albumsToAdd = new List<Album>();
			foreach ( AlbumPlaylistItem item in playlistToDuplicate.PlaylistItems )
			{
				// Find a matching Album name with the same Artist name
				Album matchingAlbum = Albums.AlbumCollection.Where( album => ( album.LibraryId == libraryId ) && 
					( album.Name.ToUpper() == item.Album.Name.ToUpper() ) && ( album.ArtistName.ToUpper() == item.Album.ArtistName.ToUpper() ) ).FirstOrDefault();
				if ( matchingAlbum != null )
				{
					albumsToAdd.Add( matchingAlbum );
				}
			}

			// Only create the playlist if we've got something to add to it
			if ( albumsToAdd.Count > 0 )
			{
				AlbumPlaylist duplicatedPlaylist = new AlbumPlaylist() { Name = playlistToDuplicate.Name, LibraryId = libraryId };
				await Playlists.AddPlaylistAsync( duplicatedPlaylist );

				duplicatedPlaylist.AddAlbums( albumsToAdd );

				// If all the albums in the playlist were found then set the song index as well. This assuming that the albums contain the same
				// number of songe
				if ( duplicatedPlaylist.PlaylistItems.Count == playlistToDuplicate.PlaylistItems.Count )
				{
					duplicatedPlaylist.SongIndex = playlistToDuplicate.SongIndex;
				}
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private async void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			PlaylistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the Playlists and playlist names. Make sure a copy of the list is used as we're going to sort it 
			PlaylistsViewModel.Playlists = Playlists.GetPlaylistsForLibrary( PlaylistsViewModel.LibraryId ).ToList();

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
				PlaylistsViewModel.SongPlaylists.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );
				PlaylistsViewModel.AlbumPlaylists.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

				// Now copy to the combined list
				PlaylistsViewModel.Playlists.Clear();
				PlaylistsViewModel.Playlists.AddRange( PlaylistsViewModel.SongPlaylists );
				PlaylistsViewModel.Playlists.AddRange( PlaylistsViewModel.AlbumPlaylists );
			} );

			PlaylistsViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data then reload
		/// </summary>
		/// <param name="_"></param>
		private void SelectedLibraryChanged( int _ )
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
		private void SongStarted( Song songStarted )
		{
			// Update the song index for any playlists for which the previous song and the current song are adjacent 
			Playlists.CheckForAdjacentSongEntries( previousSongIdentity, songStarted.Id );

			previousSongIdentity = songStarted.Id;
		}

		/// <summary>
		/// Called when the SongFinishedMessage has been received
		/// </summary>
		/// <param name="songPlayed"></param>
		private void SongFinished( Song songPlayed ) => Playlists.SongFinished( songPlayed.Id );

		/// <summary>
		/// Called when a PlaylistUpdatedMessage has been received. Pass it on to the reporter
		/// </summary>
		/// <param name="message"></param>
		private static void PlaylistUpdated( Playlist updatedPlaylist ) => PlaylistsViewModel.PlaylistUpdated( updatedPlaylist );

		/// <summary>
		/// The previous song id that has been played
		/// </summary>
		private static int previousSongIdentity = -1;
	}
}
