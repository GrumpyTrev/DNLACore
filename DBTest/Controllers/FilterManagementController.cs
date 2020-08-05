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
			Mediator.RegisterPermanent( AlbumsDeleted, typeof( AlbumsDeletedMessage ) );
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

				/// Get the current set of libraries as they'll be required for tag synchronisation
				FilterManagementModel.Libraries = await LibraryAccess.GetLibrariesAsync();
			}
		}

		/// <summary>
		/// Return a list of the names of all the user tags
		/// </summary>
		/// <returns></returns>
		public static List<string> GetUserTagNames() => ( FilterManagementModel.Tags != null ) ?
			FilterManagementModel.Tags.Where( tag => tag.UserTag == true ).Select( tag => tag.Name ).ToList() : new List<string>();

		/// <summary>
		/// Return a list of the names of all the tags
		/// </summary>
		/// <returns></returns>
		public static List<string> GetTagNames() => ( FilterManagementModel.Tags != null ) ?
			FilterManagementModel.Tags.Select( tag => tag.Name ).ToList() : new List<string>();

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
						foreach ( Library lib in FilterManagementModel.Libraries )
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

				// If the synchronise libraries flag has just been set then attempt to synchronise the contents of this tag
				// across all libraries
				if ( ( updatedTag.Synchronise != existingTag.Synchronise ) && ( updatedTag.Synchronise == true ) )
				{
					// Find all the unique Album/Artist name associated with the tag
					var distinctAlbums = existingTag.TaggedAlbums.Select( tagged => new { tagged.Album.Name, tagged.Album.ArtistName } ).Distinct().ToList();

					// Now check that each distinct album is tagged in each library, if present in the library
					foreach ( var distinctAlbum in distinctAlbums )
					{
						// If there are as many tagged albums with matching name and artist name as there are libraries then no work is required
						List<Album> matchingAlbums = existingTag.TaggedAlbums
							.Where( tagged => ( tagged.Album.Name == distinctAlbum.Name ) && ( tagged.Album.ArtistName == distinctAlbum.ArtistName ) )
							.Select( tagged => tagged.Album ).ToList();

						if ( matchingAlbums.Count != FilterManagementModel.Libraries.Count )
						{
							// Need to work out which albums are missing and the check if that album is actually in its library
							foreach ( Library library in FilterManagementModel.Libraries )
							{
								if ( matchingAlbums.FindIndex( album => ( album.LibraryId == library.Id ) ) == -1 )
								{
									// No tag found for the album in the current library.
									// Does the album/artist combination exist in the library
									Album albumToTag = await AlbumAccess.GetAlbumInLibraryAsync( distinctAlbum.Name, distinctAlbum.ArtistName, library.Id );
									if ( albumToTag != null )
									{
										await AddAlbumToTagAsync( existingTag, albumToTag );
									}
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
				existingTag.TagOrder = updatedTag.TagOrder;
				existingTag.Synchronise = updatedTag.Synchronise;

				await FilterAccess.UpdateTagAsync( existingTag );

				new TagDetailsChangedMessage() { ChangedTag = existingTag, PreviousName = oldTagName }.Send();
			}

			tagDelegate( updateOk );
		}

		/// <summary>
		/// Create a new tag
		/// </summary>
		/// <param name="newTag"></param>
		/// <param name="tagDelegate"></param>
		public static async void CreateTagAsync( Tag newTag, TagUpdatedDelegate tagDelegate )
		{
			bool createdOk = true;

			// Check for a valid name
			createdOk = ( newTag.Name.Length > 0 );

			if ( createdOk == true )
			{
				// Check that there is not another tag with the new name
				createdOk = ( FilterManagementModel.Tags.Count( tag => ( tag.Name == newTag.Name ) ) == 0 );
			}

			// Check for a valid short name
			if ( createdOk == true )
			{
				newTag.ShortName = ( newTag.ShortName.Length > 0 ) ? newTag.ShortName : newTag.Name;

				// Check that there is not another tag with the new name
				createdOk = ( FilterManagementModel.Tags.Count( tag => ( tag.ShortName == newTag.ShortName ) ) == 0 );
			}

			if ( createdOk == true )
			{
				// No problems in performing the update. Add the tag to the model
				newTag.UserTag = true;

				await FilterAccess.AddTagAsync( newTag );
				FilterManagementModel.Tags.Add( newTag );
			}

			tagDelegate( createdOk );
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
							selectedAlbums.ForEach( async selectedAlbum => await RemoveAlbumFromTagAsync( changedTag, selectedAlbum ) );
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
		private static async Task RemoveAlbumFromTagAsync( Tag fromTag, Album albumToRemove )
		{
			// Check if the album is actually tagged
			TaggedAlbum taggedAlbum = fromTag.TaggedAlbums.SingleOrDefault( tag => ( tag.AlbumId == albumToRemove.Id ) );
			if ( taggedAlbum != null )
			{
				await FilterAccess.DeleteTaggedAlbumAsync( taggedAlbum );
				fromTag.TaggedAlbums.Remove( taggedAlbum );

				await CheckForJustPlayedRemovalAsync( fromTag, albumToRemove );

				// If this tag is synchronised across libraries then remove all instances of this album
				if ( fromTag.Synchronise == true )
				{
					List<TaggedAlbum> otherAlbums = fromTag.TaggedAlbums
						.Where( tag => ( tag.Album.Name == taggedAlbum.Album.Name ) && ( tag.Album.ArtistName == taggedAlbum.Album.ArtistName ) ).ToList();
					foreach ( TaggedAlbum album in otherAlbums )
					{
						await FilterAccess.DeleteTaggedAlbumAsync( album );
						fromTag.TaggedAlbums.Remove( album );

						await CheckForJustPlayedRemovalAsync( fromTag, album.Album );
					}
				}
			}
		}

		/// <summary>
		/// If this Tag is the Just Played tag then clear the Played flag in the album and broadcast this change
		/// </summary>
		/// <param name="fromTag"></param>
		/// <param name="albumId"></param>
		/// <returns></returns>
		private static async Task CheckForJustPlayedRemovalAsync( Tag fromTag, Album albumToClear )
		{
			// If this is the JustPlayed tag and the Played flag is set for the album then clear it
			if ( fromTag == FilterManagementModel.JustPlayedTag )
			{
				albumToClear.Played = false;
				await AlbumAccess.UpdateAlbumAsync( albumToClear, false );

				// Inform interested parties
				new AlbumPlayedStateChangedMessage() { AlbumChanged = albumToClear }.Send();
			}
		}

		/// <summary>
		/// Add a TagedAlbum entry for the album to the tag
		/// </summary>
		/// <param name="toTag"></param>
		/// <param name="albumToAdd"></param>
		private static async Task AddAlbumToTagAsync( Tag toTag, Album albumToAdd, bool dontSynchronise = false )
		{
			// Keep track of whether or not a new entry is required, as it gets complicated
			bool addNewEntry = true;

			// Check whether or not this album is already in the tag. Ensure the library id matches as well
			int index = toTag.TaggedAlbums.FindIndex( tag => ( ( tag.AlbumId == albumToAdd.Id ) && ( tag.Album.LibraryId == albumToAdd.LibraryId ) ) );
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

			// Add a new entry if required
			if ( addNewEntry == true )
			{
				TaggedAlbum newTaggedAlbum = new TaggedAlbum() { TagId = toTag.Id, AlbumId = albumToAdd.Id, Album = albumToAdd };

				await FilterAccess.AddTaggedAlbumAsync( newTaggedAlbum );
				toTag.TaggedAlbums.Add( newTaggedAlbum );
			}

			// If this is the JustPlayed tag and the Played flag is not set for the album then set it now
			if ( toTag == FilterManagementModel.JustPlayedTag )
			{
				if ( albumToAdd.Played == false )
				{
					albumToAdd.Played = true;
					await AlbumAccess.UpdateAlbumAsync( albumToAdd, false );

					// Inform interested parties
					new AlbumPlayedStateChangedMessage() { AlbumChanged = albumToAdd }.Send();
				}
			}

			// If this tag is synchronised across libraries then find any matching albums in the other libraries and add them to the tag if not already present
			// Sychronise whether or not a new entry was added as we may need to reorder an existing tag item in other libraries
			if ( ( dontSynchronise == false ) && ( toTag.Synchronise == true ) )
			{
				foreach ( Library library in FilterManagementModel.Libraries )
				{
					if ( library.Id != albumToAdd.LibraryId )
					{
						// Access this Album from the library as it may not be already be available anywhere, as it is a different library
						Album albumToSynch = await AlbumAccess.GetAlbumInLibraryAsync( albumToAdd.Name, albumToAdd.ArtistName, library.Id );
						if ( albumToSynch != null )
						{
							await AddAlbumToTagAsync( toTag, albumToSynch, true );
						}
					}
				}
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
				// Assume that the album does not need adding to the tag
				bool addTag = false;

				// If this song has just played then it's album will be in the current library and in the set of albums already available in the
				// AlbumsViewModel. So get the associated object from there.
				Album songAlbum = AlbumsViewModel.AlbumLookup[ ( message as SongPlayedMessage ).SongPlayed.AlbumId ];

				// Determine if this album should be marked as having been played
				if ( songAlbum.Played == false )
				{
					// Is this the same album as last time
					if ( songAlbum.Id == FilterManagementModel.JustPlayedAlbumId )
					{
						// Have enough songs been played to tag the album
						addTag = ( ++FilterManagementModel.JustPlayedCount == FilterManagementModel.JustPlayedLimit );
					}
					else
					{
						// Different album so save its identity and reset the count
						FilterManagementModel.JustPlayedAlbumId = songAlbum.Id;
						FilterManagementModel.JustPlayedCount = 1;
					}
				}
				else
				{
					// Record this as the last album played anyway
					FilterManagementModel.JustPlayedAlbumId = songAlbum.Id;

					// And tag it
					addTag = true;
				}

				// If the album has been played, either set just now or previously, add it to the tag (to get the ordering right)
				if ( addTag == true )
				{
					await AddAlbumToTagAsync( FilterManagementModel.JustPlayedTag, songAlbum );

					// Report this filter change
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
		/// Called when a AlbumsDeletedMessage message has been received
		/// Remove the albums from any associated tags
		/// Do not synchronise as this is due to a library scan and not the user removing albums from a tag
		/// </summary>
		/// <param name="message"></param>
		private static async void AlbumsDeleted( object message )
		{
			// Get the list of albums and apply to each tag
			List<int> deletedAlbumIds = ( message as AlbumsDeletedMessage ).DeletedAlbumIds;

			foreach ( Tag tag in FilterManagementModel.Tags )
			{
				// Keep track of whether or not the tag is modified
				bool tagModified = false;

				foreach ( int albumId in deletedAlbumIds )
				{
					// If this tag contains the album then remove the TaggedAlbum item
					TaggedAlbum taggedAlbum = tag.TaggedAlbums.SingleOrDefault( ta => ( ta.AlbumId == albumId ) );
					if ( taggedAlbum != null )
					{
						await FilterAccess.DeleteTaggedAlbumAsync( taggedAlbum );
						tag.TaggedAlbums.Remove( taggedAlbum );
						tagModified = true;
					}
				}

				if ( tagModified == true )
				{
					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { tag.Name } }.Send();
				}
			}
		}

		/// <summary>
		/// Definition of delegate to call when the set of applied tags has been determined
		/// </summary>
		/// <param name="appliedTags"></param>
		public delegate void AppliedTagsDelegate( List<AppliedTag> appliedTags );

		/// <summary>
		/// Definition of delegate to call when a Tag has been updated or created
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