using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The LibraryManagementController is the Controller for the LibraryManagement view. It responds to LibraryManagement commands and maintains library data 
	/// in the LibraryManagementModel
	/// </summary>
	internal static class LibraryManagementController
	{
		/// <summary>
		/// Update the selected libary in the database and the ConnectionDetailsModel.
		/// Notify other controllers
		/// </summary>
		/// <param name="selectedLibrary">The newly selected library</param>
		public static void SelectLibrary( Library selectedLibrary )
		{
			// Only process this if the library has changed
			if ( selectedLibrary.Id != ConnectionDetailsModel.LibraryId )
			{
				Playback.LibraryId = selectedLibrary.Id;
				ConnectionDetailsModel.LibraryId = selectedLibrary.Id;
				new SelectedLibraryChangedMessage() { SelectedLibrary = selectedLibrary.Id }.Send();
			}
		}

		/// <summary>
		/// Clear the contents of the specified library
		/// </summary>
		/// <param name="libraryToClear"></param>
		/// <returns></returns>
		public static void ClearLibrary( Library libraryToClear )
		{
			int libId = libraryToClear.Id;

			// Delete all the artists in the library and their associated ArtistAlbum entries
			List<Artist> artists = Artists.ArtistCollection.Where( art => art.LibraryId == libId ).ToList();
			Artists.DeleteArtists( artists );

			ArtistAlbums.DeleteArtistAlbums(
				ArtistAlbums.ArtistAlbumCollection.Where( artAlb => artists.Any( art => art.Id == artAlb.ArtistId ) ).Distinct().ToList() );

			// Delete all the albums in the library and any tags associated with them
			List<Album> albums = Albums.AlbumCollection.Where( alb => alb.LibraryId == libId ).ToList();
			Albums.DeleteAlbums( albums );

			// We can use the FilterManagementController to carry out the Tag deletions.
			new AlbumsDeletedMessage() { DeletedAlbumIds = albums.Select( alb => alb.Id ).ToList() }.Send();

			// Delete all the user playlists and thier contents
			Playlists.GetPlaylistsForLibrary( libId ).ForEach( play => Playlists.DeletePlaylist( play ) );

			// Delete the contents of the NowPlayingList but keep the playlist itself
			Playlist nowPlaying = Playlists.GetNowPlayingPlaylist( libId );
			nowPlaying.Clear();
			nowPlaying.SongIndex = -1;

			// Delete all the songs in each of the sources associated with the library
			List<Source> sources = Sources.GetSourcesForLibrary( libId );
			foreach ( Source source in sources )
			{
				Songs.DeleteSongs( Songs.GetSourceSongs( source.Id ) );
				source.Songs = null;
			}

			// Delete the autoplay record associated with this library
			Autoplay autoplayForLibrary = Autoplays.AutoplayCollection.SingleOrDefault( auto => auto.LibraryId == libId );
			if ( autoplayForLibrary != null)
			{
				Autoplays.DeleteAutoplay( autoplayForLibrary );
			}
		}

		/// <summary>
		/// Add a new library with a default source to the collection
		/// </summary>
		/// <param name="libraryName"></param>
		public static async void CreateLibrary( string libraryName )
		{
			// Create a library with a default source and display the source editing fragment
			Library newLibrary = new() { Name = libraryName };
			await Libraries.AddLibraryAsync( newLibrary );

			// Add a source
			CreateSource( newLibrary );

			// Add an empty NowPlaying list
			Playlist nowPlaying = new SongPlaylist() { Name = Playlists.NowPlayingPlaylistName, LibraryId = newLibrary.Id };
			await Playlists.AddPlaylistAsync( nowPlaying );
			nowPlaying.SongIndex = -1;
		}

		/// <summary>
		/// Called to delete a library. Clear the library first, then remove all of it's sources and finally delete the library
		/// </summary>
		/// <param name="libraryToDelete"></param>
		public static void DeleteLibrary( Library libraryToDelete )
		{
			// Clear the library
			ClearLibrary( libraryToDelete );

			// Delete all the sources associated with the library
			foreach ( Source sourceToDelete in Sources.GetSourcesForLibrary( libraryToDelete.Id ))
			{
				Sources.DeleteSource( sourceToDelete );
			}

			// Delete the library
			Libraries.DeleteLibrary( libraryToDelete );
		}

		/// <summary>
		/// Create a new Source and add to the sources collections
		/// </summary>
		/// <param name="libraryForSource"></param>
		public static void CreateSource( Library libraryForSource )
		{
			Source newSource = new()
			{
				Name = libraryForSource.Name,
				AccessMethod = Source.AccessType.Local,
				FolderName = libraryForSource.Name,
				LibraryId = libraryForSource.Id
			};

			Sources.AddSource( newSource );
		}

		/// <summary>
		/// Delete the Source and all its associated Songs, ArtistAlbums, Albums, Artists and Playlists
		/// </summary>
		/// <param name="sourceToDelete"></param>
		public static void DeleteSource( Source sourceToDelete )
		{
			// Remove the Source from the Source collections
			Sources.DeleteSource( sourceToDelete );

			// Get all the songs associated with this source
			sourceToDelete.GetSongs();

			// Are there any
			if ( sourceToDelete.Songs != null )
			{
				// Remove the Songs from their associated ArtistAlbum and Album entries. This may mean that some
				// ArtistAlbum and Album can also be deleted
				List<ArtistAlbum> artAlbumsToDelete = new();
				List<Album> albumsToDelete = new();

				foreach ( ArtistAlbum artAlbum in ArtistAlbums.ArtistAlbumCollection )
				{
					// Get all the deleted songs in this ArtistAlbum
					List<Song> songsInArtAlbum = sourceToDelete.Songs.Where( song => song.ArtistAlbumId == artAlbum.Id ).ToList();

					// Are there any
					if ( songsInArtAlbum.Count > 0 )
					{
						// Make sure that this ArtistAlbum's Songs collection is populated
						if ( artAlbum.Songs == null )
						{
							artAlbum.Songs = Songs.GetArtistAlbumSongs( artAlbum.Id );
						}

						// Have all the songs in the ArtistAlbum been deleted
						if ( songsInArtAlbum.Count == artAlbum.Songs.Count )
						{
							// Yes, delete the ArtistAlbum. Don't delete here as we're enumerating the collection
							artAlbumsToDelete.Add( artAlbum );
						}
						else
						{
							// No, delete the individual songs from the ArtistAlbum
							foreach ( Song songToDelete in songsInArtAlbum )
							{
								artAlbum.Songs.Remove( songToDelete );
							}
						}

						// Check for songs in the Album as well
						// Make sure the songs have been read in
						artAlbum.Album.GetSongs();

						if ( ( songsInArtAlbum.Count == artAlbum.Album.Songs.Count ) || ( artAlbum.Album.Songs.Count == 0 ) )
						{
							// All the Album's songs have been removed. Delete it
							albumsToDelete.Add( artAlbum.Album );
							Albums.DeleteAlbum( artAlbum.Album );
						}
						else
						{
							// Remove the songs from the associated Album
							foreach ( Song songToDelete in songsInArtAlbum )
							{
								artAlbum.Album.Songs.Remove( songToDelete );
							}
						}
					}
				}

				// Delete any ArtistAlbums saved for deletion
				if ( artAlbumsToDelete.Count > 0 )
				{
					ArtistAlbums.DeleteArtistAlbums( artAlbumsToDelete );

					// Remove these ArtistAlbums from the Artists and delete any subsequently empty Artists
					foreach ( ArtistAlbum artAlbum in artAlbumsToDelete )
					{
						artAlbum.Artist.ArtistAlbums.Remove( artAlbum );

						if ( artAlbum.Artist.ArtistAlbums.Count == 0 )
						{
							Artists.DeleteArtist( artAlbum.Artist );
						}
					}
				}

				// Delete these songs from all the Song Playlists and the Albums from the Album Playlists
				// Speed up this by making hashsets of the Songs and Albums
				HashSet<int> songIds = sourceToDelete.Songs.Select( song => song.Id ).ToHashSet();
				HashSet<int> albumIds = albumsToDelete.Select( album => album.Id ).ToHashSet();

				List<Playlist> playlistsToDelete = new();
				foreach ( Playlist playlist in Playlists.PlaylistCollection )
				{
					// Remove the Songs from SongPlaylists and Albums from AlbumPlaylisst
					if ( playlist is SongPlaylist songPlaylist )
					{
						songPlaylist.DeleteMatchingSongs( songIds );
					}
					else
					{
						( ( AlbumPlaylist )playlist ).DeleteMatchingAlbums( albumIds );
					}

					// If the Playlist is now empty then resord that it needs to be deleted.
					// Cannot be deleted in this loop
					if ( playlist.PlaylistItems.Count == 0 )
					{
						playlistsToDelete.Add( playlist );
					}
				}

				// Delete the empty song playlists.
				foreach ( Playlist playlistToDelete in playlistsToDelete )
				{
					// Don't delete the Now Playing Playlist
					if ( playlistToDelete.Name != Playlists.NowPlayingPlaylistName )
					{
						Playlists.DeletePlaylist( playlistToDelete );
					}
				}

				// Tag management is carried out via a message to the controller
				if ( albumIds.Count > 0 )
				{
					new AlbumsDeletedMessage() { DeletedAlbumIds = albumIds.ToList() }.Send();
				}

				// Delete all the Songs
				Songs.DeleteSongs( sourceToDelete.Songs );
			}
		}

		/// <summary>
		/// Is the specified library clear. An indication of this is whether there are any artists associated with the library
		/// </summary>
		/// <param name="libraryToCheck"></param>
		/// <returns></returns>
		public static bool IsEmpty( Library libraryToCheck ) => ( Artists.ArtistCollection.Any( art => art.LibraryId == libraryToCheck.Id ) == false );
	}
}
