using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The ArtistAlbumAccess class is used to access and change ArtistAlbum data via the database
	/// </summary>
	static class ArtistAlbumAccess
	{
		/// <summary>
		/// Get all of the ArtistAlbum entries in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<ArtistAlbum>> GetArtistAlbumsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().ToListAsync();

		/// <summary>
		/// Insert a new ArtistAlbum in the database
		/// </summary>
		/// <param name="artistAlbum"></param>
		/// <returns></returns>
		public static async Task AddArtistAlbumAsync( ArtistAlbum artistAlbum ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( artistAlbum );

		/// <summary>
		/// Delete the specified ArtistAlbum entry
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static async void DeleteArtistAlbumAsync( ArtistAlbum albumToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( albumToDelete );
	}
}