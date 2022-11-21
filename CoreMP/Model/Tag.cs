using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The tag class contains a list of tagged albums
	/// </summary>
	public partial class Tag
	{
		/// <summary>
		/// Delete a set of TaggedAlbum entries from the tag
		/// </summary>
		/// <param name="albums"></param>
		public void DeleteTaggedAlbums( IEnumerable<TaggedAlbum> albums )
		{
			foreach ( TaggedAlbum album in albums )
			{
				DeleteTaggedAlbum( album );
			}
		}

		/// <summary>
		/// Delete a single TaggedAlbum from the tag
		/// </summary>
		/// <param name="album"></param>
		public void DeleteTaggedAlbum( TaggedAlbum album )
		{
			if ( PersistTag == true )
			{
				CoreMP.TaggedAlbums.DeleteTaggedAlbum( album );
			}

			TaggedAlbums.Remove( album );
			AlbumToTagLookup.Remove( album.AlbumId );
		}

		/// <summary>
		/// Add a TaggedAlbum to the Tag
		/// </summary>
		/// <param name="album"></param>
		public void AddTaggedAlbum( TaggedAlbum album )
		{
			album.TagIndex = TaggedAlbums.Count;
			album.TagId = Id;

			if ( PersistTag == true )
			{
				CoreMP.TaggedAlbums.AddTaggedAlbum( album );
			}

			TaggedAlbums.Add( album );
			AlbumToTagLookup[ album.AlbumId ] = album;
		}

		/// <summary>
		/// The collection of Albums associated with this tag
		/// </summary>
		[Ignore]
		public List<TaggedAlbum> TaggedAlbums { get; } = new List<TaggedAlbum>();

		/// <summary>
		/// Allow a TaggedAlbum entry to be accessed by associated Album is
		/// </summary>
		[Ignore]
		public Dictionary<int, TaggedAlbum> AlbumToTagLookup { get; } = new Dictionary<int, TaggedAlbum>();
	}
}
