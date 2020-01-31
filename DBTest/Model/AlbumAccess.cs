using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;

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
		public static async Task<List<Album>> GetAlbumDetailsAsync( int libraryId ) => 
			await ConnectionDetailsModel.AsynchConnection.Table<Album>().Where( album => album.LibraryId == libraryId ).ToListAsync();

		/// <summary>
		/// Get an album from the database with the specified name, artist name and library
		/// </summary>
		/// <param name="albumName"></param>
		/// <param name="artistName"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task<Album> GetAlbumInLibraryAsync( string albumName, string artistName, int libraryId ) => 
			await ConnectionDetailsModel.AsynchConnection.Table<Album>()
				.Where( album => ( album.LibraryId == libraryId ) && ( album.Name == albumName ) && ( album.ArtistName == artistName ) ).FirstOrDefaultAsync();


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