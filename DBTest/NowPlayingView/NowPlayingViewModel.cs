namespace DBTest
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	static class NowPlayingViewModel
	{
		/// <summary>
		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static Playlist NowPlayingPlaylist { get; set; } = null;

		/// <summary>
		/// Index of the selected song
		/// </summary>
		public static int SelectedSong { get; set; } = -1;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;
	}
}