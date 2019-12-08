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
		/// The list of artists that has been obtained from the database
		/// </summary>
		public static List<Album> Albums { get; set; } = null;

		/// <summary>
		/// Index into the list of Artists
		/// </summary>
		public static Dictionary<string, int> AlphaIndex { get; set; } = null;

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
		/// The list of filters obtained from the database
		/// </summary>
		public static List<Tag> Tags { get; set; } = null;
	}
}