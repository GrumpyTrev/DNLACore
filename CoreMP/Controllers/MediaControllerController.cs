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
		public MediaControllerController() => NotificationHandler.Register<StorageController>( () =>
		{
			InitialiseViewModel();

			NotificationHandler.Register<PlaybackModel>( () =>
			{
				MediaControllerViewModel.CurrentPosition = PlaybackModel.CurrentPosition;
				MediaControllerViewModel.Duration = PlaybackModel.Duration;
				MediaControllerViewModel.SongPlaying = PlaybackModel.SongPlaying;
				MediaControllerViewModel.IsPlaying = PlaybackModel.IsPlaying;
			} );

			NotificationHandler.Register<Playback>( () =>
			{
				MediaControllerViewModel.RepeatOn = Playback.RepeatOn;
				MediaControllerViewModel.ShuffleOn = Playback.ShuffleOn;
			} );
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
