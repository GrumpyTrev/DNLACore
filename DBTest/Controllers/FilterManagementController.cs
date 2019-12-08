using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The FilterManagementController class responds to filter selection commands and refects changes to other controllers
	/// </summary>
	public static class FilterManagementController
	{
		/// <summary>
		/// Get the Tag data from the database
		/// If the data has already been obtained then do nothing
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		public static async void GetTagsAsync()
		{
			// Check if the tags have already been obtained
			if ( FilterManagementModel.Tags == null )
			{
				// New data is required
				FilterManagementModel.Tags = await FilterAccess.GetTagsAsync();
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

			await Task.Run( () => {
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
							selectedAlbums.ForEach( selectedAlbum => FilterAccess.RemoveTaggedAlbumAsync( changedTag, selectedAlbum ) );
						}
						else if ( appliedTag.Applied == AppliedTag.AppliedType.All )
						{
							// Add the selected albums to this tag
							selectedAlbums.ForEach( selectedAlbum => FilterAccess.AddTaggedAlbumAsync( changedTag, selectedAlbum ) );
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
		/// Definition of delegate to call when the set of applied tags has been determined
		/// </summary>
		/// <param name="appliedTags"></param>
		public delegate void AppliedTagsDelegate( List<AppliedTag> appliedTags );
	}
}