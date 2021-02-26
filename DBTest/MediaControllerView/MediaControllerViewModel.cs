namespace DBTest
{
	/// <summary>
	/// The MediaControllerViewModel holds the status information for the Media Controller View
	/// </summary>
	static class MediaControllerViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			MediaControllerHiddenByUser = true;
			PlaybackDeviceAvailable = false;
			CurrentPosition = 0;
			Duration = 0;
			IsPlaying = false;
		}

		/// <summary>
		/// Keep track of whether or not the Media Controller has been hidden by the user
		/// </summary>
		public static bool MediaControllerHiddenByUser { get; set; } = false;

		/// <summary>
		/// Is there a playback device currently available
		/// </summary>
		public static bool PlaybackDeviceAvailable { get; set; } = false;

		/// <summary>
		/// Buffer percentage - not used
		/// </summary>
		public static int BufferPercentage => 0;

		/// <summary>
		/// The current playback position in milliseconds
		/// </summary>
		public static int CurrentPosition { get; set; } = 0;

		/// <summary>
		/// The total duration of the track in milliseconds
		/// </summary>
		public static int Duration { get; set; } = 0;

		/// <summary>
		/// Is the track being played
		/// </summary>
		public static bool IsPlaying { get; set; } = false;

		/// <summary>
		/// Can playback be paused
		/// </summary>
		public static bool CanPause { get; set; } = true;

		/// <summary>
		/// Can the playback position be moved forward
		/// </summary>
		public static bool CanSeekForeward { get; set; } = true;
		/// <summary>
		/// Can the playback position be moved backward
		/// </summary>
		public static bool CanSeekBackward { get; set; } = true;
	}
}