namespace CoreMP
{
	/// <summary>
	/// The MediaControllerViewModel holds the status information for the Media Controller View
	/// </summary>
	public static class MediaControllerViewModel
	{
		public static ModelAvailable Available { get; } = new ModelAvailable();

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
		/// The current song being played
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

		/// <summary>
		/// Is repeat mode on
		/// </summary>
		private static bool repeatOn = false;
		public static bool RepeatOn
		{
			get => repeatOn; set
			{
				repeatOn = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}

		/// <summary>
		/// Is shuffle mode on
		/// </summary>
		private static bool suffleOn = false;
		public static bool ShuffleOn
		{
			get => suffleOn; set
			{
				suffleOn = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null );
			}
		}
	}
}
