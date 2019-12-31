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
		/// Apply the list of selected albums to the Tags data to produce a list of all available tags and an indication of 
		/// whether the tag has been applied to none, all or some of the albums
		/// </summary>
		/// <param name="selectedAlbums"></param>
		public static async void GetAppliedTagsAsync( List<ArtistAlbum> selectedAlbums, AppliedTagsDelegate tagsDelegate )
		{
			List<AppliedTag> appliedTags = new List<AppliedTag>();

			await Task.Run( () => {
				foreach ( Tag tag in FilterManagementModel.Tags )
				{
					AppliedTag appliedTag = new AppliedTag() { TagName = tag.Name };

					// Check if all, none or some of the selected albums are tagged with this tag
					int taggedCount = 0;
					foreach ( ArtistAlbum selectedAlbum in selectedAlbums )
					{
						if ( tag.TaggedAlbums.Exists( artistAlbum => ( artistAlbum.AlbumId == selectedAlbum.AlbumId ) ) == true )
						{
							taggedCount++;
						}
					}

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
		public static async void ApplyTagsAsync( List<ArtistAlbum> selectedAlbums, List<AppliedTag> appliedTags )
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
							selectedAlbums.ForEach( async selectedAlbum => await RemoveAlbumFromTagAsync( changedTag, selectedAlbum.AlbumId ) );
						}
						else if ( appliedTag.Applied == AppliedTag.AppliedType.All )
						{
							// Add the selected albums to this tag
							selectedAlbums.ForEach( async selectedAlbum => await AddAlbumToTagAsync( changedTag, selectedAlbum.Album ) );
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
		/// The name given to the "Just played" tag
		/// </summary>
		public const string JustPlayedTagName = "Just played";

		/// <summary>
		/// The name given to the "Recently added" tag
		/// </summary>
		public const string RecentlyAddedTagName = "Recently added";
	}
}