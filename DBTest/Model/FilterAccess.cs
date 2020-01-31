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
		public static async Task<List<Tag>> GetTagsAsync()
		{
			// Get all the Tag records with their TaggedAlbums.
			// Doing this manually rather that using the SQLite extenstion for efficiency
			Dictionary<int, Tag> tags = ( await ConnectionDetailsModel.AsynchConnection.Table<Tag>().ToListAsync() ).ToDictionary( tag => tag.Id );

			// Get all the TaggedAlbum entries and add them to their Tag entries
			( await ConnectionDetailsModel.AsynchConnection.GetAllWithChildrenAsync<TaggedAlbum>() ).ForEach( ta => tags[ ta.TagId ].TaggedAlbums.Add( ta ) );

			return tags.Values.ToList();
		}

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