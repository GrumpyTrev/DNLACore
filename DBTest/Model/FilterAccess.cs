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

			return tags;
		}

		/// <summary>
		/// Remove an ArtistAlbum from the specified Tag
		/// </summary>
		/// <param name="changedTag"></param>
		/// <param name="selectedAlbum"></param>
		public static async void RemoveTaggedAlbumAsync( Tag changedTag, ArtistAlbum selectedAlbum )
		{
			// Check if the album is actually tagged
			int index = changedTag.TaggedAlbums.FindIndex( tag => ( tag.AlbumId == selectedAlbum.AlbumId ) );
			if ( index != -1 )
			{
				await ConnectionDetailsModel.AsynchConnection.DeleteAsync( changedTag.TaggedAlbums[ index ] );
				changedTag.TaggedAlbums.RemoveAt( index );
				await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( changedTag );
			}
		}

		/// <summary>
		/// Add a new TaggedAlbum entry for the Tag and ArtistAlbum combination and add it to the database and the tag
		/// If an existing entry already exists remove it first
		/// </summary>
		/// <param name="tagToAdd"></param>
		/// <param name="selectedAlbum"></param>
		public static async void AddTaggedAlbumAsync( Tag tagToAdd, ArtistAlbum selectedAlbum )
		{
			// look for existing entry
			int index = tagToAdd.TaggedAlbums.FindIndex( tag => ( tag.AlbumId == selectedAlbum.AlbumId ) );
			if ( index != -1 )
			{
				await ConnectionDetailsModel.AsynchConnection.DeleteAsync( tagToAdd.TaggedAlbums[ index ] );
				tagToAdd.TaggedAlbums.RemoveAt( index );
			}

			// Add a new entry
			TaggedAlbum newTaggedAlbum = new TaggedAlbum() { TagId = tagToAdd.Id, AlbumId = selectedAlbum.AlbumId };

			await ConnectionDetailsModel.AsynchConnection.InsertAsync( newTaggedAlbum );

			tagToAdd.TaggedAlbums.Add( newTaggedAlbum );
			await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( tagToAdd );
		}
	}
}