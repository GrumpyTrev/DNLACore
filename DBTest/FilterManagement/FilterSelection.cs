using Android.Views;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBTest
{
	/// <summary>
	/// The FilterSelection class controls the selection of filters to be applied
	/// </summary>
	public class FilterSelection
	{
		/// <summary>
		/// Constructor with callback delegate
		/// </summary>
		/// <param name="selectionCallback"></param>
		public FilterSelection( FilterSelectionDelegate selectionCallback ) => selectionDelegate = selectionCallback;

		public void BindToMenu( IMenuItem item )
		{
			boundMenuItem = item;

			// Allow a null item to be passed in which case nothing is bound
			if ( boundMenuItem != null )
			{
				DisplayFilterIcon();
			}
		}

		/// <summary>
		/// Allow the user to pick one of the available Tags as a filter, or to turn off filtering
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter() => FilterSelectionDialogFragment.ShowFragment( CommandRouter.Manager, CurrentFilter, TagGroups, selectionDelegate );

		/// <summary>
		/// Set the filter icon according to whether or not filtering is in effect
		/// </summary>
		public void DisplayFilterIcon() => boundMenuItem?.SetIcon( ( CurrentFilter == null ) ? Resource.Drawable.filter_off : Resource.Drawable.filter_on );

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
		public string CurrentFilterName { get => CurrentFilter?.Name ?? ""; }

		/// <summary>
		/// Has any kind of filter been specified
		/// </summary>
		public bool FilterApplied { get => ( CurrentFilter != null ) || ( TagGroups.Count > 0 ); }

		/// <summary>
		/// Get the TagOrder flag of the current filter
		/// </summary>
		public bool TagOrderFlag { get => CurrentFilter?.TagOrder ?? false; }

		/// <summary>
		/// Get the UserTag flag of the current filter
		/// </summary>
		public bool UserTagFlag { get => CurrentFilter?.UserTag ?? false; }

		/// <summary>
		/// Does the current filter contain any of the specified tag names
		/// </summary>
		/// <param name="tagNames"></param>
		/// <returns></returns>
		public bool FilterContainsTags( IEnumerable<string> tagNames )
		{
			// Check for the simple tag filter first
			bool containsTags = ( CurrentFilter == null ) ? false : tagNames.Contains( CurrentFilter.Name );

			if ( containsTags == false )
			{
				// Now check all the tags in the groups by first forming a list of all the tags in all the groups 
				IEnumerable<string> bigList = TagGroups.SelectMany( tg => tg.Tags, ( tg, ta ) => ta.Name );

				// And then checking for an intersection
				containsTags = ( bigList.Intersect( tagNames ).Count() == 0 );
			}

			return containsTags;
		}

		/// <summary>
		/// List of TagGroups containing currently selected Tags.
		/// A TagGroup only needs to be stored here if some and not all of the tags are selected.
		/// </summary>
		public List<TagGroup> TagGroups { get; set; } = new List<TagGroup>();

		/// <summary>
		/// The menu item bound to this class
		/// </summary>
		private IMenuItem boundMenuItem = null;

		/// <summary>
		/// Delegate used to report back the result of the filter selection
		/// </summary>
		/// <param name="newFilter"></param>
		public delegate void FilterSelectionDelegate( Tag newFilter );

		/// <summary>
		/// The FilterSelectionDelegate to be used for this instance
		/// </summary>
		private FilterSelectionDelegate selectionDelegate = null;
	}
}