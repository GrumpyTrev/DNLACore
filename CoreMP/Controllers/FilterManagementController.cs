using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The FilterManagementController class responds to filter selection commands and reflects changes to other controllers
	/// </summary>
	internal class FilterManagementController
	{
		/// <summary>
		/// Register for external filter change messages
		/// </summary>
		public FilterManagementController()
		{
			SongStartedMessage.Register( SongStarted );
			AlbumsDeletedMessage.Register( AlbumsDeleted );
			NotificationHandler.Register( typeof( StorageController ), StorageDataAvailable );
		}

		/// <summary>
		/// Form Tags and associated TaggedAlbum entries for each genre
		/// </summary>
		/// <returns></returns>
		public static async Task FormGenreTagsAsync() => await Task.Run( () =>
		{
			// Create a Tag for each Genre and add them to a lookup table indexed by genre name
			Dictionary<string, Tag> tagLookup = new Dictionary<string, Tag>();

			// Add a tag for unknown genre
			Tag unknownTag = new Tag() { Name = "Unknown", ShortName = "Unknown" };
			tagLookup[ "" ] = unknownTag;

			// Now link in the albums using TaggedAlbum entries
			foreach ( Album album in Albums.AlbumCollection )
			{
				// If this album has no genre then add it to the unknown tag
				if ( album.Genre.Length == 0 )
				{
					unknownTag.AddTaggedAlbum( new TaggedAlbum() { Album = album, AlbumId = album.Id } );
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
							genreTag = new Tag() { Name = genreString, ShortName = genreString };
							tagLookup[ genreString ] = genreTag;
						}

						// Add a TaggedAlbum for this album
						genreTag.AddTaggedAlbum( new TaggedAlbum()
                        {
                            Album = album,
                            AlbumId = album.Id
                        } );
					}
				}
			}

			// Create a TagGroup for genres and add it to the model
			TagGroup genres = new TagGroup() { Name = "Genre" };
			FilterManagementModel.TagGroups.Add( genres );

			// Now unload the genre tags into a list and sort it
			genres.Tags = tagLookup.Values.ToList();
			genres.Tags.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			// Display all the tags and the number of albums associated with them
			bool displayTags = true;

			if ( displayTags == true )
			{
				foreach ( Tag tag in genres.Tags )
				{
					Logger.Log( $"Genre [{tag.Name}] albums {tag.TaggedAlbums.Count}" );

					foreach ( TaggedAlbum taggedAlbum in tag.TaggedAlbums )
					{
						Logger.Log( $"Artist: {taggedAlbum.Album.ArtistName} Album: {taggedAlbum.Album.Name} genres {taggedAlbum.Album.Genre}" );
					}
				}
			}
		} );

		/// <summary>
		/// Add a TaggedAlbum entry for the album to the tag
		/// </summary>
		/// <param name="toTag"></param>
		/// <param name="albumToAdd"></param>
		public void AddAlbumToTag( Tag toTag, Album albumToAdd, bool synchronise = true )
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
					toTag.AddTaggedAlbum( new TaggedAlbum() { AlbumId = albumToAdd.Id, Album = albumToAdd } );
				}
			}
			else
			{
				toTag.AddTaggedAlbum( new TaggedAlbum() { AlbumId = albumToAdd.Id, Album = albumToAdd } );
			}

			// If this is the JustPlayed tag remove the album from the Not Played tag and check for the album's Played flag
			if ( toTag == FilterManagementModel.JustPlayedTag )
			{
				// Make sure that the Played flag is set
				if ( albumToAdd.Played == false )
				{
					albumToAdd.Played = true;
				}

				RemoveAlbumFromTag( FilterManagementModel.NotPlayedTag, albumToAdd );
			}

			// If this tag is synchronised across libraries then find any matching albums in the other libraries and add them to the tag if not already present
			// Sychronise whether or not a new entry was added as we may need to reorder an existing tag item in other libraries
			if ( ( synchronise == true ) && ( toTag.Synchronise == true ) )
			{
				foreach ( Library library in Libraries.LibraryCollection )
				{
					if ( library.Id != albumToAdd.LibraryId )
					{
						// Access this Album from the library
						Album albumToSynch = Albums.GetAlbumInLibrary( albumToAdd.Name, albumToAdd.ArtistName, library.Id );
						if ( albumToSynch != null )
						{
							AddAlbumToTag( toTag, albumToSynch, false );
						}
					}
				}
			}

			// Report this tag change. Only report on the firest call, not on synchonisation
			if ( synchronise == true )
			{
				new TagMembershipChangedMessage() { ChangedTags = new List<string>() { toTag.Name } }.Send();
			}
		}

		/// <summary>
		/// Remove the TaggedAlbum associated with the album from the tag
		/// </summary>
		/// <param name="fromTag"></param>
		/// <param name="albumId"></param>
		public void RemoveAlbumFromTag( Tag fromTag, Album albumToRemove )
		{
			// Check if the album is actually tagged
			TaggedAlbum taggedAlbum = fromTag.TaggedAlbums.SingleOrDefault( tag => ( tag.AlbumId == albumToRemove.Id ) );
			if ( taggedAlbum != null )
			{
				// If this tag is synchronised across libraries then remove all instances of this album
				if ( fromTag.Synchronise == true )
				{
					fromTag.DeleteTaggedAlbums( fromTag.TaggedAlbums.
						Where( tag => ( tag.Album.Name == taggedAlbum.Album.Name ) && ( tag.Album.ArtistName == taggedAlbum.Album.ArtistName ) ).ToList() );
				}
				else
				{
					// Just delete the single TaggedAlbum
					fromTag.DeleteTaggedAlbum( taggedAlbum );
				}

				// If this is the JustPlayed tag then clear the Played flag and add the album to the Not Played tag
				if ( fromTag == FilterManagementModel.JustPlayedTag )
				{
					albumToRemove.Played = false;
					AddAlbumToTag( FilterManagementModel.NotPlayedTag, albumToRemove );
				}

				new TagMembershipChangedMessage() { ChangedTags = new List<string>() { fromTag.Name } }.Send();
			}
		}

		/// <summary>
		/// Synchronise the 'played' status of albums across all the libraries
		/// </summary>
		public void SynchroniseAlbumPlayedStatus()
		{
			// Find all the unique Album/Artist name combinations associated with the JustPlayedTag
			List<(string Name, string ArtistName)> distinctAlbums = 
				FilterManagementModel.JustPlayedTag.TaggedAlbums.Select( tagged => (tagged.Album.Name, tagged.Album.ArtistName) ).Distinct().ToList();

			// Now check that each distinct album is tagged in each library, if present in the library
			foreach ( (string Name, string ArtistName) in distinctAlbums )
			{
				// If there are as many tagged albums with matching name and artist name as there are libraries then no work is required
				IEnumerable<TaggedAlbum> matchingAlbums = FilterManagementModel.JustPlayedTag.TaggedAlbums
					.Where( tagged => ( tagged.Album.Name == Name ) && ( tagged.Album.ArtistName == ArtistName ) );

				if ( matchingAlbums.Count() != Libraries.LibraryCollection.Count )
				{
					// Need to work out which albums are missing and then check if that album is actually in its library
					foreach ( Library library in Libraries.LibraryCollection )
					{
						if ( matchingAlbums.FirstOrDefault( tagged => ( tagged.Album.LibraryId == library.Id ) ) == null )
						{
							// No tag found for the album in the current library.
							// Does the album/artist combination exist in the library
							Album albumToTag = Albums.GetAlbumInLibrary( Name, ArtistName, library.Id );
							if ( albumToTag != null )
							{
								AddAlbumToTag( FilterManagementModel.JustPlayedTag, albumToTag, false );
							}
						}
					}
				}
			}

		}

		/// <summary>
		/// Called during startup when data is available from storage
		/// </summary>
		/// <param name="message"></param>
		private async void StorageDataAvailable()
		{
			// Extract the 'system' tags from this list for easy access later
			FilterManagementModel.JustPlayedTag = Tags.GetTagByName( JustPlayedTagName );

			// Do the linking of TaggedAlbums off the UI thread
			await LinkInTaggedAlbums();

			// Create the Not Played tag from the Just Played tag
			await CreateNotPlayedTagAsync();
		}

		/// <summary>
		/// Link the TaggedAlbum entries to their Tags and set the Album entry in the TaggedAlbum  
		/// </summary>
		/// <returns></returns>
		private async Task LinkInTaggedAlbums() => await Task.Run( () =>
		{
			// Tags indexed by their ids
			Dictionary<int, Tag> tagLookup = Tags.TagsCollection.ToDictionary( tag => tag.Id );

			// Keep track of any TaggedAlbums that are not associated with valid tags
			List<TaggedAlbum> lostTags = new List<TaggedAlbum>();

			foreach ( TaggedAlbum taggedAlbum in TaggedAlbums.TaggedAlbumCollection )
			{
				taggedAlbum.Album = Albums.GetAlbumById( taggedAlbum.AlbumId );

				if ( taggedAlbum.Album == null )
				{
					lostTags.Add( taggedAlbum );
				}
				else
				{
					Tag tag = tagLookup.GetValueOrDefault( taggedAlbum.TagId );
					if ( tag != null )
					{
						tagLookup[ taggedAlbum.TagId ].TaggedAlbums.Add( taggedAlbum );
					}
					else
					{
						lostTags.Add( taggedAlbum );
					}
				}
			};

			// Delete the lost TaggedAlbum entries
			foreach ( TaggedAlbum taggedAlbum in lostTags )
			{
				TaggedAlbums.DeleteTaggedAlbum( taggedAlbum );
			}
		} );

		/// <summary>
		/// Create the NotPlayed tag based on the Just Played tag
		/// </summary>
		/// <returns></returns>
		private async Task CreateNotPlayedTagAsync() => await Task.Run( () =>
		{
			// Create a new Not Played tag
			FilterManagementModel.NotPlayedTag = new Tag() { Name = NotPlayedTagName, ShortName = NotPlayedTagName, Synchronise = true };

			// No need to wait for this as it is not persisted
			Tags.AddTagAsync( FilterManagementModel.NotPlayedTag );

			// Now add all albums to this tag that are not in the Just Played tag
			HashSet<int> justPlayedAlbumIds = FilterManagementModel.JustPlayedTag.TaggedAlbums.Select( ta => ta.AlbumId ).ToHashSet();

			foreach ( Album album in Albums.AlbumCollection )
			{
				if ( justPlayedAlbumIds.Contains( album.Id ) == false )
				{
					FilterManagementModel.NotPlayedTag.AddTaggedAlbum( new TaggedAlbum() { Album = album, AlbumId = album.Id } );
				}
			}
		} );

		/// <summary>
		/// Called when the SongStartedMessage is received
		/// Add the associated album to the Just Played tag 
		/// </summary>
		/// <param name="message"></param>
		private void SongStarted( Song songPlayed )
		{
			// Assume that the album does not need adding to the tag
			bool addTag = false;

			// Get the Album from the Albums collection
			Album songAlbum = Albums.GetAlbumById( songPlayed.AlbumId );

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
			}
		}

		/// <summary>
		/// Called when a AlbumsDeletedMessage message has been received
		/// Remove the albums from any associated tags
		/// Do not synchronise as this is due to a library scan and not the user removing albums from a tag
		/// </summary>
		/// <param name="message"></param>
		private void AlbumsDeleted( List<int> deletedAlbums )
		{
			// Get the list of deleted albums and apply to each tag
			HashSet<int> deletedAlbumIds = deletedAlbums.ToHashSet();

			foreach ( Tag tag in Tags.TagsCollection )
			{
				// Get the TaggedAlbum entries that have album ids in the deleted set.
				// Don't use a lazy enumerator here as we'll be deleting entries from the collection being enumerated
				List<TaggedAlbum> taggedAlbums = tag.TaggedAlbums.Where( ta => ( deletedAlbumIds.Contains( ta.AlbumId ) ) ).ToList();

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
		/// The name given to the "Just played" tag
		/// </summary>
		public const string JustPlayedTagName = "Just played";

		/// <summary>
		/// The name given to the "Not played" tag
		/// </summary>
		public const string NotPlayedTagName = "Not played";
	}
}
