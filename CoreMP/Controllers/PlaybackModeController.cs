namespace CoreMP
{
	/// <summary>
	/// The PlaybackModeController is used to obtain the playback mode and respond to user and system initiated playback mode changes 
	/// </summary>
	internal class PlaybackModeController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// Register for the main data available event.
		/// </summary>
		public PlaybackModeController() => NotificationHandler.Register( typeof( StorageController ), StorageDataAvailable );

		/// <summary>
		/// Update the state of the Repeat play flag
		/// Copy it to the Playback singleton so that it can be stored and update the PlaybackModeModel
		/// </summary>
		public bool RepeatOn
		{
			set
			{
				Playback.RepeatOn = value;
				PlaybackModeModel.RepeatOn = value;
			}
		}

		/// <summary>
		/// Update the state of the Shuffle play flag
		/// Copy it to the Playback singleton so that it can be stored and update the PlaybackModeModel
		/// </summary>
		public bool ShuffleOn
		{
			set
			{
				Playback.ShuffleOn = value;
				PlaybackModeModel.ShuffleOn = value;

				// Currently the actual shuffling is triggered by a message. This should be a data change notification
				new ShuffleModeChangedMessage().Send();
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private void StorageDataAvailable()
		{
			// Save the current playback mode obtained from the Playback object
			PlaybackModeModel.RepeatOn = Playback.RepeatOn;
			PlaybackModeModel.ShuffleOn = Playback.ShuffleOn;
		}
	}
}
