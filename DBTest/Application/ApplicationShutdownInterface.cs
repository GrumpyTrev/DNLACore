using Android.Content;
using Android.OS;

namespace DBTest
{
	/// <summary>
	/// The ApplicationShutdownInterface class provides an interface to the ApplicationShutdownService
	/// </summary>
	internal class ApplicationShutdownInterface : Java.Lang.Object, IServiceConnection, ApplicationShutdownService.IServiceCallbacks
	{
		/// <summary>
		/// Start and then bind to the service
		/// </summary>
		/// <param name="context"></param>
		public ApplicationShutdownInterface( Context context )
		{
			// Start the media control service
			_ = context.StartService( new Intent( context, typeof( ApplicationShutdownService ) ) );

			// Bind to the service
			_ = context.BindService( new Intent( context, typeof( ApplicationShutdownService ) ), this, Bind.None );
		}

		/// <summary>
		/// Called when the running service has connected
		/// Retain a reference to the service for commands and provide this instance as the service's callback interface
		/// </summary>
		/// <param name="name"></param>
		/// <param name="service"></param>
		public void OnServiceConnected( ComponentName name, IBinder service )
		{
			controlService = ( ( ApplicationShutdownService.ApplicationShutdownServiceBinder )service ).Service;
			controlService.Reporter = this;
		}

		/// <summary>
		/// Called when the service has disconnected
		/// This only happens when something unexpected has happened at the service end
		/// </summary>
		/// <param name="name"></param>
		public void OnServiceDisconnected( ComponentName name ) => controlService = null;

		/// <summary>
		/// Called by the service when it has detected the application being removed from the system
		/// </summary>
		public void Shutdown() => MainApp.Shutdown();

		/// <summary>
		/// The service carrying out the shutdown monitoring
		/// </summary>
		private ApplicationShutdownService controlService = null;
	}
}
