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
		/// The list of artists that has been obtained from the database
		/// </summary>
		public static List<Artist> Artists { get; set; } = null;

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
	}
}