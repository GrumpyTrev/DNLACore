namespace CoreMP
{
	/// <summary>
	/// The PlaybackModeController is used to obtain the playback mode and respond to user and system initiated playback mode changes 
	/// </summary>
	internal class PlaybackModeController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		public PlaybackModeController() =>
			// Register for the main data available event.
			NotificationHandler.Register( typeof( StorageController ), StorageDataAvailable );

		/// <summary>
		/// Update the state of the Auto play flag
		/// </summary>
		public bool AutoOn
		{
			set
			{
				Playback.SingletonAutoPlayOn = value;

				// If autoplay is now on, turn off repeat and shuffle
				if ( Playback.SingletonAutoPlayOn == true )
				{
					Playback.SingletonRepeatPlayOn = false;
					Playback.SingletonShufflePlayOn = false;
				}

				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Update the state of the Repeat play flag
		/// </summary>
		public bool RepeatOn
		{
			set
			{
				Playback.SingletonRepeatPlayOn = value;

				// If repeat is now on, turn off auto
				if ( Playback.SingletonRepeatPlayOn == true )
				{
					Playback.SingletonAutoPlayOn = false;
				}

				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Update the state of the Shuffle play flag
		/// </summary>
		public bool ShuffleOn
		{
			set
			{
				Playback.SingletonShufflePlayOn = value;

				// If shuffle is now on, turn off auto
				if ( Playback.SingletonShufflePlayOn == true )
				{
					Playback.SingletonAutoPlayOn = false;
				}

				new ShuffleModeChangedMessage().Send();

				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private void StorageDataAvailable()
		{
			// Save the current playback mode obtained from the Playback object
			PlaybackModeModel.AutoOn = Playback.SingletonAutoPlayOn;
			PlaybackModeModel.RepeatOn = Playback.SingletonRepeatPlayOn;
			PlaybackModeModel.ShuffleOn = Playback.SingletonShufflePlayOn;

			// Update the summary state in the model
			PlaybackModeModel.UpdateActivePlayMode();
		}
	}
}
