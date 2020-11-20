using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Tags class holds a collection of all the Tag entries read from storage.
	/// </summary>	
	static class Tags
	{
		/// <summary>
		/// Get the Tags collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( TagsCollection == null )
			{
				// Get the current set of tags and form the lookup tables
				TagsCollection = await FilterAccess.GetTagsAsync();
				NameLookup = TagsCollection.ToDictionary( tag => tag.Name );
				ShortNameLookup = TagsCollection.ToDictionary( tag => tag.ShortName );
			}
		}

		/// <summary>
		/// Return the Tag with the specified name or null if not found
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tag GetTagByName( string name ) => NameLookup.GetValueOrDefault( name );

		/// <summary>
		/// Return the Tag with the specified short name or null if not found
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tag GetTagByShortName( string shortName ) => ShortNameLookup.GetValueOrDefault( shortName );

		/// <summary>
		/// Add a new tag to the storage and the local collections
		/// </summary>
		/// <param name="albumToAdd"></param>
		public static void AddTag( Tag tagToAdd )
		{
			TagsCollection.Add( tagToAdd );

			// No need to wait for this as we are not using its identity
			FilterAccess.AddTagAsync( tagToAdd );

			NameLookup[ tagToAdd.Name ] = tagToAdd;
			ShortNameLookup[ tagToAdd.ShortName ] = tagToAdd;
		}

		/// <summary>
		/// Delete a tag and its contents from storage and the local collection
		/// </summary>
		/// <param name="tagToDelete"></param>
		public static void DeleteTag( Tag tagToDelete )
		{
			// Delete all the TaggedAlbum entries from the Tag
			tagToDelete.DeleteTaggedAlbums( tagToDelete.TaggedAlbums.ToList() );

			// Delete the Tag itself. No need to wait for this
			FilterAccess.DeleteTagAsync( tagToDelete );

			// And locally
			TagsCollection.Remove( tagToDelete );

			new TagDeletedMessage() { DeletedTag = tagToDelete }.Send();
		}

		/// <summary>
		/// The details (and possibly the names) of a tag have changed. Update the name lookup tables
		/// </summary>
		/// <param name="changedTag"></param>
		/// <param name="oldName"></param>
		/// <param name="oldShortName"></param>
		public static void TagDetailsChanged( Tag changedTag, string oldName, string oldShortName )
		{
			NameLookup.Remove( oldName );
			NameLookup[ changedTag.Name ] = changedTag;
			ShortNameLookup.Remove( oldShortName );
			ShortNameLookup[ changedTag.ShortName ] = changedTag;
		}

		public static bool CheckNamesAreValid( string name, string shortName, Tag existingTag = null )
		{
			bool namesValid = true;

			// Check for a name change
			if ( ( existingTag == null ) || ( name != existingTag.Name ) )
			{
				// Check that there is not another tag with the new name
				namesValid = ( GetTagByName( name ) == null );
			}

			// Check for a short name change
			if ( ( namesValid == true ) && ( ( existingTag == null ) || ( shortName != existingTag.ShortName ) ) )
			{
				// Check that there is not another tag with the new short name
				namesValid = ( GetTagByShortName( shortName ) == null );
			}

			return namesValid;
		}

		/// <summary>
		/// The set of Tags currently held in storage
		/// </summary>
		public static List<Tag> TagsCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by tag name
		/// </summary>
		private static Dictionary<string, Tag> NameLookup { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by tag short name
		/// </summary>
		private static Dictionary<string, Tag> ShortNameLookup { get; set; } = null;
	}
}