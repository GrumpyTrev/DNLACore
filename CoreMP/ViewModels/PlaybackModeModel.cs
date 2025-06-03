namespace CoreMP
{
	/// <summary>
	/// The PlaybackModeModel holds the playback mode currently active
	/// </summary>
	public static class PlaybackModeModel
	{
		/// <summary>
		/// Keep track of whether or not repeat play is on
		/// </summary>
		private static bool repeatOn = false;
		public static bool RepeatOn
		{
			get => repeatOn;
			set
			{
				repeatOn = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// Keep track of whether or not shuffle play is on
		/// </summary>
		private static bool shuffleOn = false;
		public static bool ShuffleOn
		{
			get => shuffleOn;
			set
			{
				shuffleOn = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}
	}
}
