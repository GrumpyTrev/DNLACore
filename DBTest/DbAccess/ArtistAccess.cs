using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The ArtistAccess class is used to access and change Artist data via the database
	/// </summary>
	static class ArtistAccess
	{
		/// <summary>
		/// Get all the Artists in the data base
		/// </summary>
		public static async Task<List<Artist>> GetAllArtistsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Artist>().ToListAsync();

		/// <summary>
		/// Insert a new Artist in the database
		/// </summary>
		/// <param name="artist"></param>
		/// <returns></returns>
		public static async Task AddArtistAsync( Artist artist ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( artist );

		/// <summary>
		/// Delete the specified Artist
		/// </summary>
		/// <param name="artistId"></param>
		/// <returns></returns>
		public static async void DeleteArtistAsync( Artist artist ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( artist );
	}
}