using System;
using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The ArtistsViewModel holds the Artist data obtained from the ArtistsController
	/// </summary>
	public static class ArtistsViewModel
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
			FilteredAlbumsIds.Clear();
			SortSelection.ActiveSortType = SortType.alphabetic; // This also sets the sort order to alphabetic ascending
		}

		public static ModelAvailable Available { get; } = new ModelAvailable();

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
		/// The set of filtered Albums formed fromthe current filter (including TagGroups)
		/// </summary>
		public static HashSet<int> FilteredAlbumsIds { get; set; } = new HashSet<int>();
		
		public static FilterSelectionModel FilterSelection { get; } = new FilterSelectionModel();

		public static SortSelectionModel SortSelection { get; } = new SortSelectionModel();
	}
}
