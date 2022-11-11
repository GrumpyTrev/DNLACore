using Android.Content;
using Android.OS;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The MediaNotificationServiceInterface class provides an interface to the MediaNotificationService for the rest of the system
	/// </summary>
	internal class MediaNotificationServiceInterface : Java.Lang.Object, IServiceConnection, MediaNotificationController.INotificationReporter,
		MediaNotificationService.IServiceCallbacks
	{
		public MediaNotificationServiceInterface( Context context )
		{
			// Start the media control service
			context.StartService( new Intent( context, typeof( MediaNotificationService ) ) );

			// Bind to the service
			context.BindService( new Intent( context, typeof( MediaNotificationService ) ), this, Bind.None );

			MediaNotificationController.DataReporter = this;
		}

		/// <summary>
		/// Called to stop the service
		/// </summary>
		public void StopService() => controlService?.Stop();

		/// <summary>
		/// Called when the data associated with this view is first read or accessed
		/// </summary>
		public void DataAvailable()
		{
		}

		/// <summary>
		/// Called when the playing of a song has been stopped rather than just paused
		/// </summary>
		public void PlayStopped() => controlService?.PlayStopped();

		/// <summary>
		/// Called when the song being played has changed
		/// </summary>
		/// <param name="song"></param>
		public void SongStarted( Song song ) => controlService.SongStarted( song );

		/// <summary>
		/// Called when the song being played has finished
		/// </summary>
		public void SongFinished() => controlService.SongFinished();		
		
		/// <summary>
		/// Called when the playing state of the song changes
		/// </summary>
		/// <param name="isPlaying"></param>
		public void IsPlaying( bool isPlaying ) => controlService.IsPlaying( isPlaying );

		/// <summary>
		/// Called when the user resumes play via the notification
		/// </summary>
		public void MediaPlay() => MediaNotificationController.MediaPlay();

		/// <summary>
		/// Called when the user pauses play via the notification
		/// </summary>
		public void MediaPause() => MediaNotificationController.MediaPause();

		/// <summary>
		/// Called when the running service has connected to this manager
		/// Retain a reference to the service for commands and provide this instance as the service's callback interface
		/// </summary>
		/// <param name="name"></param>
		/// <param name="service"></param>
		public void OnServiceConnected( ComponentName name, IBinder service )
		{
			controlService = ( ( MediaNotificationService.MediaNotificationServiceBinder )service ).Service;
			controlService.Reporter = this;
		}

		/// <summary>
		/// Called when the service has disconnected
		/// This only happens when something unexpected has happened at the service end
		/// </summary>
		/// <param name="name"></param>
		public void OnServiceDisconnected( ComponentName name ) => controlService = null;

		/// <summary>
		/// The service carrying out the notification media controls
		/// </summary>
		private MediaNotificationService controlService = null;
	}
}
