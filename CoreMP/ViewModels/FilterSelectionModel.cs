using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreMP
{
	/// <summary>
	/// The FilterSelectionModel class holds filter selection data
	/// </summary>
	public class FilterSelectionModel
	{
		/// <summary>
		/// Return a string representation of the current filter
		/// </summary>
		/// <returns></returns>
		public string TabString()
		{
			string tabString = "";
			if ( FilterApplied == true )
			{
				StringBuilder tabStringBuilder = new StringBuilder();

				tabStringBuilder.Append( ( CurrentFilter == null ) ? "\r\n" : $"\r\n[{CurrentFilter.ShortName}]" );
				TagGroups.ForEach( tg => tabStringBuilder.Append( $"[{tg.Name}]" ) );

				tabString = tabStringBuilder.ToString();
			}

			return tabString;
		}

		/// <summary>
		/// Combine the simple Tag and groups of tags together to provide a set of AlbumIds to be applied
		/// </summary>
		/// <returns></returns>
		public HashSet<int> CombineAlbumFilters()
		{
			// If any group tags have been selected combine their selected TaggedAlbum items together
			List<TaggedAlbum> albumsInFilter = new List<TaggedAlbum>();

			// It is possible that the combination of filters results in no albums, so keep track of this
			bool noMatchingAlbums = false;

			if ( TagGroups.Count > 0 )
			{
				foreach ( TagGroup group in TagGroups )
				{
					// Get the TaggedAlbum entries from all the Tags in this group
					List<TaggedAlbum> groupAlbums = group.Tags.SelectMany( ta => ta.TaggedAlbums ).Distinct().ToList();

					// If this is the first group then simply copy its albums to the collection being accumulated
					if ( albumsInFilter.Count == 0 )
					{
						albumsInFilter.AddRange( groupAlbums );
					}
					else
					{
						// AND together the albums already accumulated with the albums in this group
						albumsInFilter = albumsInFilter.Intersect( groupAlbums ).ToList();
					}
				}

				noMatchingAlbums = ( albumsInFilter.Count == 0 );
			}

			if ( noMatchingAlbums == false )
			{
				// If there is a simple filter then combine it with the accumulated albums
				if ( CurrentFilter != null )
				{
					if ( albumsInFilter.Count == 0 )
					{
						albumsInFilter.AddRange( CurrentFilter.TaggedAlbums );
					}
					else
					{
						// AND together the albums already accumulated with the albums in this group
						albumsInFilter = albumsInFilter.Intersect( CurrentFilter.TaggedAlbums ).ToList();
					}
				}
			}

			return albumsInFilter.Select( ta => ta.AlbumId ).ToHashSet();
		}

		/// <summary>
		/// The current tag being used to filter
		/// </summary>
		public Tag CurrentFilter { get; set; } = null;

		/// <summary>
		/// Return the name of the current filter
		/// </summary>
		/// <returns></returns>
		public string CurrentFilterName => CurrentFilter?.Name ?? "";

		/// <summary>
		/// Has any kind of filter been specified
		/// </summary>
		public bool FilterApplied => ( CurrentFilter != null ) || ( TagGroups.Count > 0 );

		/// <summary>
		/// Get the TagOrder flag of the current filter
		/// </summary>
		public bool TagOrderFlag => CurrentFilter?.TagOrder ?? false;

		/// <summary>
		/// Does the current filter contain any of the specified tag names
		/// </summary>
		/// <param name="tagNames"></param>
		/// <returns></returns>
		public bool FilterContainsTags( IEnumerable<string> tagNames )
		{
			// Check for the simple tag filter first
			bool containsTags = ( CurrentFilter != null ) && ( tagNames.Contains( CurrentFilter.Name ) == true );

			if ( containsTags == false )
			{
				// Now check all the tags in the groups by first forming a list of all the tags in all the groups 
				IEnumerable<string> bigList = TagGroups.SelectMany( tg => tg.Tags, ( tg, ta ) => ta.Name );

				// And then checking for an intersection
				containsTags = ( bigList.Intersect( tagNames ).Count() != 0 );
			}

			return containsTags;
		}

		/// <summary>
		/// Return a list of the names of all the tags.
		/// </summary>
		public List<string> GetTagNames() => Tags.TagsCollection.Select( tag => tag.Name ).ToList();

		/// <summary>
		/// List of TagGroups containing currently selected Tags.
		/// A TagGroup only needs to be stored here if some and not all of the tags are selected.
		/// </summary>
		public List<TagGroup> TagGroups { get; set; } = new List<TagGroup>();
	}
}
