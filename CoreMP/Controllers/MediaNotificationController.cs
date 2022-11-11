namespace CoreMP
{
	/// <summary>
	/// The MediaNotificationController is used to maintain the model for the MediaNotificationView and to process any commands from the view
	/// The MediaNotificationView model data is mostly transient so the reading of storage is provided just for consistency with other controllers
	/// </summary>
	public class MediaNotificationController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static MediaNotificationController()
		{
			MediaPlayingMessage.Register( MediaPlaying );
			SongStartedMessage.Register( SongStarted );
			SongFinishedMessage.Register( SongFinished );
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
		private static void StorageDataAvailable() => DataReporter?.DataAvailable();

		/// <summary>
		/// Called when a MediaPlayingMessage has been received.
		/// </summary>
		/// <param name="message"></param>
		private static void MediaPlaying( bool isPlaying ) => DataReporter?.IsPlaying( isPlaying );

		/// <summary>
		/// Called when a SongStartedMessage has been received.
		/// </summary>
		/// <param name="message"></param>
		private static void SongStarted( Song songStarted ) => DataReporter?.SongStarted( songStarted );

		/// <summary>
		/// Called when a SongFinishedMessage has been received.
		/// </summary>
		private static void SongFinished( Song _ ) => DataReporter?.SongFinished();

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
