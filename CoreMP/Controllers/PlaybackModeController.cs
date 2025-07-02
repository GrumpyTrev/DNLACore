namespace CoreMP
{
	/// <summary>
	/// The PlaybackModeController is used to respond to user and system initiated playback mode changes 
	/// </summary>
	internal class PlaybackModeController
	{
		/// <summary>
		/// Update the state of the Repeat play flag
		/// </summary>
		public bool RepeatOn
		{
			set => Playback.RepeatOn = value;
		}

		/// <summary>
		/// Update the state of the Shuffle play flag
		/// </summary>
		public bool ShuffleOn
		{
			set => Playback.ShuffleOn = value;
		}
	}
}
