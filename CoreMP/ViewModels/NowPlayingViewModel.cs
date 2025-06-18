namespace CoreMP
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	public static class NowPlayingViewModel
	{
		/// <summary>
		/// Allow changes to this model to be monitored
		/// </summary>
		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static SongPlaylist NowPlayingPlaylist { get; internal set; } = null;

		/// <summary>
		/// Property introduced to trigger a notification when changes to the playlist have been made (and completed)
		/// </summary>
		internal static bool PlaylistUpdated
		{
			set => NotificationHandler.NotifyPropertyChanged( null );
		}

		/// <summary>
		///  The index of the song currently being played
		/// </summary>
		private static int currentSongIndex = -1;
		public static int CurrentSongIndex
		{
			get => currentSongIndex;
			internal set
			{
				currentSongIndex = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}

		/// <summary>
		/// Is the song currently being played
		/// </summary>
		private static bool isPlaying = false;
		public static bool IsPlaying
		{
			get => isPlaying;
			internal set
			{
				isPlaying = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}
	}
}
