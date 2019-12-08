using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanningModel holds the Library data obtained from the LibraryScanningController
	/// </summary>
	static class LibraryScanningModel
	{
		/// <summary>
		/// The list of Libraries that has been obtained from the database
		/// </summary>
		public static List< Library > Libraries { get; set; } = null;
	}
}