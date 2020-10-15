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
			CurrentFilter = null;
			SortSelector.SetActiveSortOrder( SortSelector.SortType.alphabetic ); // This also sets the sort order to alphabetic ascending
			DataValid = false;
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
		/// The playlists associated with the current library
		/// </summary>
		public static List<Playlist> Playlists { get; set; } = null;

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		public static IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// The current tag being used to filter the artists displayed
		/// </summary>
		public static Tag CurrentFilter { get; set; } = null;

		/// <summary>
		/// Class used to select the artist sort order
		/// </summary>
		public static SortSelector SortSelector { get; } = new SortSelector();

		/// <summary>
		/// List of TagGroups containing currently selected Tags.
		/// A TagGroup only needs to be stored here if some and not all of the tags are selected.
		/// </summary>
		public static List<TagGroup> TagGroups { get; set; } = new List<TagGroup>();

		/// <summary>
		/// Indicates whether or not the data held by the class is valid
		/// </summary>
		public static bool DataValid { get; set; } = false;
	}
}