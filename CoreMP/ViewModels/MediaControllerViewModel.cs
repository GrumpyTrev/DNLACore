namespace CoreMP
{
	/// <summary>
	/// The MediaControllerViewModel holds the status information for the Media Controller View
	/// </summary>
	public static class MediaControllerViewModel
	{
		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// Is there a playback device currently available
		/// </summary>
		private static bool playbackDeviceAvailable = false;
		public static bool PlaybackDeviceAvailable
		{
			get => playbackDeviceAvailable;
			set
			{
				playbackDeviceAvailable = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

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
				NotificationHandler.NotifyPropertyChanged( null );
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
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// The current song being played
		/// </summary>
		private static Song songPlaying = null;
		public static Song SongPlaying
		{
			get => songPlaying; set
			{
				songPlaying = value;
				NotificationHandler.NotifyPropertyChanged( null );
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
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}
	}
}
