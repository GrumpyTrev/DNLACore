using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;

namespace DBTest
{
	/// <summary>
	/// The FilterAccess class is used to access and change Filter/Tag data via the database
	/// </summary>
	static class FilterAccess
	{
		/// <summary>
		/// Get all the Tags in the database with their children TaggedAlbums
		/// NB This list is for the entire database. The Albums will generally be filtered by library id before being used.
		/// </summary>
		public static async Task<List<Tag>> GetTagsAsync() =>
			await ConnectionDetailsModel.AsynchConnection.Table<Tag>().ToListAsync();

		/// <summary>
		/// Get all the TaggedAlbum entries in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<TaggedAlbum>> GetTaggedAlbumsAsync() =>
			await ConnectionDetailsModel.AsynchConnection.Table<TaggedAlbum>().ToListAsync();

		/// <summary>
		/// Get all the Genres in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Genre>> GetGenresAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Genre>().ToListAsync();

		/// <summary>
		/// Add a new Genre to the collection 
		/// </summary>
		/// <param name="genreToAdd"></param>
		/// <returns></returns>
		public static async Task AddGenre( Genre genreToAdd ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( genreToAdd );

		/// <summary>
		/// Remove the specified TaggedAlbum  from the database
		/// </summary>
		/// <param name="taggedAlbum"></param>
		public static async Task DeleteTaggedAlbumAsync( TaggedAlbum taggedAlbum ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( taggedAlbum );

		/// <summary>
		/// Update the database with any changes to this Tag
		/// </summary>
		/// <param name="tagToUpdate"></param>
		public static async Task UpdateTagAsync( Tag tagToUpdate ) => await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( tagToUpdate );

		/// <summary>
		/// Add the specified TaggedAlbum to the database
		/// </summary>
		/// <param name="taggedAlbum"></param>
		public static async Task AddTaggedAlbumAsync( TaggedAlbum taggedAlbum ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( taggedAlbum );

		/// <summary>
		/// Add the specified tag to the database
		/// </summary>
		/// <param name="tagToAdd"></param>
		/// <returns></returns>
		public static async Task AddTagAsync( Tag tagToAdd ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( tagToAdd );

		/// <summary>
		/// Delete the specified tag from the database
		/// </summary>
		/// <param name="tagToDelete"></param>
		/// <returns></returns>
		public static async Task DeleteTagAsync( Tag tagToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( tagToDelete );
	}
}