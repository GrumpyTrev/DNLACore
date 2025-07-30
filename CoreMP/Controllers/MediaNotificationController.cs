namespace CoreMP
{
	/// <summary>
	/// The MediaNotificationController is used to maintain the model for the MediaNotificationView.
	/// </summary>
	internal class MediaNotificationController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		public MediaNotificationController()
		{
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.IsPlaying ), () => MediaNotificationViewModel.IsPlaying = PlaybackModel.IsPlaying );
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.SongStarted ),
				( songStarted ) => MediaNotificationViewModel.SongStarted = ( ( bool )songStarted == true ) ? PlaybackModel.SongPlaying : null );
		}
	}
}
