using System.Threading.Tasks;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AlbumAccess class is used to access and change Album data via the database
	/// </summary>
	static class AlbumAccess
	{
		/// <summary>
		/// Get all the Albums in the data base
		/// </summary>
		public static async Task<List<Album>> GetAllAlbumsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Album>().ToListAsync();

		/// <summary>
		/// Get the songs for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static async Task< List< Song > > GetAlbumSongsAsync( int albumId ) => 
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.AlbumId == albumId) ).ToListAsync();

		/// <summary>
		/// Insert a new Album in the database
		/// </summary>
		/// <param name="album"></param>
		/// <returns></returns>
		public static async Task AddAlbumAsync( Album album ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( album );

		/// <summary>
		/// Update the database with any changes to this Album
		/// </summary>
		/// <param name="album"></param>
		/// <returns></returns>
		public static async void UpdateAlbumAsync( Album album ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( album );

		/// <summary>
		/// Delete the specifed Album
		/// </summary>
		/// <param name="albumId"></param>
		/// <returns></returns>
		public static async void DeleteAlbumAsync( Album album ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( album );
	}
}