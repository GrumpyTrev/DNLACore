using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The TaggedAlbums class holds a collection of all the TaggedAlbum entries read from storage.
	/// </summary>	
	static class TaggedAlbums
	{
		/// <summary>
		/// Get the TaggedAlbum collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( TaggedAlbumCollection == null )
			{
				// Get the current set of albums and form the lookup tables
				TaggedAlbumCollection = await FilterAccess.GetTaggedAlbumsAsync();
			}
		}

		/// <summary>
		/// The set of ArtistAlbums currently held in storage
		/// </summary>
		public static List<TaggedAlbum> TaggedAlbumCollection { get; set; } = null;
	}
}