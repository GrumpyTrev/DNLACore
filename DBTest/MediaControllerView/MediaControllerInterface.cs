using Android.Content;
using Android.OS;
using Android.Views;
using System;
using static Android.Widget.MediaController;

namespace DBTest
{
	/// <summary>
	/// The MediaControllerInterface class is used to interface to the MediaController class being used to provide the UI for controlling playback
	/// </summary>
	class MediaControllerInterface : BaseBoundControl, MediaControllerController.IMediaReporter, IMediaPlayerControl
	{
		/// <summary>
		/// If being bound to a valid activity then create a MediaController and bind to it.
		/// Otherwise clear the reference to the MediaController
		/// </summary>
		/// <param name="menu"></param>
		public override void BindToMenu( IMenu menu, Context context, View activityContent )
		{
			if ( activityContent != null )
			{
				// This will be called everytime the main menu is recreated but we only want to do this once so check if already bound
				if ( mediaController == null )
				{
					mediaController = new MediaControllerNoHide( context );

					mediaController.SetPrevNextListeners(
						new ClickHandler() { OnClickAction = () => { PlayNext(); } },
						new ClickHandler() { OnClickAction = () => { PlayPrevious(); } } );
					mediaController.SetMediaPlayer( this );

					mediaController.SetAnchorView( activityContent );

					mediaController.Enabled = true;

					MediaControllerController.DataReporter = this;

					// Create a handler for UI switching
					handler = new Handler( context.MainLooper );
				}
			}
			else
			{
				mediaController = null;
				MediaControllerController.DataReporter = null;
			}
		}

		/// <summary>
		/// Called when the data associated with this view is first read or accessed
		/// </summary>
		public void DataAvailable()
		{
			// If a playback device is already available then display the media controller
			if ( MediaControllerViewModel.PlaybackDeviceAvailable == true )
			{
				ShowMediaController();
			}
			else
			{
				// It should already be hidden but hide it anyway
				HideMediaController();
			}
		}

		/// <summary>
		/// Called when the availability of a playback device has changed
		/// </summary>
		/// <param name="available"></param>
		public void DeviceAvailable( bool available )
		{
			// Only process a change
			if ( MediaControllerViewModel.PlaybackDeviceAvailable != available )
			{
				if ( available == true )
				{
					ShowMediaController();
				}
				else
				{
					HideMediaController();
				}
			}
		}

		/// <summary>
		/// Called when the play state has changed.
		/// Reshow the controller to prompt it to access the IsPlaying flag and hence update its internal state
		/// </summary>
		public void PlayStateChanged() => ShowMediaController();

		/// <summary>
		/// Called when the user want to display the previous hidden controller
		/// </summary>
		public void ShowMediaControls()
		{
			if ( MediaControllerViewModel.MediaControllerHiddenByUser == true )
			{
				MediaControllerViewModel.MediaControllerHiddenByUser = false;
				ShowMediaController();
			}
		}

		/// <summary>
		/// Called when new progress values are available.
		/// No action carried out at the moment as the MediaController pulls these values from here
		/// </summary>
		public void MediaProgress()
		{
		}

		/// <summary>
		/// Can the selected connection be paused
		/// </summary>
		/// <returns></returns>
		public bool CanPause() => MediaControllerViewModel.CanPause;

		/// <summary>
		/// Does the selected connection support seeking forward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekBackward() => MediaControllerViewModel.CanSeekBackward;

		/// <summary>
		/// Does the selected connection support seeking backward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekForward() => MediaControllerViewModel.CanSeekForeward;

		/// <summary>
		/// Pause the selected connection
		/// </summary>
		public void Pause() => MediaControllerController.Pause();

		/// <summary>
		/// Seek to the specified position
		/// </summary>
		/// <param name="pos"></param>
		public void SeekTo( int pos ) => MediaControllerController.SeekTo( pos );

		/// <summary>
		/// Get the audio session id for the connection
		/// </summary>
		public int AudioSessionId => throw new NotImplementedException();

		/// <summary>
		/// Buffer percentage - not used
		/// </summary>
		public int BufferPercentage => MediaControllerViewModel.BufferPercentage;

		/// <summary>
		/// The current playback position in milliseconds
		/// </summary>
		public int CurrentPosition => MediaControllerViewModel.CurrentPosition;

		/// <summary>
		/// The total duration of the track in milliseconds
		/// </summary>
		public int Duration => MediaControllerViewModel.Duration;

		/// <summary>
		/// Is the track being played
		/// </summary>
		public bool IsPlaying => MediaControllerViewModel.IsPlaying;

		/// <summary>
		/// Start or resume playback
		/// </summary>
		public void Start() => MediaControllerController.Start();

		/// <summary>
		/// Show the MediaController control
		/// </summary>
		private void ShowMediaController()
		{
			// Perform this on the UI thread
			handler.Post( () => 
			{ 
				// Only show the Media Controller if it has not been previously hidden
				if ( MediaControllerViewModel.MediaControllerHiddenByUser == false )
				{
					mediaController.Visibility = ViewStates.Visible;
					mediaController.Show();
				}
				else
				{
					mediaController.Visibility = ViewStates.Gone;
				}
			} );
		}

		/// <summary>
		/// Hide the MediaController control
		/// </summary>
		private void HideMediaController()
		{
			// Perform this on the UI thread
			handler.Post( () => 
			{
				mediaController.Visibility = ViewStates.Gone;
			} );
		}

		/// <summary>
		/// Play the next track
		/// </summary>
		private void PlayNext() => MediaControllerController.PlayNext();

		/// <summary>
		/// Play the previous track
		/// </summary>
		private void PlayPrevious() => MediaControllerController.PlayPrevious();

		/// <summary>
		/// Class required to implement the View.IOnClickListener interface
		/// </summary>
		private class ClickHandler : Java.Lang.Object, View.IOnClickListener
		{
			/// <summary>
			/// Called when a click has been detected
			/// </summary>
			/// <param name="v"></param>
			public void OnClick( View v ) => OnClickAction();

			/// <summary>
			/// The Action to be performed when a click has been detected
			/// </summary>
			public Action OnClickAction;
		}

		/// <summary>
		/// The MediaController to use to display the UI
		/// </summary>
		private MediaControllerNoHide mediaController = null;

		/// <summary>
		/// The Handler used for UI switching
		/// </summary>
		private Handler handler = null;
	}
}