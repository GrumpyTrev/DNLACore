namespace DBTest
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	static class NowPlayingViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			NowPlayingPlaylist = null;
			LibraryId = -1;
			DataValid = false;
		}

		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static Playlist NowPlayingPlaylist { get; set; } = new Playlist();

		/// <summary>
		/// Index of the selected song
		/// </summary>
		public static int SelectedSong { get; set; } = -1;

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