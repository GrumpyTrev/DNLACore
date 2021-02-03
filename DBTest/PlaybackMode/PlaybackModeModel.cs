namespace DBTest
{
	/// <summary>
	/// The PlaybackModeModel holds the playback mode currently active
	/// </summary>
	static class PlaybackModeModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
		}

		/// <summary>
		/// Form the ActivePlayMode from the repeat, shuffle and auto flag
		/// </summary>
		public static void UpdateActivePlayMode()
		{
			if ( AutoOn == true )
			{
				ActivePlayMode = PlayModeType.Auto;
			}
			else if ( RepeatOn == true )
			{
				if ( ShuffleOn == true )
				{
					ActivePlayMode = PlayModeType.RepeatAndShuffle;
				}
				else
				{
					ActivePlayMode = PlayModeType.Repeat;
				}
			}
			else if ( ShuffleOn == true )
			{
				ActivePlayMode = PlayModeType.Shuffle;
			}
			else
			{
				ActivePlayMode = PlayModeType.LinearPlay;
			}
		}

		/// <summary>
		/// The possible combinations of playback mode
		/// </summary>
		public enum PlayModeType : short
		{
			LinearPlay,
			Repeat,
			Shuffle,
			RepeatAndShuffle,
			Auto
		}

		/// <summary>
		/// The currently active playback mode
		/// </summary>
		public static PlayModeType ActivePlayMode { get; private set; } = PlayModeType.LinearPlay;

		/// <summary>
		/// Keep track of whether or not repeat play is on
		/// </summary>
		public static bool RepeatOn { get; set; } = false;

		/// <summary>
		/// Keep track of whether or not shuffle play is on
		/// </summary>
		public static bool ShuffleOn { get; set; } = false;

		/// <summary>
		/// Keep track of whether or not auto play is on
		/// </summary>
		public static bool AutoOn { get; set; } = false;
	}
}