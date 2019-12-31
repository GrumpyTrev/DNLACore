using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The AlbumAccess class is used to access and change Album data via the database
	/// </summary>
	static class AlbumAccess
	{
		/// <summary>
		/// Get all the Albums associated with the library identity
		/// </summary>
		public static async Task<List<Album>> GetAlbumDetailsAsync( int libraryId, Tag currentFilter )
		{
			List<Album> albums = null;

			// If there is no filter get all the albums in the library
			if ( currentFilter == null )
			{
				albums = ( await ConnectionDetailsModel.AsynchConnection.GetWithChildrenAsync<Library>( libraryId ) ).Albums;
			}
			else
			{
				// Only obtain albums that have been tagged

				// First of all form a list of all the album identities in the selected filter
				List<int> albumIds = currentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToList();

				// Now get all the albums that are tagged and in the correct library
				albums = ( await ConnectionDetailsModel.AsynchConnection.Table<Album>().
					Where( album => ( albumIds.Contains( album.Id ) == true ) && ( album.LibraryId == libraryId ) ).ToListAsync() );

				// Use the list of album ids to sort the albums
				albums = albums.OrderBy( album => albumIds.IndexOf( album.Id ) ).ToList();
			}

			foreach ( Album album in albums )
			{
				if ( ( album.VariousArtists == false ) && ( album.ArtistId != 0 ) )
				{
					album.Artist = await ConnectionDetailsModel.AsynchConnection.GetAsync<Artist>( album.ArtistId );
				}
			}

			return albums;
		}

		/// <summary>
		/// Get the Album specified by the id
		/// </summary>
		/// <param name="albumId"></param>
		/// <returns></returns>
		public static async Task<Album> GetAlbumAsync( int albumId ) => await ConnectionDetailsModel.AsynchConnection.GetAsync<Album>( albumId );

		/// <summary>
		/// Get the contents for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static async Task GetAlbumContentsAsync( Album theAlbum ) => await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( theAlbum );
	}
}