using System;
using SQLite;

namespace CoreMP
{
	public class CoreMPApp
	{
		/// <summary>
		/// Base constructor
		/// </summary>
		public CoreMPApp()
		{
			instance = this;

			// Initialise the network monitoring
			deviceDiscoverer = new DeviceDiscovery();
		}

		/// <summary>
		/// Save the interface provided by the UI implementation
		/// </summary>
		/// <param name="reporter"></param>
		public void SetInterface( ICoreMP uiInterface )
		{
			coreInterface = uiInterface;
			Logger.Reporter = uiInterface;
		}

		/// <summary>
		/// Initialise the CoreMP library
		/// </summary>
		public void Initialise()
		{
			InitialiseStorage( coreInterface.StoragePath );

			// Configure the controllers
			ConfigureControllers();
		}

		/// <summary>
		/// Pass on wifi state changes to the DeviceDiscoverer instance
		/// </summary>
		/// <param name="wifiAvailable"></param>
		public void WifiStateChanged( bool wifiAvailable ) => deviceDiscoverer.OnWiFiStateChanged( wifiAvailable );

		/// <summary>
		/// Add the specified interface to the callback colletion
		/// </summary>
		/// <param name="callback"></param>
		public static void RegisterPlaybackCapabilityCallback( DeviceDiscovery.IDeviceDiscoveryChanges callback ) =>
			instance.deviceDiscoverer.RegisterCallback( callback );

		/// <summary>
		/// Post an Action onto the UI thread
		/// </summary>
		/// <param name="actionToPost"></param>
		public static void Post( Action actionToPost ) => coreInterface.PostAction( actionToPost );

		/// <summary>
		/// Commander class used to pass all commands on to
		/// </summary>
		public Commander CommandInterface { get; } = new Commander();

		/// <summary>
		/// Configure all the controllers
		/// </summary> 
		private void ConfigureControllers()
		{
			AlbumsController.GetControllerData();
			ArtistsController.GetControllerData();
			PlaylistsController.GetControllerData();
			LibraryNameDisplayController.GetControllerData();
			LibraryManagementController.GetControllerData();
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
        private void InitialiseStorage( string databasePath )
		{
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
		/// The one and only MainApp
		/// </summary>
		private static CoreMPApp instance = null;

		/// <summary>
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
		private static readonly SimpleHTTPServer localServer = new SimpleHTTPServer( "", 8080 );

		/// <summary>
		/// The DeviceDiscovery instance used to monitor the network and scan for DLNA devices
		/// </summary>
		private readonly DeviceDiscovery deviceDiscoverer = null;

		/// <summary>
		/// The interface used to access the UI system
		/// </summary>
		private static ICoreMP coreInterface = null;
	}
}
