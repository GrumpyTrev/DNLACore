namespace CoreMP
{
	/// <summary>
	/// The MediaControllerController is used to maintain the model for the MediaControllerView
	/// </summary>
	internal class MediaControllerController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		public MediaControllerController()
		{
			NotificationHandler.Register( typeof( PlaybackModel ), () =>
			{
				MediaControllerViewModel.CurrentPosition = PlaybackModel.CurrentPosition;
				MediaControllerViewModel.Duration = PlaybackModel.Duration;
				MediaControllerViewModel.SongPlaying = PlaybackModel.SongPlaying;
			} );

			NotificationHandler.Register( typeof( Playback ), () =>
			{
				MediaControllerViewModel.RepeatOn = Playback.RepeatOn;
				MediaControllerViewModel.ShuffleOn = Playback.ShuffleOn;
			} );

			NotificationHandler.Register( typeof( PlaybackModel ), "IsPlaying", ()=> MediaControllerViewModel.IsPlaying = PlaybackModel.IsPlaying );
			SongFinishedMessage.Register( ( _ ) => MediaControllerViewModel.SongPlaying = null );
			NotificationHandler.Register( typeof( StorageController ), () => MediaControllerViewModel.Available.IsSet = true );
		}
	}
}
