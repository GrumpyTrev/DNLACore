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
			LibraryId = -1;
			DataValid = false;
		}

		/// <summary>
		/// The list of PlayLists that has been obtained from the database
		/// </summary>
		public static List<Playlist> Playlists { get; set; } = new List<Playlist>();

		/// <summary>
		/// The names of the playlists associated with the current library
		/// </summary>
		public static List<string> PlaylistNames { get; set; } = new List<string>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// Indicates whether or not the data held by the class is valid
		/// </summary>
		public static bool DataValid { get; set; } = false;
	}
}