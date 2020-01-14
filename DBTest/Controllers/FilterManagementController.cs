using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The FilterManagementController class responds to filter selection commands and reflects changes to other controllers
	/// </summary>
	public static class FilterManagementController
	{
		/// <summary>
		/// Register for external filter change messages
		/// </summary>
		static FilterManagementController()
		{
			Mediator.RegisterPermanent( SongPlayed, typeof( SongPlayedMessage ) );
			Mediator.RegisterPermanent( AlbumAdded, typeof( AlbumAddedMessage ) );
		}

		/// <summary>
		/// Get the Tag data from the database
		/// If the data has already been obtained then do nothing
		/// Otherwise get the data from the database asynchronously
		/// Data is not reported back as it is not required yet
		/// </summary>
		public static async void GetTagsAsync()
		{
			// Check if the tags have already been obtained
			if ( FilterManagementModel.Tags == null )
			{
				// New data is required
				FilterManagementModel.Tags = await FilterAccess.GetTagsAsync();

				// Extract the 'system' tags from this list for easy access later
				FilterManagementModel.RecentlyAddedTag = FilterManagementModel.Tags.SingleOrDefault( tag => tag.Name == RecentlyAddedTagName );
				FilterManagementModel.JustPlayedTag = FilterManagementModel.Tags.SingleOrDefault( tag => tag.Name == JustPlayedTagName );
			}
		}

		/// <summary>
		/// Return a list of the names of all the user tags
		/// </summary>
		/// <returns></returns>
		public static List<string> GetUserTagNames() => ( FilterManagementModel.Tags != null ) ? 
			FilterManagementModel.Tags.Where( tag => tag.UserTag == true ).Select( tag => tag.Name ).ToList() : new List<string>();

		/// <summary>
		/// Return the Tag with the given name, or null if no such Tag
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tag GetTagFromName( string name ) => FilterManagementModel.Tags.SingleOrDefault( tag => tag.Name == name );

		/// <summary>
		/// Update an existing Tag with updated values.
		/// Remove TaggedAlbums if the MaxCount for a library has now been exceeded
		/// </summary>
		/// <param name="existingTag"></param>
		/// <param name="updatedTag"></param>
		/// <param name="tagDelegate"></param>
		public static async void UpdateTagAsync( Tag existingTag, Tag updatedTag, TagUpdatedDelegate tagDelegate )
		{
			bool updateOk = true;

			// Check for a name change
			if ( updatedTag.Name != existingTag.Name )
			{
				// Check that there is not another tag with the new name
				updateOk = ( FilterManagementModel.Tags.Count( tag => ( tag.Name == updatedTag.Name ) ) == 0 );
			}

			// Check for a short name change
			if ( ( updateOk == true ) && ( updatedTag.ShortName != existingTag.ShortName ) )
			{
				updatedTag.ShortName = ( updatedTag.ShortName.Length > 0 ) ? updatedTag.ShortName : updatedTag.Name;

				// Check that there is not another tag with the new name
				updateOk = ( FilterManagementModel.Tags.Count( tag => ( tag.ShortName == updatedTag.ShortName ) ) == 0 );
			}

			if ( updateOk == true )
			{
				// No problems in performing the update.
				// Has the maximum number changed and do we need to do anything about it?
				if ( updatedTag.MaxCount != existingTag.MaxCount )
				{
					if ( ( updatedTag.MaxCount != -1 ) && ( ( existingTag.MaxCount == -1 ) || ( updatedTag.MaxCount < existingTag.MaxCount ) ) )
					{
						// Possibly need to reduce the number of tagged albums (for each library)
						List<Library> libraries = await LibraryAccess.GetLibrariesAsync();
						foreach ( Library lib in libraries )
						{
							// Get the count for this library 
							int tagCount = existingTag.TaggedAlbums.Count( taggedAlbum => ( taggedAlbum.Album.LibraryId == lib.Id ) );
							if ( tagCount > updatedTag.MaxCount )
							{
								// Need to remove the first (tagCount - updatedTag.MaxCount) entries from the Tag for this library
								List<TaggedAlbum> albums = existingTag.TaggedAlbums.Where( album => ( album.Album.LibraryId == lib.Id ) ).
									Take( tagCount - updatedTag.MaxCount ).ToList();

								foreach ( TaggedAlbum album in albums )
								{
									await FilterAccess.DeleteTaggedAlbumAsync( album );
									existingTag.TaggedAlbums.Remove( album );
								}
							}
						}
					}
				}

				// Update the details for the existing Tag and save it
				// Save the old name to send in the message
				string oldTagName = existingTag.Name;
				existingTag.Name = updatedTag.Name;
				existingTag.ShortName = updatedTag.ShortName;
				existingTag.MaxCount = updatedTag.MaxCount;
				existingTag.TagOrder = updatedTag.TagOrder;

				await FilterAccess.UpdateTagAsync( existingTag );

				new TagDetailsChangedMessage() { ChangedTag = existingTag, PreviousName = oldTagName }.Send();
			}

			tagDelegate( updateOk );
		}

		/// <summary>
		/// Update an existing Tag with updated values.
		/// Remove TaggedAlbums if the MaxCount for a library has now been exceeded
		/// </summary>
		/// <param name="existingTag"></param>
		/// <param name="updatedTag"></param>
		/// <param name="tagDelegate"></param>
		public static async void CreateTagAsync( Tag newTag, TagUpdatedDelegate tagDelegate )
		{
			bool updateOk = true;

			// Check for a valid name
			updateOk = ( newTag.Name.Length > 0 );

			if ( updateOk == true )
			{
				// Check that there is not another tag with the new name
				updateOk = ( FilterManagementModel.Tags.Count( tag => ( tag.Name == newTag.Name ) ) == 0 );
			}

			// Check for a valid short name
			if ( updateOk == true )
			{
				newTag.ShortName = ( newTag.ShortName.Length > 0 ) ? newTag.ShortName : newTag.Name;

				// Check that there is not another tag with the new name
				updateOk = ( FilterManagementModel.Tags.Count( tag => ( tag.ShortName == newTag.ShortName ) ) == 0 );
			}

			if ( updateOk == true )
			{
				// No problems in performing the update. Add the tag to the model
				newTag.UserTag = true;

				await FilterAccess.AddTagAsync( newTag );
				FilterManagementModel.Tags.Add( newTag );
			}

			tagDelegate( updateOk );
		}

		/// <summary>
		/// Apply the list of selected albums to the Tags data to produce a list of all available tags and an indication of 
		/// whether the tag has been applied to none, all or some of the albums
		/// </summary>
		/// <param name="selectedAlbums"></param>
		public static async void GetAppliedTagsAsync( List<Album> selectedAlbums, AppliedTagsDelegate tagsDelegate )
		{
			List<AppliedTag> appliedTags = new List<AppliedTag>();

			await Task.Run( () => {
				foreach ( Tag tag in FilterManagementModel.Tags )
				{
					AppliedTag appliedTag = new AppliedTag() { TagName = tag.Name };

					// Check if all, none or some of the selected albums are tagged with this tag
					int taggedCount = 0;
					foreach ( Album selectedAlbum in selectedAlbums )
					{
						if ( tag.TaggedAlbums.Exists( artistAlbum => ( artistAlbum.AlbumId == selectedAlbum.Id ) ) == true )
						{
							taggedCount++;
						}
					}

					// Set the Applied value according to the taggedCount
					appliedTag.Applied = ( taggedCount == 0 ) ? AppliedTag.AppliedType.None :
						( ( taggedCount == selectedAlbums.Count ) ? AppliedTag.AppliedType.All : AppliedTag.AppliedType.Some );

					// Keep track of the original value so that any changes can be processed
					appliedTag.OriginalApplied = appliedTag.Applied;

					appliedTags.Add( appliedTag );
				}
			} );

			// Pass back the results.
			tagsDelegate( appliedTags );
		}

		/// <summary>
		/// Apply any tag changes to the selected albums
		/// </summary>
		/// <param name="selectedAlbums"></param>
		/// <param name="appliedTags"></param>
		public static async void ApplyTagsAsync( List<Album> selectedAlbums, List<AppliedTag> appliedTags )
		{
			// Keep track of which tags have been changed so that this can be sent to other controllers
			List<string> changedTags = new List<string>();

			await Task.Run( async () => {
				foreach ( AppliedTag appliedTag in appliedTags )
				{
					// Has there been a change to this tag
					if ( appliedTag.Applied != appliedTag.OriginalApplied )
					{
						changedTags.Add( appliedTag.TagName );

						// Find the tag record
						Tag changedTag = FilterManagementModel.Tags.Find( tag => ( tag.Name == appliedTag.TagName ) );

						if ( appliedTag.Applied == AppliedTag.AppliedType.None )
						{
							// Remove the selected albums from this tag
							selectedAlbums.ForEach( async selectedAlbum => await RemoveAlbumFromTagAsync( changedTag, selectedAlbum.Id ) );
						}
						else if ( appliedTag.Applied == AppliedTag.AppliedType.All )
						{
							// Add the selected albums to this tag
							selectedAlbums.ForEach( async selectedAlbum => await AddAlbumToTagAsync( changedTag, selectedAlbum ) );
						}

						await FilterAccess.UpdateTagAsync( changedTag );
					}
				}
			} );

			// Report any tags that have changed
			if ( changedTags.Count > 0 )
			{
				new TagMembershipChangedMessage() { ChangedTags = changedTags }.Send();
			}
		}

		/// <summary>
		/// Delete the specified Tag 
		/// </summary>
		/// <param name="tagtoDelete"></param>
		public static async void DeleteTagAsync( Tag tagToDelete )
		{
			tagToDelete.TaggedAlbums.ForEach( async album => await FilterAccess.DeleteTaggedAlbumAsync( album ) );
			await FilterAccess.DeleteTagAsync( tagToDelete );

			FilterManagementModel.Tags.Remove( tagToDelete );

			new TagDeletedMessage() { DeletedTag = tagToDelete }.Send();
		}

		/// <summary>
		/// Remove the TaggedAlbum associated with the album from the tag
		/// </summary>
		/// <param name="fromTag"></param>
		/// <param name="albumId"></param>
		private static async Task RemoveAlbumFromTagAsync( Tag fromTag, int albumId )
		{
			// Check if the album is actually tagged
			TaggedAlbum taggedAlbum = fromTag.TaggedAlbums.SingleOrDefault( tag => ( tag.AlbumId == albumId ) );
			if ( taggedAlbum != null )
			{
				await FilterAccess.DeleteTaggedAlbumAsync( taggedAlbum );
				fromTag.TaggedAlbums.Remove( taggedAlbum );
			}
		}

		/// <summary>
		/// Add a TagedAlnum entry for the album to the tag
		/// </summary>
		/// <param name="toTag"></param>
		/// <param name="albumToAdd"></param>
		private static async Task AddAlbumToTagAsync( Tag toTag, Album albumToAdd )
		{
			// Keep track of whether or not a new entry is required, as it gets complicated
			bool addNewEntry = true;

			// Check whether or not this album is already in the tag
			int index = toTag.TaggedAlbums.FindIndex( tag => ( tag.AlbumId == albumToAdd.Id ) );
			if ( index != -1 )
			{
				// If this Tag is ordered by tag id and this existing entry is not the most recently added then
				// the existing entry will have to be removed and a new entry added at the end
				if ( ( toTag.TagOrder == true ) && ( index < ( toTag.TaggedAlbums.Count - 1 ) ) )
				{
					await FilterAccess.DeleteTaggedAlbumAsync( toTag.TaggedAlbums[ index ] );
					toTag.TaggedAlbums.RemoveAt( index );
				}
				else
				{
					// A new entry is not required
					addNewEntry = false;
				}
			}
			else
			{
				// There is no existing entry. If there is a limit to the number of albums that
				// can be tagged then check if the limit has been reached for the associated library and remove the oldest (lowest id) entry
				if ( toTag.MaxCount != -1 )
				{
					// Count the number of entries for the same library as the album being added
					if ( toTag.TaggedAlbums.Count( taggedAlbum => ( taggedAlbum.Album.LibraryId == albumToAdd.LibraryId ) ) >= toTag.MaxCount )
					{
						// Remove the oldest entry, i.e. the first entry 
						await FilterAccess.DeleteTaggedAlbumAsync( toTag.TaggedAlbums.First() );
						toTag.TaggedAlbums.RemoveAt( 0 );
					}
				}
			}

			if ( addNewEntry == true )
			{
				// Add a new entry
				TaggedAlbum newTaggedAlbum = new TaggedAlbum() { TagId = toTag.Id, AlbumId = albumToAdd.Id, Album = albumToAdd };

				await FilterAccess.AddTaggedAlbumAsync( newTaggedAlbum );
				toTag.TaggedAlbums.Add( newTaggedAlbum );
			}
		}

		/// <summary>
		/// Called when the SongPlayedMessage is received
		/// Add the associated album to the Just Played tag 
		/// </summary>
		/// <param name="message"></param>
		private static async void SongPlayed( object message )
		{
			// Only process this if the Just Played tag exists
			if ( FilterManagementModel.JustPlayedTag != null )
			{
				// Need to get the Album rather than just the AlbumId to access the library id
				Album songAlbum = await AlbumAccess.GetAlbumAsync( ( message as SongPlayedMessage ).SongPlayed.AlbumId );
				await AddAlbumToTagAsync( FilterManagementModel.JustPlayedTag, songAlbum );

				// Report this filter change if the associated library is being displayed
				if ( ConnectionDetailsModel.LibraryId == songAlbum.LibraryId )
				{
					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { JustPlayedTagName } }.Send();
				}
			}
		}

		/// <summary>
		/// Called when the AlbumAddedMessage is received
		/// Add the associated album to the Recently Added tag 
		/// </summary>
		/// <param name="message"></param>
		private static async void AlbumAdded( object message )
		{
			// Only process this if the Recently Added tag exists
			if ( FilterManagementModel.RecentlyAddedTag != null )
			{
				Album albumAdded = ( message as AlbumAddedMessage ).AlbumAdded;
				await AddAlbumToTagAsync( FilterManagementModel.RecentlyAddedTag, albumAdded );

				// Report this filter change if the associated library is being displayed
				if ( ConnectionDetailsModel.LibraryId == albumAdded.LibraryId )
				{
					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { RecentlyAddedTagName } }.Send();
				}
			}
		}

		/// <summary>
		/// Definition of delegate to call when the set of applied tags has been determined
		/// </summary>
		/// <param name="appliedTags"></param>
		public delegate void AppliedTagsDelegate( List<AppliedTag> appliedTags );

		/// <summary>
		/// Definition of delegate to call when a Tag has been updated
		/// </summary>
		/// <returns></returns>
		public delegate void TagUpdatedDelegate( bool updateOk );

		/// <summary>
		/// The name given to the "Just played" tag
		/// </summary>
		public const string JustPlayedTagName = "Just played";

		/// <summary>
		/// The name given to the "Recently added" tag
		/// </summary>
		public const string RecentlyAddedTagName = "Recently added";
	}
}