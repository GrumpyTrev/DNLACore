using System.Collections.Generic;
using System.Linq;

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
	}
}
