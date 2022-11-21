using System;
using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The AlbumsViewModel holds the Album data obtained from the AlbumsController
	/// </summary>
	public static class AlbumsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Albums = null;
			UnfilteredAlbums = null;
			FilteredAlbums = null;
			GenreSortedAlbums = null;
			LibraryId = -1;
		}

		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// The list of albums that is available to be displayed. These will have been filtered and sorted according to the current sort order and 
		/// filter settings. This will be set to either the FilteredAlbums, or the GenreSortedAlbums.
		/// </summary>
		public static List<Album> Albums { get; set; } = null;

		/// <summary>
		/// These are the filtered albums that have not had a Genre sort order applied, see below
		/// </summary>
		public static List<Album> FilteredAlbums { get; set; } = null;

		/// <summary>
		/// These are the filtered albums that have had a Genre sort order applied to them. Any album that has multiple genres will have multiple 
		/// entries in this list
		/// </summary>
		public static List<Album> GenreSortedAlbums { get; set; } = null;

		/// <summary>
		/// List of genres for each album ordered their index in GenreSortedAlbums
		/// </summary>
		public static List< string > AlbumIndexToGenreLookup { get; set; } = null;

		/// <summary>
		/// The list of albums for the current library that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Album> UnfilteredAlbums { get; set; } = null;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		public static FilterSelectionModel FilterSelection { get; } = new FilterSelectionModel();

		public static SortSelectionModel SortSelection { get; } = new SortSelectionModel();
	}
}
