using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Runtime;
using SQLite;
using static Android.App.Application;

namespace DBTest
{
	[Application]
	class MainApp : Application, IActivityLifecycleCallbacks
	{
		/// <summary>
		/// Base constructor which must be implemented if it is to successfully inherit from the Application
		/// class.
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="transfer"></param>
		public MainApp( IntPtr handle, JniHandleOwnership transfer ) : base( handle, transfer )
		{
			RegisterActivityLifecycleCallbacks( this );
		}

		public void OnActivityCreated( Activity activity, Bundle savedInstanceState )
		{
		}

		public void OnActivityDestroyed( Activity activity )
		{
		}

		public void OnActivityPaused( Activity activity )
		{
			Logger.Log( "MainApp::OnActivityPaused()" );
			Foreground = false;
		}

		public void OnActivityResumed( Activity activity )
		{
			Logger.Log( "MainApp::OnActivityResumed()" );
			Foreground = true;
		}

		public void OnActivitySaveInstanceState( Activity activity, Bundle outState )
		{
		}

		public void OnActivityStarted( Activity activity )
		{
		}

		public void OnActivityStopped( Activity activity )
		{
		}

		/// <summary>
		/// Called when the application is starting, before any other application objects have been created (like MainActivity).
		/// Perform and process scope initialisation
		/// </summary>
		public override void OnCreate()
		{
			// Path to the locally stored database
			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			// Check if the database connections are still there. They will be on an activity restart (configuration change)
			if ( ConnectionDetailsModel.SynchConnection == null )
			{
				ConnectionDetailsModel.SynchConnection = new SQLiteConnection( databasePath );
				ConnectionDetailsModel.AsynchConnection = new SQLiteAsyncConnection( databasePath );
			}

			// Initialise the rest of the ConnectionDetailsModel if required
			ConnectionDetailsModel.LibraryId = InitialiseDatabase();
			// Bind the command handlers to their command identities
			CommandRouter.BindHandlers();

			AlbumsController.GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
			ArtistsController.GetArtistsAsync( ConnectionDetailsModel.LibraryId );
			PlaylistsController.GetPlaylistsAsync( ConnectionDetailsModel.LibraryId );
			NowPlayingController.GetNowPlayingListAsync( ConnectionDetailsModel.LibraryId );
			FilterManagementController.GetTagsAsync();

			base.OnCreate();
		}

		/// <summary>
		/// Make sure that the database exists and extract the current library
		/// </summary>
		private int InitialiseDatabase()
		{
			int currentLibraryId = -1;

			bool createTables = false;
			bool dropGenres = false;
			bool changeSource = false;

			try
			{
				if ( dropGenres == true )
				{
					ConnectionDetailsModel.SynchConnection.DropTable<Genre>();
					ConnectionDetailsModel.SynchConnection.CreateTable<Genre>();
				}

				if ( changeSource == true )
				{
					ConnectionDetailsModel.SynchConnection.CreateTable<Source>();
				}

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
					ConnectionDetailsModel.SynchConnection.CreateTable<Genre>();
				}

				// Check for a Playback record which will tell us the currently selected library
				currentLibraryId = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault().LibraryId;
			}
			catch ( SQLite.SQLiteException )
			{
			}

			return currentLibraryId;
		}

		public static bool Foreground { get; private set; } = false;

		/// <summary>
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
		private static readonly SimpleHTTPServer localServer = new SimpleHTTPServer( "", 8080 );
	}
}