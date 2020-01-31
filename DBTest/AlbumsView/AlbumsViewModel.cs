using Android.OS;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AlbumsViewModel holds the Album data obtained from the AlbumsController
	/// </summary>
	static class AlbumsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Albums.Clear();
			AlphaIndex.Clear();
			UnfilteredAlbums.Clear();
			LibraryId = -1;
			ListViewState = null;
			CurrentFilter = null;
			SortSelector.CurrentSortOrder = AlbumSortSelector.AlbumSortOrder.alphaAscending;
		}

		/// <summary>
		/// The list of albums that has been obtained from the database (may be sorted or filtered)
		/// </summary>
		public static List<Album> Albums { get; set; } = new List<Album>();

		/// <summary>
		/// The list of albums that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Album> UnfilteredAlbums { get; set; } = new List<Album>();

		/// <summary>
		/// Index into the list of Albums
		/// </summary>
		public static Dictionary<string, int> AlphaIndex { get; set; } = new Dictionary<string, int>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// The names of the playlists associated with the current library
		/// </summary>
		public static List<string> PlaylistNames { get; set; } = null;

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		public static IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// The current tag being used to filter the artists displayed
		/// </summary>
		public static Tag CurrentFilter { get; set; } = null;

		/// <summary>
		/// Class used to select the album sort order
		/// </summary>
		public static AlbumSortSelector SortSelector { get; } = new AlbumSortSelector();
	}
}