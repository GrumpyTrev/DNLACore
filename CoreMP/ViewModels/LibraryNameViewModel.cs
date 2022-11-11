namespace CoreMP
{
	/// <summary>
	/// The LibraryNameViewModel holds the name of the currently selected Library
	/// </summary>
	public static class LibraryNameViewModel
	{
		/// <summary>
		/// The name of the currently selected library
		/// </summary>
		private static string libraryName = "";
		public static string LibraryName
		{
			get => libraryName;
			set
			{
				libraryName = value;
				NotificationHandler.NotifyPropertyChanged();
			}
		}
	}
}
