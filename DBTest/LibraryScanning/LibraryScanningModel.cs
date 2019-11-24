using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsViewModel holds the Playlist data obtained from the PlaylistsController
	/// </summary>
	static class LibraryScanningModel
	{
		/// <summary>
		/// The list of Libraries that has been obtained from the database
		/// </summary>
		public static List< Library > Libraries { get; set; } = null;
	}
}