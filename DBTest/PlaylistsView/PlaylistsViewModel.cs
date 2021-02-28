using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsViewModel holds the Playlist data obtained from the PlaylistsController
	/// </summary>
	static class PlaylistsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Playlists.Clear();
			PlaylistNames.Clear();
			Tags.Clear();
			CombinedList.Clear();
			LibraryId = -1;
		}

		/// <summary>
		/// The list of PlayLists that has been obtained from storage
		/// </summary>
		public static List<Playlist> Playlists { get; set; } = new List<Playlist>();

		/// <summary>
		/// The names of the playlists associated with the current library
		/// </summary>
		public static List<string> PlaylistNames { get; set; } = new List<string>();

		/// <summary>
		/// The list of (user) Tags that has been obtained from storage
		/// </summary>
		public static List<Tag> Tags { get; set; } = new List<Tag>();

		/// <summary>
		/// The combined list of Playlists and Tags actually displayed
		/// </summary>
		public static List<object> CombinedList { get; set; } = new List<object>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// Should the album genres be displayed
		/// </summary>
		public static bool DisplayGenre { get; set; } = true;
	}
}