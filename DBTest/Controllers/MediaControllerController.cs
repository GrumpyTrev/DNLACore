﻿namespace DBTest
{
	/// <summary>
	/// The MediaControllerController is used to maintain the model for the MediaControllerView and to process any commands from the view
	/// The MediaControllerView model data is mostly transient so the reading of storage is provided just for consistency with other controllers
	/// </summary>
	class MediaControllerController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static MediaControllerController()
		{
			Mediator.RegisterPermanent( MediaProgressMessageReceived , typeof( MediaProgressMessage ) );
			Mediator.RegisterPermanent( DeviceAvailable, typeof( PlaybackDeviceAvailableMessage ) );
			Mediator.RegisterPermanent( MediaPlayingMessageReceived, typeof( MediaPlayingMessage ) );
		}

		/// <summary>
		/// Get the library name data 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called to show the playback controls. Pass on to the view
		/// </summary>
		public static void ShowMediaController() => DataReporter?.ShowMediaControls();

		/// <summary>
		/// Called when the media control pause button has been pressed.
		/// Generate a MediaControlPauseMessage message
		/// </summary>
		public static void Pause() => new MediaControlPauseMessage().Send();

		/// <summary>
		/// Called when the media control seek button has been pressed.
		/// Generate a MediaControlSeekToMessage message
		/// </summary>
		public static void SeekTo( int position ) => new MediaControlSeekToMessage() { Position = position }.Send();

		/// <summary>
		/// Called when the media control start button has been pressed.
		/// Generate a MediaControlStartMessage message
		/// </summary>
		public static void Start() => new MediaControlStartMessage().Send();

		/// <summary>
		/// Called when the media control play next button has been pressed.
		/// Generate a MediaControlPlayNextMessage message
		/// </summary>
		public static void PlayNext() => new MediaControlPlayNextMessage().Send();

		/// <summary>
		/// Called when the media control play previous button has been pressed.
		/// Generate a MediaControlPlayPreviousMessage message
		/// </summary>
		public static void PlayPrevious() => new MediaControlPlayPreviousMessage().Send();

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable()
		{
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when a MediaProgressMessage has been received.
		/// Update the values held in the model.
		/// There is no need at the moment to let the view know about the change, The view will access the new values when required
		/// </summary>
		/// <param name="message"></param>
		private static void MediaProgressMessageReceived( object message )
		{
			MediaProgressMessage receivedMessage = message as MediaProgressMessage;
			MediaControllerViewModel.CurrentPosition = receivedMessage.CurrentPosition;
			MediaControllerViewModel.Duration = receivedMessage.Duration;
		}

		/// <summary>
		/// Called when a MediaPlayingMessage is received.
		/// If this is a change of play state then let the view know
		/// </summary>
		/// <param name="message"></param>
		private static void MediaPlayingMessageReceived( object message )
		{
			bool newPlayState = ( ( MediaPlayingMessage )message ).IsPlaying;
			if ( MediaControllerViewModel.IsPlaying != newPlayState )
			{
				MediaControllerViewModel.IsPlaying = newPlayState;
				DataReporter?.PlayStateChanged();
			}
		}

		/// <summary>
		/// Called when the PlaybackDeviceAvailableMessage message is received
		/// If this is a change then report it
		/// </summary>
		/// <param name="message"></param>
		private static void DeviceAvailable( object message )
		{
			PlaybackDevice newDevice = ( message as PlaybackDeviceAvailableMessage ).SelectedDevice;

			// If the view data is not available yet, just update the model.
			// Otherwise report to the view and then update the model
			if ( dataReporter.DataAvailable == true )
			{
				DataReporter?.DeviceAvailable( newDevice != null );
			}

			MediaControllerViewModel.PlaybackDeviceAvailable = ( newDevice != null );
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IMediaReporter DataReporter
		{
			get => ( IMediaReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IMediaReporter : DataReporter.IReporter
		{
			void DeviceAvailable( bool available );
			void PlayStateChanged();
			void ShowMediaControls();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}