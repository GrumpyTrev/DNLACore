namespace CoreMP
{
	/// <summary>
	/// The PlaybackModel holds details of the song being played. This includes the playback position.
	/// </summary>
	public static class PlaybackModel
	{
		/// <summary>
		/// The current playback position in milliseconds
		/// </summary>
		private static int currentPosition = 0;
		public static int CurrentPosition
		{
			get => currentPosition;
			set
			{
				currentPosition = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}

		/// <summary>
		/// The total duration of the track in milliseconds
		/// </summary>
		private static int duration = 0;
		public static int Duration
		{
			get => duration;
			set
			{
				duration = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}

		/// <summary>
		/// The current song being played (null indicates no song is currently being played)
		/// </summary>
		private static Song songPlaying = null;
		public static Song SongPlaying
		{
			get => songPlaying; set
			{
				songPlaying = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}

		/// <summary>
		/// Is the track being played
		/// </summary>
		private static bool isPlaying = false;
		public static bool IsPlaying
		{
			get => isPlaying; set
			{
				isPlaying = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}
	}
}
