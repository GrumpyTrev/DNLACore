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
		public MediaControllerController() => NotificationHandler.Register<StorageController>( nameof( StorageController.IsSet ), () =>
		{
			InitialiseViewModel();

			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.CurrentPosition ),
				() => MediaControllerViewModel.CurrentPosition = PlaybackModel.CurrentPosition );
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.Duration ),
				() => MediaControllerViewModel.Duration = PlaybackModel.Duration );
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.SongPlaying ),
				() => MediaControllerViewModel.SongPlaying = PlaybackModel.SongPlaying );
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.IsPlaying ),
				() => MediaControllerViewModel.IsPlaying = PlaybackModel.IsPlaying );

			NotificationHandler.Register<Playback>( nameof( Playback.RepeatOn ),
				() => MediaControllerViewModel.RepeatOn = Playback.RepeatOn );
			NotificationHandler.Register<Playback>( nameof( Playback.ShuffleOn ),
				() => MediaControllerViewModel.ShuffleOn = Playback.ShuffleOn );
		} );

		/// <summary>
		/// Called when storage data is available to initialise the view model
		/// </summary>
		private void InitialiseViewModel()
		{
			MediaControllerViewModel.CurrentPosition = PlaybackModel.CurrentPosition;
			MediaControllerViewModel.Duration = PlaybackModel.Duration;
			MediaControllerViewModel.SongPlaying = PlaybackModel.SongPlaying;
			MediaControllerViewModel.RepeatOn = Playback.RepeatOn;
			MediaControllerViewModel.ShuffleOn = Playback.ShuffleOn;
			MediaControllerViewModel.IsPlaying = PlaybackModel.IsPlaying;

			// Let the view know
			MediaControllerViewModel.Available.IsSet = true;
		}
	}
}
