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
			// No need to wait for the delete
			FilterAccess.DeleteTaggedAlbumAsync( album );
			TaggedAlbums.Remove( album );
		}

		/// <summary>
		/// Add a TaggedAlbum to the Tag
		/// </summary>
		/// <param name="album"></param>
		public void AddTaggedAlbum( TaggedAlbum album )
		{
			// No need to wait for the delete
			FilterAccess.AddTaggedAlbumAsync( album );
			TaggedAlbums.Add( album );
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
			MaxCount = newTagDetails.MaxCount;
			TagOrder = newTagDetails.TagOrder;
			Synchronise = newTagDetails.Synchronise;

			// No need to wait for this
			FilterAccess.UpdateTagAsync( this );

			// The Tags class maintains lookup tables using the tag details so inform it of the change
			Tags.TagDetailsChanged( this, oldName, oldShortName );

			// Let everyone know about this
			new TagDetailsChangedMessage() { ChangedTag = this, PreviousName = oldName }.Send();
		}

		[Ignore]
		public List<TaggedAlbum> TaggedAlbums { get; set; } = new List<TaggedAlbum>();
	}
}