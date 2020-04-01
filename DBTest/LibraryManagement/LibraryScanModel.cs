using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanModel holds the data used by the LibraryScanController
	/// </summary>
	static class LibraryScanModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			UnmatchedSongs = null;
			LibraryModified = false;
			LibraryBeingScanned = null;
		}

		/// <summary>
		/// Any songs in the library that have not been matched in the scan process
		/// </summary>
		public static List<Song> UnmatchedSongs { get; set; } = null;

		/// <summary>
		/// Has the library been changed at all during the scan process
		/// </summary>
		public static bool LibraryModified { get; set; } = false;

		/// <summary>
		/// The library being scanned
		/// </summary>
		public static Library LibraryBeingScanned { get; set; } = null;
	}
}