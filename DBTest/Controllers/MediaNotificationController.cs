namespace DBTest
{
	/// <summary>
	/// The MediaNotificationController is used to maintain the model for the MediaNotificationView and to process any commands from the view
	/// The MediaNotificationView model data is mostly transient so the reading of storage is provided just for consistency with other controllers
	/// </summary>
	class MediaNotificationController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static MediaNotificationController()
		{
			Mediator.RegisterPermanent( MediaPlaying, typeof( MediaPlayingMessage ) );
			Mediator.RegisterPermanent( SongStarted, typeof( SongStartedMessage ) );
			Mediator.RegisterPermanent( SongFinished, typeof( SongFinishedMessage ) );
		}

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called when the user resumes play via the notification
		/// </summary>
		public static void MediaPlay() => new MediaControlStartMessage().Send();

		/// <summary>
		/// Called when the user pauses play via the notification
		/// </summary>
		public static void MediaPause() => new MediaControlPauseMessage().Send();

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable()
		{
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when a MediaPlayingMessage has been received.
		/// </summary>
		/// <param name="message"></param>
		private static void MediaPlaying( object message ) => DataReporter?.IsPlaying( ( ( MediaPlayingMessage )message ).IsPlaying );

		/// <summary>
		/// Called when a SongStartedMessage has been received.
		/// </summary>
		/// <param name="message"></param>
		private static void SongStarted( object message ) => DataReporter?.SongStarted( ( ( SongStartedMessage )message ).SongPlayed );

		/// <summary>
		/// Called when a SongFinishedMessage has been received.
		/// </summary>
		/// <param name="message"></param>
		private static void SongFinished( object _ ) => DataReporter?.SongFinished();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static INotificationReporter DataReporter
		{
			get => ( INotificationReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface INotificationReporter : DataReporter.IReporter
		{
			void PlayStopped();
			void SongStarted( Song song );
			void SongFinished();
			void IsPlaying( bool isPlayimg );
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}