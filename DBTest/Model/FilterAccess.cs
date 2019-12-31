using System.Collections.Generic;
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
			// Get all the Tag records with their TaggedAlbums. Can't go fully recursive as that would read in the songs as well which are
			// not required - yet
			List< Tag > tags = await ConnectionDetailsModel.AsynchConnection.GetAllWithChildrenAsync<Tag>();

			// The Album entry in the TaggedAlbums will be required so we may as well get those now
			tags.ForEach( tag => tag.TaggedAlbums.ForEach( async taggedAlbum => await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( taggedAlbum ) ) );

			return tags;
		}

		/// <summary>
		/// Get the named tag
		/// </summary>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public static async Task<Tag> GetTagAsync( string tagName )
		{
			Tag namedTag = await ConnectionDetailsModel.AsynchConnection.GetAsync<Tag>( t => t.Name == tagName );
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( namedTag );

			return namedTag;
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
	}
}