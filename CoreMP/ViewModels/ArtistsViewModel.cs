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
			FastScrollSections = null;
			FastScrollSectionLookup = null;
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

		//
		// The following two collections are used for fast scrolling
		// The Adapter requires 3 things
		// 1) A list of all the section names in display order
		// 2) The starting index for each section
		// 3) The section index for each album
		//
		// The section names can be obtained from FastScrollSections
		// The starting index for a section is FastScrollSections[ sectionIndex ]
		// The section index for an album is FastScrollSectionLookup[ albumIndex ]
		//
		/// <summary>
		/// Lookup table specifying the strings used when fast scrolling, and the index into the ArtistsAndAlbums collection
		/// </summary>
		public static List<Tuple<string, int>> FastScrollSections = null;

		/// <summary>
		/// Array of the section indexes associated with each ArtistsAndAlbums entry
		/// </summary>
		public static int[] FastScrollSectionLookup = null;

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
