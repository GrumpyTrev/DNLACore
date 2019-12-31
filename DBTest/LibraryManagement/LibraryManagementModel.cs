using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The LibraryManagementModel holds the Library data obtained from the LibraryManagementController
	/// </summary>
	static class LibraryManagementModel
	{
		/// <summary>
		/// The list of Libraries that has been obtained from the database
		/// </summary>
		public static List< Library > Libraries { get; set; } = null;
	}
}