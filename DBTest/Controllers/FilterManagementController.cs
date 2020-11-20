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
			Mediator.RegisterPermanent( AlbumsDeleted, typeof( AlbumsDeletedMessage ) );
		}

		/// <summary>
		/// Get the Tag data from storage
		/// </summary>
		public static void GetTags()
		{
			// The Tags need to be linked to their TaggedAlbum entries which contain Albums, so wait
			// until the Album data is available
			StorageController.RegisterInterestInDataAvailable( DataAvailable );
		}

		/// <summary>
		/// Return a list of the names of all the tags
		/// </summary>
		/// <returns></returns>
		public static List<string> GetTagNames() => Tags.TagsCollection?.Select( tag => tag.Name ).ToList() ?? new List<string>();

		/// <summary>
		/// Return the Tag with the given name, or null if no such Tag
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tag GetTagFromName( string name ) => Tags.GetTagByName( name );

		/// <summary>
		/// Update an existing Tag with updated values.
		/// Remove TaggedAlbums if the MaxCount for a library has now been exceeded
		/// </summary>
		/// <param name="existingTag"></param>
		/// <param name="updatedTag"></param>
		/// <param name="tagDelegate"></param>
		public static void UpdateTag( Tag existingTag, Tag updatedTag, TagUpdatedDelegate tagDelegate )
		{
			// Check if the new name and short name are valid
			bool updateOk = Tags.CheckNamesAreValid( updatedTag.Name, updatedTag.ShortName, existingTag ) ;

			if ( updateOk == true )
			{
				// No problems in performing the update.
				// Has the maximum number changed and do we need to do anything about it?
				if ( updatedTag.MaxCount != existingTag.MaxCount )
				{
					if ( ( updatedTag.MaxCount != -1 ) && ( ( existingTag.MaxCount == -1 ) || ( updatedTag.MaxCount < existingTag.MaxCount ) ) )
					{
						// The maximum count setting has been reduced, so we may need to remove TagedAlbum entries
						ReduceTaggedAlbumNumbers( existingTag, updatedTag.MaxCount );
					}
				}

				// If the synchronise libraries flag has just been set then attempt to synchronise the contents of this tag
				// across all libraries
				if ( ( updatedTag.Synchronise != existingTag.Synchronise ) && ( updatedTag.Synchronise == true ) )
				{
					SynchroniseTagAcrossLibraries( existingTag );
				}

				// Update the details for the existing Tag and save it
				existingTag.UpdateTagDetails( updatedTag );
			}

			tagDelegate( updateOk );
		}

		/// <summary>
		/// Create a new tag
		/// </summary>
		/// <param name="newTag"></param>
		/// <param name="tagDelegate"></param>
		public static void CreateTag( Tag newTag, TagUpdatedDelegate tagDelegate )
		{
			// Check if the new name and short name are valid
			bool createdOk = Tags.CheckNamesAreValid( newTag.Name, newTag.ShortName );

			if ( createdOk == true )
			{
				newTag.UserTag = true;
				Tags.AddTag( newTag );
			}

			tagDelegate( createdOk );
		}

		/// <summary>
		/// Apply the list of selected albums to the Tags data to produce a list of all available tags and an indication of 
		/// whether the tag has been applied to none, all or some of the albums
		/// </summary>
		/// <param name="selectedAlbums"></param>
		public static async void GetAppliedTagsAsync( IEnumerable<Album> selectedAlbums, AppliedTagsDelegate tagsDelegate )
		{
			List<AppliedTag> appliedTags = new List<AppliedTag>();

			await Task.Run( () => {

				// Form a lookup table for the selected album ids
				HashSet<int> albumLookup = selectedAlbums.Select( alb => alb.Id ).ToHashSet();

				foreach ( Tag tag in Tags.TagsCollection )
				{
					// Check if all, none or some of the selected albums are tagged with this tag
					int taggedCount = tag.TaggedAlbums.Count( artistAlbum => albumLookup.Contains( artistAlbum.AlbumId ) );

					AppliedTag appliedTag = new AppliedTag()
					{
						TagName = tag.Name,
						// Set the Applied value according to the taggedCount
						Applied = ( taggedCount == 0 ) ? AppliedTag.AppliedType.None :
							( ( taggedCount == selectedAlbums.Count() ) ? AppliedTag.AppliedType.All : AppliedTag.AppliedType.Some )
					};

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
		public static async void ApplyTagsAsync( IEnumerable<Album> selectedAlbums, List<AppliedTag> appliedTags )
		{
			// Keep track of which tags have been changed so that this can be sent to other controllers
			List<string> changedTags = new List<string>();

			await Task.Run( () =>
			{
				foreach ( AppliedTag appliedTag in appliedTags )
				{
					// Has there been a change to this tag
					if ( appliedTag.Applied != appliedTag.OriginalApplied )
					{
						changedTags.Add( appliedTag.TagName );

						// Find the tag record
						Tag changedTag = GetTagFromName( appliedTag.TagName );

						if ( appliedTag.Applied == AppliedTag.AppliedType.None )
						{
							// Remove the selected albums from this tag
							foreach ( Album selectedAlbum in selectedAlbums )
							{
								RemoveAlbumFromTag( changedTag, selectedAlbum );
							}
						}
						else if ( appliedTag.Applied == AppliedTag.AppliedType.All )
						{
							// Add the selected albums to this tag
							foreach ( Album selectedAlbum in selectedAlbums )
							{
								AddAlbumToTag( changedTag, selectedAlbum );
							}
						}
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
		public static void DeleteTag( Tag tagToDelete ) => Tags.DeleteTag( tagToDelete );

		/// <summary>
		/// Called during startup when data is available from storage
		/// </summary>
		/// <param name="message"></param>
		private static async void DataAvailable( object message )
		{
			// Extract the 'system' tags from this list for easy access later
			FilterManagementModel.JustPlayedTag = Tags.GetTagByName( JustPlayedTagName );

			// Do the linking of TaggedAlbums off the UI thread
			await LinkInTaggedAlbums();

			// Generate tags for all the genres
			await FormGenreTagsAsync();
		}

		/// <summary>
		/// Link the TaggedAlbum entries to their Tags and set the Album entry in the TaggedAlbum  
		/// </summary>
		/// <returns></returns>
		private static async Task LinkInTaggedAlbums()
		{
			await Task.Run( () =>
			{
				// Link these to their Tags
				Dictionary<int, Tag> tagLookup = Tags.TagsCollection.ToDictionary( tag => tag.Id );

				List<TaggedAlbum> taggedAlbumsToDelete = new List<TaggedAlbum>();

				foreach ( TaggedAlbum taggedAlbum in TaggedAlbums.TaggedAlbumCollection )
				{
					taggedAlbum.Album = Albums.GetAlbumById( taggedAlbum.AlbumId );
					if ( taggedAlbum.Album != null )
					{
						tagLookup[ taggedAlbum.TagId ].TaggedAlbums.Add( taggedAlbum );
					}
					else
					{
						// This tagged album points to an album that no longer exists, don't link it
						taggedAlbumsToDelete.Add( taggedAlbum );
					}
				}

				taggedAlbumsToDelete.ForEach( ta => FilterAccess.DeleteTaggedAlbumAsync( ta ) );
			} );
		}

		/// <summary>
		/// Form Tags and associated TaggedAlbum entries for each genre
		/// </summary>
		/// <returns></returns>
		private static async Task FormGenreTagsAsync()
		{
			await Task.Run( () =>
			{
				// Create a Tag for each Genre and add them to a lookup table indexed by genre name
				Dictionary<string, Tag> tagLookup = new Dictionary<string, Tag>();

				// Add a tag for unknown genre
				Tag unknownTag = new Tag() { Name = "Unknown", UserTag = false, ShortName = "Unknown" };
				tagLookup[ "" ] = unknownTag;

				// Now link in the albums using TaggedAlbum entries
				foreach ( Album album in Albums.AlbumCollection )
				{
					// If this album has no genre then add it to the unknown tag
					if ( album.Genre.Length == 0 )
					{
						unknownTag.TaggedAlbums.Add( new TaggedAlbum() { Album = album, AlbumId = album.Id, TagIndex = unknownTag.TaggedAlbums.Count } );
						Logger.Log( $"Album {album.Name} genre unknown" );
					}
					else
					{
						// Get the Genre string and split it into seperate genre strings
						string[] genreStrings = album.Genre.Split( ';' );
						foreach ( string genreString in genreStrings )
						{
							// Is there is no tag for this genre create one
							Tag genreTag = tagLookup.GetValueOrDefault( genreString );
							if ( genreTag == null )
							{
								genreTag = new Tag() { Name = genreString, UserTag = false, ShortName = genreString };
								tagLookup[ genreString ] = genreTag;
								Logger.Log( $"Album {album.Name} new genre {genreString}" );
							}

							// Add a TaggedAlbum for this album
							genreTag.TaggedAlbums.Add( new TaggedAlbum() { Album = album, AlbumId = album.Id, TagIndex = genreTag.TaggedAlbums.Count } );
						}
					}
				}

				// Create a TagGroup for genres and add it to the model
				TagGroup genres = new TagGroup() { Name = "Genre" };
				FilterManagementModel.TagGroups.Add( genres );

				// Now unload the genre tags into a list and sort it
				genres.Tags = tagLookup.Values.ToList();
				genres.Tags.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );
			} );
		}

		/// <summary>
		/// Synchronise the Tag across all libraries.
		/// Determine the unique Album/Artist combinations in the Tag and ensure that each combination is tagged in all libraries
		/// </summary>
		/// <param name="tagToSynchronise"></param>
		private static void SynchroniseTagAcrossLibraries( Tag tagToSynchronise )
		{
			// Find all the unique Album/Artist name combinations associated with the tag
			var distinctAlbums = tagToSynchronise.TaggedAlbums.Select( tagged => new { tagged.Album.Name, tagged.Album.ArtistName } ).Distinct();

			// Now check that each distinct album is tagged in each library, if present in the library
			foreach ( var distinctAlbum in distinctAlbums )
			{
				// If there are as many tagged albums with matching name and artist name as there are libraries then no work is required
				IEnumerable<TaggedAlbum> matchingAlbums = tagToSynchronise.TaggedAlbums
					.Where( tagged => ( tagged.Album.Name == distinctAlbum.Name ) && ( tagged.Album.ArtistName == distinctAlbum.ArtistName ) );

				if ( matchingAlbums.Count() != Libraries.LibraryCollection.Count )
				{
					// Need to work out which albums are missing and then check if that album is actually in its library
					foreach ( Library library in Libraries.LibraryCollection )
					{
						if ( matchingAlbums.FirstOrDefault( tagged => ( tagged.Album.LibraryId == library.Id ) ) == null )
						{
							// No tag found for the album in the current library.
							// Does the album/artist combination exist in the library
							Album albumToTag = Albums.GetAlbumInLibrary( distinctAlbum.Name, distinctAlbum.ArtistName, library.Id );
							if ( albumToTag != null )
							{
								tagToSynchronise.AddTaggedAlbum( new TaggedAlbum() { TagId = tagToSynchronise.Id, AlbumId = albumToTag.Id, Album = albumToTag } );

								// If this is the JustPlayed tag and the Played flag is not set for the album then set it now
								CheckAlbumAddedToJustPlayingTag( tagToSynchronise, albumToTag );
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Reduce the number of TaggedAlbum entries for a Tag if the count for any library excees the new maximum count
		/// </summary>
		/// <param name="tagToReduce"></param>
		/// <param name="newMaxCount"></param>
		private static void ReduceTaggedAlbumNumbers( Tag tagToReduce, int newMaxCount )
		{
			// Look at the TaggedAlbums in each library in turn
			foreach ( Library library in Libraries.LibraryCollection )
			{
				// Get the TaggedAlbums for this library
				IEnumerable<TaggedAlbum> taggedAlbums = tagToReduce.TaggedAlbums.Where( ta => ta.Album.LibraryId == library.Id );

				// Count exceeded?
				int tagCount = taggedAlbums.Count();
				if ( tagCount > newMaxCount )
				{
					// Need to remove the first (tagCount - newMaxCount) entries from the Tag for this library
					tagToReduce.DeleteTaggedAlbums( taggedAlbums.Take( tagCount - newMaxCount ) );
				}
			}
		}

		/// <summary>
		/// If this is the JustPlayed tag and the Played flag is not set for the album then set it now
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="album"></param>
		private static void CheckAlbumAddedToJustPlayingTag( Tag tag, Album album )
		{
			if ( tag == FilterManagementModel.JustPlayedTag )
			{
				if ( album.Played == false )
				{
					album.SetPlayedFlag( true );
				}
			}
		}

		/// <summary>
		/// Remove the TaggedAlbum associated with the album from the tag
		/// </summary>
		/// <param name="fromTag"></param>
		/// <param name="albumId"></param>
		private static void RemoveAlbumFromTag( Tag fromTag, Album albumToRemove )
		{
			// Check if the album is actually tagged
			TaggedAlbum taggedAlbum = fromTag.TaggedAlbums.SingleOrDefault( tag => ( tag.AlbumId == albumToRemove.Id ) );
			if ( taggedAlbum != null )
			{
				// If this tag is synchronised across libraries then remove all instances of this album
				if ( fromTag.Synchronise == true )
				{
					fromTag.DeleteTaggedAlbums( fromTag.TaggedAlbums.
						Where( tag => ( tag.Album.Name == taggedAlbum.Album.Name ) && ( tag.Album.ArtistName == taggedAlbum.Album.ArtistName ) ) );
				}
				else
				{
					// Just delete the single TaggedAlbum
					fromTag.DeleteTaggedAlbum( taggedAlbum );
				}

				// If this is the JustPlayed tag and the Played flag is set for the album then clear it
				if ( fromTag == FilterManagementModel.JustPlayedTag )
				{
					albumToRemove.SetPlayedFlag( false );
				}
			}
		}

		/// <summary>
		/// Add a TaggedAlbum entry for the album to the tag
		/// </summary>
		/// <param name="toTag"></param>
		/// <param name="albumToAdd"></param>
		private static void AddAlbumToTag( Tag toTag, Album albumToAdd, bool dontSynchronise = false )
		{
			// Get the set of TaggedAlbum entries in this tag for the album's library
			List<TaggedAlbum> tagEntriesInSameLibrary = toTag.TaggedAlbums.Where( ta => ta.Album.LibraryId == albumToAdd.LibraryId ).ToList();

			// Check whether or not this album is already in the tag.
			int index = tagEntriesInSameLibrary.FindIndex( ta => ( ta.AlbumId == albumToAdd.Id ) );
			if ( index != -1 )
			{
				// If this Tag is ordered by tag id and this existing entry is not the most recently added then
				// the existing entry will have to be removed and a new entry added at the end
				if ( ( toTag.TagOrder == true ) && ( index < ( tagEntriesInSameLibrary.Count - 1 ) ) )
				{
					// Can't just move the entry in the list as we are using the entry's id to order the list when loaded
					toTag.DeleteTaggedAlbum( tagEntriesInSameLibrary[ index ] );
					toTag.AddTaggedAlbum( new TaggedAlbum() { TagId = toTag.Id, AlbumId = albumToAdd.Id, Album = albumToAdd } );
				}
			}
			else
			{
				// There is no existing entry. If there is a limit to the number of albums that can be tagged then check if the limit has been reached 
				// and remove the oldest (lowest id) entry
				if ( ( toTag.MaxCount != -1 ) && ( tagEntriesInSameLibrary.Count >= toTag.MaxCount ) )
				{
					// Remove the oldest entry, i.e. the first entry 
					toTag.DeleteTaggedAlbum( tagEntriesInSameLibrary[ 0 ] );
				}

				toTag.AddTaggedAlbum( new TaggedAlbum() { TagId = toTag.Id, AlbumId = albumToAdd.Id, Album = albumToAdd } );
			}

			// If this is the JustPlayed tag and the Played flag is not set for the album then set it now
			CheckAlbumAddedToJustPlayingTag( toTag, albumToAdd );

			// If this tag is synchronised across libraries then find any matching albums in the other libraries and add them to the tag if not already present
			// Sychronise whether or not a new entry was added as we may need to reorder an existing tag item in other libraries
			if ( ( dontSynchronise == false ) && ( toTag.Synchronise == true ) )
			{
				foreach ( Library library in Libraries.LibraryCollection )
				{
					if ( library.Id != albumToAdd.LibraryId )
					{
						// Access this Album from the library as it may not be already be available anywhere, as it is a different library
						Album albumToSynch = Albums.GetAlbumInLibrary( albumToAdd.Name, albumToAdd.ArtistName, library.Id );
						if ( albumToSynch != null )
						{
							AddAlbumToTag( toTag, albumToSynch, true );
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
		private static void SongPlayed( object message )
		{
			// Only process this if the Just Played tag exists
			if ( FilterManagementModel.JustPlayedTag != null )
			{
				// Assume that the album does not need adding to the tag
				bool addTag = false;

				// Get the Album from the Albums collection
				Album songAlbum = Albums.GetAlbumById( ( message as SongPlayedMessage ).SongPlayed.AlbumId );

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

					// And tag it just to get it at the top of the tag
					addTag = true;
				}

				// If the album has been played, either set just now or previously, add it to the tag (to get the ordering right)
				if ( addTag == true )
				{
					AddAlbumToTag( FilterManagementModel.JustPlayedTag, songAlbum );

					// Report this filter change
					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { JustPlayedTagName } }.Send();
				}
			}
		}

		/// <summary>
		/// Called when a AlbumsDeletedMessage message has been received
		/// Remove the albums from any associated tags
		/// Do not synchronise as this is due to a library scan and not the user removing albums from a tag
		/// </summary>
		/// <param name="message"></param>
		private static void AlbumsDeleted( object message )
		{
			// Get the list of deleted albums and apply to each tag
			HashSet<int> deletedAlbumIds = ( message as AlbumsDeletedMessage ).DeletedAlbumIds.ToHashSet();

			foreach ( Tag tag in Tags.TagsCollection )
			{
				// Get the TaggedAlbum entries that have album ids in the deleted set
				IEnumerable<TaggedAlbum> taggedAlbums = tag.TaggedAlbums.Where( ta => ( deletedAlbumIds.Contains( ta.AlbumId ) ) );

				// Delete all these
				if ( taggedAlbums.Count() > 0 )
				{
					tag.DeleteTaggedAlbums( taggedAlbums );
					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { tag.Name } }.Send();
				}
			}

			// Now remove these Albums from the group tags
			foreach ( TagGroup group in FilterManagementModel.TagGroups )
			{
				List<Tag> removedTags = new List<Tag>();

				foreach ( Tag tag in group.Tags )
				{
					// Get the set of TaggedAlbum entries in this tag that refer to any deleted album and delete them from the tag
					tag.TaggedAlbums.Where( ta => deletedAlbumIds.Contains( ta.AlbumId ) == true ).ToList().ForEach( ta => tag.TaggedAlbums.Remove( ta ) );

					if ( tag.TaggedAlbums.Count == 0 )
					{
						removedTags.Add( tag );
					}
				}

				if ( removedTags.Count > 0 )
				{
					removedTags.ForEach( tag => group.Tags.Remove( tag ) );

					new TagMembershipChangedMessage() { ChangedTags = new List<string>() { group.Name } }.Send();
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
	}
}