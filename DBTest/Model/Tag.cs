using System.Collections.Generic;
using SQLite;

namespace DBTest
{
	/// <summary>
	/// The tag class contains a list of tagged albums
	/// </summary>
	partial class Tag
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
				DBTest.TaggedAlbums.DeleteTaggedAlbum( album );
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
			if ( PersistTag == true )
			{
				DBTest.TaggedAlbums.AddTaggedAlbum( album );
			}

			TaggedAlbums.Add( album );
			AlbumToTagLookup[ album.AlbumId ] = album;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newTagDetails"></param>
		public void UpdateTagDetails( Tag newTagDetails )
		{
			// Update the details for the existing Tag and save it
			// Save the old names to send in the message
			string oldName = Name;
			string oldShortName = ShortName;

			Name = newTagDetails.Name;
			ShortName = newTagDetails.ShortName;
			TagOrder = newTagDetails.TagOrder;
			Synchronise = newTagDetails.Synchronise;

			if ( PersistTag == true )
			{
				// No need to wait for this
				FilterAccess.UpdateTagAsync( this );
			}

			// The Tags class maintains lookup tables using the tag details so inform it of the change
			Tags.TagDetailsChanged( this, oldName, oldShortName );

			// Let everyone know about this
			new TagDetailsChangedMessage() { ChangedTag = this }.Send();
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