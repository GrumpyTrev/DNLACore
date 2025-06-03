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
			MediaProgressMessage.Register( ( currentPosition, duration ) =>
			{
				MediaControllerViewModel.CurrentPosition = currentPosition;
				MediaControllerViewModel.Duration = duration;
			});
			NotificationHandler.Register( typeof( PlaybackSelectionModel ), "SelectedDevice", 
				() => MediaControllerViewModel.PlaybackDeviceAvailable = ( PlaybackSelectionModel.SelectedDevice != null) );

			MediaPlayingMessage.Register( ( isPlaying ) => MediaControllerViewModel.IsPlaying = isPlaying );
			SongFinishedMessage.Register( ( _ ) => MediaControllerViewModel.SongPlaying = null );
			NotificationHandler.Register( typeof( Playlists ), () => MediaControllerViewModel.SongPlaying = NowPlayingViewModel.CurrentSong );
			NotificationHandler.Register( typeof( StorageController ), () => MediaControllerViewModel.Available.IsSet = true );
			NotificationHandler.Register( typeof( PlaybackModeModel ), () =>
			{
				MediaControllerViewModel.RepeatOn = PlaybackModeModel.RepeatOn;
				MediaControllerViewModel.ShuffleOn = PlaybackModeModel.ShuffleOn;
			} );
		}
	}
}
