using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The LibraryManagementViewModel holds data maintained by the LibraryManagementController
	/// </summary>
	public static class LibraryManagementViewModel
	{
		/// <summary>
		/// List of libraries to choose from
		/// </summary>
		public static List<Library> AvailableLibraries { get; internal set; } = new List<Library>();

		/// <summary>
		/// The names of all the available libraries
		/// </summary>
		public static List<string> LibraryNames { get; internal set; } = new List<string>();

		/// <summary>
		/// The index in the AvailableLibraries of the currently selected library
		/// </summary>
		public static int SelectedLibraryIndex { get; internal set; } = -1;
	}
}
