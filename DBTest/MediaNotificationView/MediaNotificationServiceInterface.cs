using Android.Content;
using Android.OS;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The MediaNotificationServiceInterface class provides an interface to the MediaNotificationService for the rest of the system
	/// </summary>
	internal class MediaNotificationServiceInterface : Java.Lang.Object, IServiceConnection, MediaNotificationService.IServiceCallbacks
	{
		public MediaNotificationServiceInterface( Context context )
		{
			// Start the media control service
			context.StartService( new Intent( context, typeof( MediaNotificationService ) ) );

			// Bind to the service
			context.BindService( new Intent( context, typeof( MediaNotificationService ) ), this, Bind.None );

			// Register interest in notification provided via the MediaNotificationViewModel
			NotificationHandler.Register( typeof( MediaNotificationViewModel ), "SongStarted", ( sender, _ ) => controlService.SongStarted( ( Song )sender ) );
			NotificationHandler.Register( typeof( MediaNotificationViewModel ), "SongFinished", ( _, _ ) => controlService.SongFinished());
			NotificationHandler.Register( typeof( MediaNotificationViewModel ), "IsPlaying", ( sender, _ ) => controlService.IsPlaying( ( bool )sender ) );
		}

		/// <summary>
		/// Called to stop the service
		/// </summary>
		public void StopService() => controlService?.Stop();

		/// <summary>
		/// Called when the user resumes play via the notification
		/// </summary>
		public void MediaPlay() => MainApp.CommandInterface.Start();

		/// <summary>
		/// Called when the user pauses play via the notification
		/// </summary>
		public void MediaPause() => MainApp.CommandInterface.Pause();

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
