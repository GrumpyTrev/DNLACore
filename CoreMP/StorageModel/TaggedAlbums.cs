using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The TaggedAlbums class holds a collection of all the TaggedAlbum entries read from storage.
	/// </summary>	
	internal static class TaggedAlbums
	{
		/// <summary>
		/// Get the TaggedAlbum collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync() => TaggedAlbumCollection ??= await DbAccess.LoadAsync<TaggedAlbum>();

		/// <summary>
		/// Delete the specified TaggedAlbum from the storage and the collection
		/// </summary>
		/// <param name="taggedAlbumToDelete"></param>
		/// <returns></returns>
		public static void DeleteTaggedAlbum( TaggedAlbum taggedAlbumToDelete )
		{
			// No need to wait for the TaggedAlbum to be deleted from storage
			DbAccess.DeleteAsync( taggedAlbumToDelete );
			_ = TaggedAlbumCollection.Remove( taggedAlbumToDelete );
		}

		/// <summary>
		/// Add a new TaggedAlbum to the storage and the local collections
		/// </summary>
		/// <param name="artistAlbumToAdd"></param>
		public static void AddTaggedAlbum( TaggedAlbum taggedAlbumToAdd )
		{
			DbAccess.Insert( taggedAlbumToAdd );
			TaggedAlbumCollection.Add( taggedAlbumToAdd );
		}

		/// <summary>
		/// The set of ArtistAlbums currently held in storage
		/// </summary>
		public static List<TaggedAlbum> TaggedAlbumCollection { get; set; } = null;
	}
}
