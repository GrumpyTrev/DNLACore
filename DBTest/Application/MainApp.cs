using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	[Application]
	internal class MainApp : Application, ICoreMP
	{
		/// <summary>
		/// Base constructor which must be implemented if it is to successfully inherit from the Application
		/// class.
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="transfer"></param>
		public MainApp( IntPtr handle, JniHandleOwnership transfer ) : base( handle, transfer )
		{
			instance = this;

			// Create an instance of the CoreMPApp to interface to the CoreMP library
			coreMPInterface = new CoreMPApp();

			// Set up the link from the CoreMP libarary to this UI implementation
			coreMPInterface.SetInterface( this );

			// Tell the CoreMP how to play locally
			coreMPInterface.SetLocalPlayer( new LocalPlayback( this ) );

			// Start monitoring the WiFi
			new WifiMontor( this, ( wifiAvailable ) => coreMPInterface.WifiStateChanged( wifiAvailable ) );

			// Create a wake lock for use during playback
			wakeLock = new KeepAwake( this );
		}

		/// <summary>
		/// Called when the activity has checked or obtained the storage permission
		/// </summary>
		public static void StoragePermissionGranted() => Task.Run( () => instance.PostPermissionInitialisation() );

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="message"></param>
		public void Log( string message ) => Android.Util.Log.WriteLine( Android.Util.LogPriority.Debug, "DBTest", message );

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="message"></param>
		public void LogTimed( string message ) => Android.Util.Log.WriteLine( Android.Util.LogPriority.Debug, "DBTest", $"{DateTime.Now}:{message}" );

		/// <summary>
		/// Report an event 
		/// </summary>
		/// <param name="message"></param>
		public void Event( string message ) => UiSwitchingHandler.Post( () => Toast.MakeText( this, message, ToastLength.Short ).Show() );

		/// <summary>
		/// Report an error
		/// </summary>
		/// <param name="message"></param>
		public void Error( string message ) => UiSwitchingHandler.Post( () => Toast.MakeText( this, message, ToastLength.Long ).Show() );

		/// <summary>
		/// Post an action onto the UI thread
		/// </summary>
		/// <param name="post"></param>
		public void PostAction( Action post ) => UiSwitchingHandler.Post( post );

		/// <summary>
		/// Bind the controls to the menu
		/// </summary>
		/// <param name="menu"></param>
		public static void BindMenu( IMenu menu, Context context, View activityContent )
		{
			instance.playbackMonitoring.BindToMenu( menu, context, activityContent );
			instance.playbackModeViewer.BindToMenu( menu, context, activityContent );
		}

		/// <summary>
		/// Bind the controls to the view
		/// </summary>
		/// <param name="view"></param>
		/// <param name="context"></param>
		public static void BindView( View view, Context context )
		{
			instance.libraryNameDisplay.BindToView( view, context );
			instance.mediaControllerView.BindToView( view, context );
		}

		/// <summary>
		/// OnCreate needs to be overwritten otherwise Android does not create the MainApp class until it wnats to - strange but true
		/// </summary>
		public override void OnCreate() => base.OnCreate();

		/// <summary>
		/// Called when the application shutdown service has detected this applicaton being removed from the system
		/// </summary>
		public static void Shutdown()
		{
			instance.mediaNotificationServiceInterface.StopService();
			instance.coreMPInterface.Shutdown();
		}

		/// <summary>
		/// The path used to store the media database
		/// </summary>
		public string StoragePath => Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

		/// <summary>
		/// Aquire the wakelock
		/// </summary>
		public void AquireWakeLock() => wakeLock.AquireLock();

		/// <summary>
		/// Relesae the wakelock
		/// </summary>
		public void ReleaseWakeLock() => wakeLock.ReleaseLock();

		/// <summary>
		/// Allow static access to the CoreMP command interface
		/// </summary>
		public static Commander CommandInterface => instance.coreMPInterface.CommandInterface;

		/// <summary>
		/// Initialisation to be performed once we know that storage permissions have been obtained
		/// </summary>
		private void PostPermissionInitialisation()
		{
			// Let the CoreMP library know that it can start initialising the storage
			coreMPInterface.Initialise();

			// Bind the command handlers to their command identities
			CommandRouter.BindHandlers();

			// Start the MediaNotificationService
			mediaNotificationServiceInterface = new MediaNotificationServiceInterface( Context );

			// Start the ApplicationShutdownService
			applicationShutdownInterface = new ApplicationShutdownInterface( Context );
		}

		/// <summary>
		/// Context to use for switching to the UI thread
		/// </summary>
		private static Handler UiSwitchingHandler { get; } = new( Looper.MainLooper ); 

		/// <summary>
		/// THe one and only MainApp
		/// </summary>
		private static MainApp instance = null;

		/// <summary>
		/// The PlaybackMonitor instance used to monitor the state of the playback system
		/// </summary>
		private readonly PlaybackMonitor playbackMonitoring = new();

		/// <summary>
		/// The PlaybackModeView instance used to display the playback mode and to allow it to be changed
		/// </summary>
		private readonly PlaybackModeView playbackModeViewer = new();

		/// <summary>
		/// The LibraryNameDisplay instance used to display the library name
		/// </summary>
		private readonly LibraryNameDisplay libraryNameDisplay = new();

		/// <summary>
		/// The MediaControllerView instance used to control playback
		/// </summary>
		private readonly MediaControllerView mediaControllerView = new();

		/// <summary>
		/// The control used to interface to the media notification service
		/// </summary>
		private MediaNotificationServiceInterface mediaNotificationServiceInterface = null;

		/// <summary>
		/// The control used to interface to the application shutdown service
		/// </summary>
#pragma warning disable IDE0052 // Remove unread private members
		private ApplicationShutdownInterface applicationShutdownInterface = null;
#pragma warning restore IDE0052 // Remove unread private members

		/// <summary>
		/// The CoreMPApp instance used to iunterface to the CoreMP library
		/// </summary>
		private readonly CoreMPApp coreMPInterface = null;

		/// <summary>
		/// KeepAwake instance used during playback
		/// </summary>
		private readonly KeepAwake wakeLock = null;
	}
}
