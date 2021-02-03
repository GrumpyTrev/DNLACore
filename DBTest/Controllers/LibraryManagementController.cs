using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The LibraryManagementController is the Controller for the LibraryManagement. It responds to LibraryManagement commands and maintains library data in the
	/// LibraryManagementModel
	/// </summary>
	static class LibraryManagementController
	{
		/// <summary>
		/// Update the selected libary in the database and the ConnectionDetailsModel.
		/// Notify other controllers
		/// </summary>
		/// <param name="selectedLibrary"></param>
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
		public static async Task ClearLibraryAsync( Library libraryToClear )
		{
			int libId = libraryToClear.Id;

			// Delete all the artists in the library and their associated ArtistAlbum entries
			// Use a List rather than a lazy enumerator here as we'll be removing entries from the collection being enumerated
			List<Artist> artists = Artists.ArtistCollection.Where( art => art.LibraryId == libId ).ToList();
			foreach ( Artist artist in artists )
			{
				Artists.DeleteArtist( artist );

				// Use a List rather than a lazy enumerator here as we'll be removing entries from the collection being enumerated
				ArtistAlbums.ArtistAlbumCollection.Where( artAlb => artAlb.ArtistId == artist.Id ).ToList()
					.ForEach( artAlb => ArtistAlbums.DeleteArtistAlbum( artAlb ) );
			}

			// Delete all the albums in the library and any tags associated with them
			// Use a List rather than a lazy enumerator here as we'll be removing entries from the collection being enumerated
			List<Album> albums = Albums.AlbumCollection.Where( alb => alb.LibraryId == libId ).ToList();
			albums.ForEach( alb => Albums.DeleteAlbum( alb ) );

			// We can use the FilterManagementController to carry out the Tag deletions.
			new AlbumsDeletedMessage() { DeletedAlbumIds = albums.Select( alb => alb.Id ).ToList() }.Send();

			// Delete all the user playlists and thier contents
			Playlists.GetPlaylistsForLibrary( libId ).ForEach( play => Playlists.DeletePlaylist( play ) );

			// Delete the contenst of the NowPlayingList but keep the playlist itself
			Playlist nowPlaying = Playlists.GetNowPlayingPlaylist( libId );
			nowPlaying.DeletePlaylistItems( nowPlaying.PlaylistItems.ToList() );

			// Delete all the songs in each of the sources associated with the library
			List<Source> sources = Sources.GetSourcesForLibrary( libId );
			foreach ( Source source in sources )
			{
				List<Song> songs = await SongAccess.GetSongsForSourceAsync( source.Id );
				await SongAccess.DeleteSongsAsync( songs );
			}
		}
	}
}