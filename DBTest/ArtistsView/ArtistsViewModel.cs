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
			AlphaIndex.Clear();
			LibraryId = -1;
			ListViewState = null;
			CurrentFilter = null;
			CurrentSortOrder = AlbumSortSelector.AlbumSortOrder.alphaAscending;
			DataValid = false;
		}

		/// <summary>
		/// The list of artists that has been obtained from the database
		/// </summary>
		public static List<Artist> Artists { get; set; } = new List<Artist>();

		/// <summary>
		/// The list of artists that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Artist> UnfilteredArtists { get; set; } = new List<Artist>();

		/// <summary>
		/// THe list of all ArtistAlbum items
		/// </summary>
		public static List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();

		/// <summary>
		/// Index into the list of Artists
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
		/// The current sort order for the displayed Artists
		/// </summary>
		public static AlbumSortSelector.AlbumSortOrder CurrentSortOrder { get; set; } = AlbumSortSelector.AlbumSortOrder.alphaAscending;

		/// <summary>
		/// Indicates whether or not the data held by the class is valid
		/// </summary>
		public static bool DataValid { get; set; } = false;
	}
}