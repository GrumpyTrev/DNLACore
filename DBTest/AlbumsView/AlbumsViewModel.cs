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
			UnfilteredAlbums.Clear();
			LibraryId = -1;
			ListViewState = null;
			FilterSelector.CurrentFilter = null;
		}

		/// <summary>
		/// The list of albums that has been obtained from the database (may be sorted or filtered)
		/// </summary>
		public static List<Album> Albums { get; set; } = new List<Album>();

		/// <summary>
		/// The list of albums for the current library that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Album> UnfilteredAlbums { get; set; } = new List<Album>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		public static IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// Class used to select the album sort order
		/// </summary>
		public static SortSelector SortSelector { get; } = new SortSelector();

		/// <summary>
		/// The FilterSelection used to select and apply filter to the Albums tab
		/// </summary>
		public static FilterSelection FilterSelector { get; } = new FilterSelection( AlbumsController.SetNewFilter );
	}
}