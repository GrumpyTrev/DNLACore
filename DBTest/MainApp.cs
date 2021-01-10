using System;
using System.IO;
using Android.App;
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

			// Initialise the newtwork monitoring
			playbackCapabilities = new PlaybackCapabilities( Context );

			// Initialise the playback device monitoring
			playbackMonitoring = new PlaybackMonitor();
		}

		/// Add the specified interface to the callback colletion
		/// </summary>
		/// <param name="callback"></param>
		public static void RegisterPlaybackCapabilityCallback( PlaybackCapabilities.IPlaybackCapabilitiesChanges callback ) => 
			instance.playbackCapabilities.RegisterCallback( callback );

		/// <summary>
		/// Bind the playback monitor to the specified menu
		/// </summary>
		/// <param name="menu"></param>
		public static void BindToPlaybackMonitor( IMenu menu ) => instance.playbackMonitoring.BindToMenu( menu );

		/// <summary>
		/// OnCreate needs to be overwritten otherwise Android does not create the MainApp class until it wnats to - strange but true
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();
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
			PlaybackSelectionController.GetPlaybackDetails();
			AutoplayController.GetControllerData();
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
//					ConnectionDetailsModel.SynchConnection.DropTable<Autoplay>();
//					ConnectionDetailsModel.SynchConnection.DropTable<GenrePopulation>();

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
				currentLibraryId = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault().LibraryId;
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
		private readonly PlaybackMonitor playbackMonitoring = null;
	}
}