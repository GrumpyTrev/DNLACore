namespace DBTest
{
	/// <summary>
	/// The PlaybackModeController is used to obtain the playback mode and respond to user and system initiated playback mode changes 
	/// </summary>
	class PlaybackModeController : BaseController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static PlaybackModeController()
		{
			instance = new PlaybackModeController();
		}

		/// <summary>
		/// Get the playback mode data 
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Update the state of the Auto play flag
		/// </summary>
		public static bool AutoOn
		{
			set
			{
				Playback.AutoPlayOn = value;

				// If autoplay is now on, turn off repeat and shuffle
				if ( Playback.AutoPlayOn == true )
				{
					Playback.RepeatPlayOn = false;
					Playback.ShufflePlayOn = false;
				}

				instance.StorageDataAvailable();
			}
		}

		/// <summary>
		/// Update the state of the Repeat play flag
		/// </summary>
		public static bool RepeatOn
		{
			set
			{
				Playback.RepeatPlayOn = value;

				// If repeat is now on, turn off auto
				if ( Playback.RepeatPlayOn == true )
				{
					Playback.AutoPlayOn = false;
				}

				instance.StorageDataAvailable();
			}
		}

		/// <summary>
		/// Update the state of the Shuffle play flag
		/// </summary>
		public static bool ShuffleOn
		{
			set
			{
				Playback.ShufflePlayOn = value;

				// If shuffle is now on, turn off auto
				if ( Playback.ShufflePlayOn == true )
				{
					Playback.AutoPlayOn = false;
				}

				instance.StorageDataAvailable();
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		protected override void StorageDataAvailable( object _ = null )
		{
			// Save the current playback mode obtained from the Playback object
			PlaybackModeModel.AutoOn = Playback.AutoPlayOn;
			PlaybackModeModel.RepeatOn = Playback.RepeatPlayOn;
			PlaybackModeModel.ShuffleOn = Playback.ShufflePlayOn;

			// Update the summary state in the model
			PlaybackModeModel.UpdateActivePlayMode();

			// Call the base class
			base.StorageDataAvailable();
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter DataReporter
		{
			set => instance.Reporter = value;
		}

		/// <summary>
		/// The one and only PlaybackModeController instance
		/// </summary>
		private static readonly PlaybackModeController instance = null;
	}
}