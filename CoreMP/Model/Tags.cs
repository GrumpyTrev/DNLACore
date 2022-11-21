using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Tags class holds a collection of all the Tag entries read from storage.
	/// </summary>	
	public static class Tags
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
				TagsCollection = await DbAccess.LoadAsync<Tag>();
				NameLookup = TagsCollection.ToDictionary( tag => tag.Name );
			}
		}

		/// <summary>
		/// Return the Tag with the specified name or null if not found
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tag GetTagByName( string name ) => NameLookup.GetValueOrDefault( name );

		/// <summary>
		/// Add a new tag to the storage and the local collections
		/// </summary>
		/// <param name="albumToAdd"></param>
		public static async void AddTagAsync( Tag tagToAdd )
		{
			TagsCollection.Add( tagToAdd );

			if ( tagToAdd.PersistTag == true )
			{
				// We need to wait for this in case the caller want to use the Tag Id
				await DbAccess.InsertAsync( tagToAdd );
			}

			NameLookup[ tagToAdd.Name ] = tagToAdd;
		}

		/// <summary>
		/// The set of Tags currently held in storage
		/// </summary>
		public static List<Tag> TagsCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by tag name
		/// </summary>
		private static Dictionary<string, Tag> NameLookup { get; set; } = null;
	}
}
