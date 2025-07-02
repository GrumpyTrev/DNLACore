using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The AlbumsViewModel holds the Album data obtained from the AlbumsController
	/// </summary>
	public class AlbumsViewModel
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
		}

		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// The list of albums that is available to be displayed. These will have been filtered and sorted according to the current sort order and 
		/// filter settings. This will be set to either the FilteredAlbums, or the GenreSortedAlbums.
		/// </summary>
		public static List<Album> Albums { get; internal set; } = null;

		/// <summary>
		/// These are the filtered albums that have not had a Genre sort order applied, see below
		/// </summary>
		public static List<Album> FilteredAlbums { get; internal set; } = null;

		/// <summary>
		/// These are the filtered albums that have had a Genre sort order applied to them. Any album that has multiple genres will have multiple 
		/// entries in this list
		/// </summary>
		public static List<Album> GenreSortedAlbums { get; internal set; } = null;

		/// <summary>
		/// List of genres for each album ordered their index in GenreSortedAlbums
		/// </summary>
		public static List< string > AlbumIndexToGenreLookup { get; internal set; } = null;

		/// <summary>
		/// The list of albums for the current library that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Album> UnfilteredAlbums { get; internal set; } = null;

		public static FilterSelectionModel FilterSelection { get; } = new FilterSelectionModel();

		public static SortSelectionModel SortSelection { get; } = new SortSelectionModel();
	}
}
