using System.Collections.Generic;
using System.Linq;
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
			album.TagIndex = TaggedAlbums.Count;
			album.TagId = Id;

			if ( PersistTag == true )
			{
				DBTest.TaggedAlbums.AddTaggedAlbum( album );
			}

			TaggedAlbums.Add( album );
			AlbumToTagLookup[ album.AlbumId ] = album;
		}

		/// <summary>
		/// Update the existing tag with new properties
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
				DbAccess.UpdateAsync( this );
			}

			// The Tags class maintains lookup tables using the tag details so inform it of the change
			Tags.TagDetailsChanged( this, oldName, oldShortName );

			// Let everyone know about this
			new TagDetailsChangedMessage() { ChangedTag = this }.Send();
		}

		/// <summary>
		/// Move a set of selected items down and update the tag indexes
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsDown( IEnumerable<TaggedAlbum> items )
		{
			// There must be at least one TaggedAlbum entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			TaggedAlbum itemToMove = TaggedAlbums[ items.Last().TagIndex + 1 ];
			TaggedAlbums.RemoveAt( items.Last().TagIndex + 1 );
			TaggedAlbums.Insert( items.First().TagIndex, itemToMove );

			// Now the tag index numbers in the TaggedAlbum entries must be updated to match their index in the collection
			AdjustTagIndexes();
		}

		/// <summary>
		/// Move a set of selected items up and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsUp( IEnumerable<TaggedAlbum> items )
		{
			// There must be at least one TaggedAlbum entry above those that are selected. That entry needs to be moved to below the end of the selection
			TaggedAlbum itemToMove = TaggedAlbums[ items.First().TagIndex - 1 ];
			TaggedAlbums.RemoveAt( items.First().TagIndex - 1 );
			TaggedAlbums.Insert( items.Last().TagIndex, itemToMove );

			// Now the tag index numbers in the TaggedAlbum entries must be updated to match their index in the collection
			AdjustTagIndexes();
		}

		/// <summary>
		/// Adjust the tag index numbers to match the indexes in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		public void AdjustTagIndexes()
		{
			// The track numbers in the PlaylistItems must be updated to match their index in the collection
			for ( int index = 0; index < TaggedAlbums.Count; ++index )
			{
				TaggedAlbum itemToCheck = TaggedAlbums[ index ];
				if ( itemToCheck.TagIndex != index )
				{
					itemToCheck.TagIndex = index;

					// Update the item in the model. No need to wait for this.
					DbAccess.UpdateAsync( itemToCheck );
				}
			}
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