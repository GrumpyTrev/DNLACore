using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The tag class contains a list of tagged albums
	/// </summary>
	[Table( "Tag" )]
	public class Tag
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		/// <summary>
		/// The full name of the tag
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Name to be displayed in tab when filter applied
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Sort tagged albums by tag id
		/// </summary>
		public bool TagOrder { get; set; } = false;

		/// <summary>
		/// Synchronise tagged albums across libraries
		/// </summary>
		public bool Synchronise { get; set; } = false;

		/// <summary>
		/// Should this tag be persisted in storage
		/// </summary>
		public bool PersistTag { get; set; } = false;

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
