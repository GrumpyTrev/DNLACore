using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsViewModel holds the Playlist data obtained from the PlaylistsController
	/// </summary>
	static class PlaylistsViewModel
	{
		/// <summary>
		/// The list of PlayLists that has been obtained from the database
		/// </summary>
		public static List<Playlist> Playlists { get; set; } = null;

		/// <summary>
		/// The names of the playlists associated with the current library
		/// </summary>
		public static List<string> PlaylistNames { get; set; } = null;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;
	}
}