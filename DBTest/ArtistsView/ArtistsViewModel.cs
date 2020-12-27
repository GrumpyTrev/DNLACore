using Android.OS;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The ArtistsViewModel holds the Artist data obtained from the ArtistsController
	/// </summary>
	static class ArtistsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Artists.Clear();
			UnfilteredArtists.Clear();
			ArtistsAndAlbums.Clear();
			LibraryId = -1;
			ListViewState = null;
			FilterSelector.CurrentFilter = null;
			FilteredAlbumsIds.Clear();
			SortSelector.SetActiveSortOrder( SortSelector.SortType.alphabetic ); // This also sets the sort order to alphabetic ascending
		}

		/// <summary>
		/// The list of artists that has been obtained from the database. These are sorted and filtered by the user.
		/// </summary>
		public static List<Artist> Artists { get; set; } = new List<Artist>();

		/// <summary>
		/// The list of artists that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Artist> UnfilteredArtists { get; set; } = new List<Artist>();

		/// <summary>
		/// The list of Artists and their associated ArtistAlbum entries to be displayed
		/// </summary>
		public static List<object> ArtistsAndAlbums { get; set; } = new List<object>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		public static IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// Class used to select the artist sort order
		/// </summary>
		public static SortSelector SortSelector { get; } = new SortSelector();

		/// <summary>
		/// The set of filtered Albums formed fromthe current filter (including TagGroups)
		/// </summary>
		public static HashSet<int> FilteredAlbumsIds { get; set; } = new HashSet<int>();
		
		/// <summary>
		/// The FilterSelection used to select and apply filter to the Albums tab
		/// </summary>
		public static FilterSelection FilterSelector { get; } = new FilterSelection( ArtistsController.SetNewFilter );
	}
}