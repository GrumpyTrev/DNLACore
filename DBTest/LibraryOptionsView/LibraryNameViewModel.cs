namespace DBTest
{
	/// <summary>
	/// The LibraryNameViewModel holds the name of the currently selected Library
	/// </summary>
	static class LibraryNameViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			LibraryName = "";
		}

		/// <summary>
		/// The name of the currently selected library
		/// </summary>
		public static string LibraryName { get; set; } = "";
	}
}