using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace DBTest
{
	[Application]
	internal class MainApp : Application, Logger.ILogger
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

			// Set up logging
			Logger.Reporter = this;

			// Initialise the storage
			InitialiseStorage();

			// Bind the command handlers to their command identities
			CommandRouter.BindHandlers();

			// Configure the controllers
			ConfigureControllers();

			// Initialise the network monitoring
			deviceDiscoverer = new DeviceDiscovery( Context );

			// Create a PlaybackRouter.
			playbackRouter = new PlaybackRouter( Context );

			// Start the MediaNotificationService
			mediaNotificationServiceInterface = new MediaNotificationServiceInterface( Context );

			// Start the ApplicationShutdownService
			applicationShutdownInterface = new ApplicationShutdownInterface( Context );
		}

		/// Add the specified interface to the callback colletion
		/// </summary>
		/// <param name="callback"></param>
		public static void RegisterPlaybackCapabilityCallback( DeviceDiscovery.IDeviceDiscoveryChanges callback ) => 
			instance.deviceDiscoverer.RegisterCallback( callback );

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="message"></param>
		public void Log( string message ) => Android.Util.Log.WriteLine( Android.Util.LogPriority.Debug, "DBTest", message );

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
			instance.playbackRouter.StopRouter();
			instance.mediaNotificationServiceInterface.StopService();
		}

		/// <summary>
		/// Configure all the controllers
		/// </summary>
		private void ConfigureControllers()
		{
			AlbumsController.GetControllerData();
			ArtistsController.GetControllerData();
			PlaylistsController.GetControllerData();
			LibraryNameDisplayController.GetControllerData();
			FilterManagementController.GetControllerData();
			PlaybackSelectionController.GetControllerData();
			AutoplayController.GetControllerData();
			PlaybackModeController.GetControllerData();
			PlaybackManagementController.GetControllerData();
			MediaControllerController.GetControllerData();
			MediaNotificationController.GetControllerData();
            NowPlayingController.GetControllerData();
        }

        /// <summary>
        /// Initialisze access to the persistent storage
        /// </summary>
        private void InitialiseStorage()
		{
			// Path to the locally stored database
			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			// The synchronous and aynchronous connectionn
			ConnectionDetailsModel.SynchConnection = new SQLiteConnection( databasePath );
			ConnectionDetailsModel.AsynchConnection = new SQLiteAsyncConnection( databasePath )
			{
				// Tracing when required
				Tracer = ( message ) => Logger.Log( message ),

				// Tracing currently required
				Trace = true
			};

			// Initialise the rest of the ConnectionDetailsModel if required
			ConnectionDetailsModel.LibraryId = InitialiseDatabase();
		}

		/// <summary>
		/// Make sure that the database exists and extract the current library
		/// </summary>
		private int InitialiseDatabase()
		{
			int currentLibraryId = -1;

			bool createTables = false;

			try
			{
				if ( createTables == true )
				{
					// Create the tables if they don't already exist
					ConnectionDetailsModel.SynchConnection.CreateTable<Library>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Source>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Artist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Album>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Song>();
					ConnectionDetailsModel.SynchConnection.CreateTable<ArtistAlbum>();
					ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<SongPlaylistItem>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Tag>();
					ConnectionDetailsModel.SynchConnection.CreateTable<TaggedAlbum>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Autoplay>();
					ConnectionDetailsModel.SynchConnection.CreateTable<GenrePopulation>();
					ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<AlbumPlaylistItem>();
				}

				// Check for a Playback record which will tell us the currently selected library
				currentLibraryId = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault().DBLibraryId;
			}
			catch ( SQLite.SQLiteException )
			{
			}

			return currentLibraryId;
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
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
		private static readonly SimpleHTTPServer localServer = new( "", 8080 );

		/// <summary>
		/// The DeviceDiscovery instance used to monitor the network and scan for DLNA devices
		/// </summary>
		private readonly DeviceDiscovery deviceDiscoverer = null;

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
		/// The PlaybackRouter used to pass playback requests to the the correct playback device
		/// </summary>
		private readonly PlaybackRouter playbackRouter = null;

		/// <summary>
		/// The control used to interface to the media notification service
		/// </summary>
		private readonly MediaNotificationServiceInterface mediaNotificationServiceInterface = null;

		/// <summary>
		/// The control used to interface to the application shutdown service
		/// </summary>
		private readonly ApplicationShutdownInterface applicationShutdownInterface = null;
	}
}
