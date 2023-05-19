namespace CoreMP
{
	/// <summary>
	/// The MediaNotificationController is used to maintain the model for the MediaNotificationView and to process any commands from the view
	/// The MediaNotificationModel data is mostly transient so the reading of storage is provided just for consistency with other controllers
	/// </summary>
	public class MediaNotificationController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static MediaNotificationController()
		{
			MediaPlayingMessage.Register( ( isPlaying ) => MediaNotificationViewModel.IsPlaying( isPlaying ) );
			SongStartedMessage.Register( ( songStarted ) => MediaNotificationViewModel.SongStarted( songStarted ) );
			SongFinishedMessage.Register( ( _ ) => MediaNotificationViewModel.SongFinished() );
		}
	}
}
