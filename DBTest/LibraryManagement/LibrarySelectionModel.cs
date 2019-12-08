using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The LibrarySelectionModel holds the Library data obtained from the LibrarySelectionController
	/// </summary>
	static class LibrarySelectionModel
	{
		/// <summary>
		/// The list of Libraries that has been obtained from the database
		/// </summary>
		public static List< Library > Libraries { get; set; } = null;
	}
}