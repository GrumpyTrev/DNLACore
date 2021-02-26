using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using SQLite;

namespace DBTest
{
	[Application]
	class MainApp : Application
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

			// Initialise the storage
			InitialiseStorage();

			// Bind the command handlers to their command identities
			CommandRouter.BindHandlers();

			// Configure the controllers
			ConfigureControllers();

			// Initialise the network monitoring
			playbackCapabilities = new PlaybackCapabilities( Context );

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
		public static void RegisterPlaybackCapabilityCallback( PlaybackCapabilities.IPlaybackCapabilitiesChanges callback ) => 
			instance.playbackCapabilities.RegisterCallback( callback );

		/// <summary>
		/// Bind the controls to the menu
		/// </summary>
		/// <param name="menu"></param>
		public static void BindMenu( IMenu menu, Context context, View activityContent )
		{
			instance.playbackMonitoring.BindToMenu( menu, context, activityContent );
			instance.playbackModeViewer.BindToMenu( menu, context, activityContent );
			instance.libraryNameDisplay.BindToMenu( menu, context, activityContent );
			instance.mediaControllerInterface.BindToMenu( menu, context, activityContent );
		}

		/// <summary>
		/// Bind the controls to the view
		/// </summary>
		/// <param name="view"></param>
		/// <param name="context"></param>
		public static void BindView( View view, Context context )
		{
			instance.playbackMonitoring.BindToView( view, context );
			instance.playbackModeViewer.BindToView( view, context );
			instance.libraryNameDisplay.BindToView( view, context );
			instance.mediaControllerInterface.BindToView( view, context );
		}

		/// <summary>
		/// OnCreate needs to be overwritten otherwise Android does not create the MainApp class until it wnats to - strange but true
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();
		}

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
			NowPlayingController.GetControllerData();
			LibraryNameDisplayController.GetControllerData();
			FilterManagementController.GetControllerData();
			PlaybackSelectionController.GetControllerData();
			AutoplayController.GetControllerData();
			PlaybackModeController.GetControllerData();
			PlaybackManagementController.GetControllerData();
			DisplayGenreController.GetControllerData();
			MediaControllerController.GetControllerData();
			MediaNotificationController.GetControllerData();
		}

		/// <summary>
		/// Initialisze access to the persistent storage
		/// </summary>
		private void InitialiseStorage()
		{
			// Path to the locally stored database
			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			ConnectionDetailsModel.SynchConnection = new SQLiteConnection( databasePath );
			ConnectionDetailsModel.AsynchConnection = new SQLiteAsyncConnection( databasePath );

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
					ConnectionDetailsModel.SynchConnection.CreateTable<Playlist>();
					ConnectionDetailsModel.SynchConnection.CreateTable<PlaylistItem>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Tag>();
					ConnectionDetailsModel.SynchConnection.CreateTable<TaggedAlbum>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Autoplay>();
					ConnectionDetailsModel.SynchConnection.CreateTable<GenrePopulation>();
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
		/// THe one and only MainApp
		/// </summary>
		private static MainApp instance = null;

		/// <summary>
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
		private static readonly SimpleHTTPServer localServer = new SimpleHTTPServer( "", 8080 );

		/// <summary>
		/// The PlaybackCapabilities instance used to monitor the network and scan for DLNA devices
		/// </summary>
		private readonly PlaybackCapabilities playbackCapabilities = null;

		/// <summary>
		/// The PlaybackMonitor instance used to monitor the state of the playback system
		/// </summary>
		private readonly PlaybackMonitor playbackMonitoring = new PlaybackMonitor();

		/// <summary>
		/// The PlaybackModeView instance used to display the playback mode and to allow it to be changed
		/// </summary>
		private readonly PlaybackModeView playbackModeViewer = new PlaybackModeView();

		/// <summary>
		/// The LibraryNameDisplay instance used to display the library name
		/// </summary>
		private readonly LibraryNameDisplay libraryNameDisplay = new LibraryNameDisplay();

		/// <summary>
		/// The control user to interface to the media controller
		/// </summary>
		private readonly MediaControllerInterface mediaControllerInterface = new MediaControllerInterface();

		/// <summary>
		/// The PlaybackRouter used to pass playback requests to the the correct playback device
		/// </summary>
		private readonly PlaybackRouter playbackRouter = null;

		/// <summary>
		/// The control used to interface to the media notification service
		/// </summary>
		private readonly MediaNotificationServiceInterface mediaNotificationServiceInterface = null;

		/// <summary>
		/// The control used to interface to the application shutdwon service
		/// </summary>
		private readonly ApplicationShutdownInterface applicationShutdownInterface = null;
	}
}