using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace DBTest
{
	[Activity( Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true )]
	public class MainActivity: AppCompatActivity
	{
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			View view = LayoutInflater.Inflate( Resource.Layout.activity_main, null );
			SetContentView( view );

			SetSupportActionBar( FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar ) );

			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			// Check if the database connections are still tehre
			if ( ConnectionDetailsModel.SynchConnection == null )
			{
				ConnectionDetailsModel.SynchConnection = new SQLiteConnection( databasePath );
				ConnectionDetailsModel.AsynchConnection = new SQLiteAsyncConnection( databasePath );
			}

			// Initialise the rest of the ConnectionDetailsModel if required
			if ( ConnectionDetailsModel.DatabasePath.Length == 0 )
			{
				// Save the database path and get the current library
				ConnectionDetailsModel.DatabasePath = databasePath;
				ConnectionDetailsModel.LibraryId = InitialiseDatabase();
			}

			//			if ( File.Exists( databasePath ) == true )
			//			{
			//				File.Delete( databasePath );
			//			}

			// Request the Now Playing list from the library - via a Post so that any response comes back after the UI has been created
			view.Post( () => {
				// Initialise the PlaybackRouter
				playbackRouter = new PlaybackRouter( this, FindViewById<LinearLayout>( Resource.Id.mainLayout ) );
				playbackRouter.StartRouter();

				// Initialise the PlaybackSelectionManager
				playbackSelector = new PlaybackSelectionManager( this );
				playbackSelector.StartSelection();
			} );



			InitialiseFragments( savedInstanceState );
		}

		protected override void OnStart()
		{
			base.OnStart();

			// Initialise the PlaybackRouter
//			playbackRouter = new PlaybackRouter( this, FindViewById<LinearLayout>( Resource.Id.mainLayout ) );
//			playbackRouter.StartRouter();

			// Initialise the PlaybackSelectionManager
//			playbackSelector = new PlaybackSelectionManager( this );
//			playbackSelector.StartSelection();

		}

		public override bool OnCreateOptionsMenu( IMenu menu )
		{
			MenuInflater.Inflate( Resource.Menu.menu_main, menu );

			return true;
		}

		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			int id = item.ItemId;
			if ( id == Resource.Id.action_settings )
			{
				playbackSelector.ShowSelection();
				return true;
			}

			return base.OnOptionsItemSelected( item );
		}

		protected override void OnDestroy()
		{
			Mediator.RemoveTemporaryRegistrations();

			playbackRouter.StopRouter( ( IsFinishing == true ) );

			if ( IsFinishing == true )
			{
				ConnectionDetailsModel.SynchConnection.Dispose();
				ConnectionDetailsModel.SynchConnection = null;

				SQLite.SQLiteAsyncConnection.ResetPool();
				ConnectionDetailsModel.AsynchConnection = null;
			}

			base.OnDestroy();
		}

		/// <summary>
		/// Make sure that the database exists and extract the current library
		/// </summary>
		private int InitialiseDatabase()
		{
			int currentLibraryId = -1;

			try
			{
				// Create the tables if they don't already exist
				ConnectionDetailsModel.SynchConnection.CreateTable<Library>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Source>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Artist>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Album>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Song>();
				ConnectionDetailsModel.SynchConnection.CreateTable<ArtistAlbum>();

				//				db.DropTable<Playlist>();

				ConnectionDetailsModel.SynchConnection.CreateTable<Playlist>();
				ConnectionDetailsModel.SynchConnection.CreateTable<PlaylistItem>();

				//				ConnectionDetailsModel.SynchConnection.DropTable<Playback>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();

				// Check for a Playback record which will tell us the currently selected library
				Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();

				if ( ( playbackRecord == null ) || ( playbackRecord.LibraryId == -1 ) )
				{
					// Current library is not specified so find one now
					// Check for an existing library
					Library currentLibrary = ConnectionDetailsModel.SynchConnection.Table<Library>().FirstOrDefault();

					if ( currentLibrary == null )
					{
						// For debugging - setup a single library
						Library lib1 = new Library() { Name = "Remote" };

						Source source1 = new Source() {
							Name = "Laptop", ScanSource = "192.168.1.5", ScanType = "FTP",
							AccessSource = "http://192.168.1.5:80/RemoteMusic/", AccessType = "HTTP"
						};
						source1.Songs = new List<Song>();

						Source source2 = new Source() {
							Name = "Phone", ScanSource = "/storage/emulated/0/Music/", ScanType = "Local",
							AccessSource = "/storage/emulated/0/Music/", AccessType = "Local"
						};
						source2.Songs = new List<Song>();

						ConnectionDetailsModel.SynchConnection.Insert( lib1 );
						ConnectionDetailsModel.SynchConnection.Insert( source1 );
						ConnectionDetailsModel.SynchConnection.Insert( source2 );

						lib1.Sources = new List<Source> { source1, source2 };
						lib1.Artists = new List<Artist>();
						lib1.Albums = new List<Album>();

						ConnectionDetailsModel.SynchConnection.UpdateWithChildren( lib1 );

						// Read the library definitions and process
						TableQuery<Library> libraries = ConnectionDetailsModel.SynchConnection.Table<Library>();
						foreach ( Library lib in libraries )
						{
							LibraryScanner scanner = new LibraryScanner( lib, ConnectionDetailsModel.SynchConnection );
							scanner.ScanLibrary();
						}

						currentLibrary = ConnectionDetailsModel.SynchConnection.GetAllWithChildren<Library>().SingleOrDefault();

						// Check for any playlists associated with the current library
						List<Playlist> playlists = ConnectionDetailsModel.SynchConnection.Table<Playlist>().
							Where( d => ( d.LibraryId == currentLibrary.Id ) ).ToList();

						if ( playlists.Count == 0 )
						{
							// Add some playlists to the current library
							// First fully load the library so that it can be updated
							ConnectionDetailsModel.SynchConnection.GetChildren<Library>( currentLibrary );

							// Add PlayLists to the databse and the library
							Playlist list = new Playlist() { Name = "Playlist 1" };
							ConnectionDetailsModel.SynchConnection.Insert( list );
							currentLibrary.PlayLists.Add( list );

							list = new Playlist() { Name = "Playlist 2" };
							ConnectionDetailsModel.SynchConnection.Insert( list );
							currentLibrary.PlayLists.Add( list );

							list = new Playlist() { Name = NowPlayingController.NowPlayingPlaylistName };
							ConnectionDetailsModel.SynchConnection.Insert( list );
							currentLibrary.PlayLists.Add( list );

							ConnectionDetailsModel.SynchConnection.UpdateWithChildren( currentLibrary );
						}
					}

					// We now have a current library. If we don't have a Playback record then create one now
					if ( playbackRecord == null )
					{
						playbackRecord = new Playback();
						playbackRecord.SongIndex = -1;
						ConnectionDetailsModel.SynchConnection.Insert( playbackRecord );
					}

					playbackRecord.LibraryId = currentLibrary.Id;
					playbackRecord.PlaybackDeviceName = "Local playback";

					ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
				}

				currentLibraryId = playbackRecord.LibraryId;
			}
			catch ( SQLite.SQLiteException queryException )
			{
			}

			return currentLibraryId;
		}

		private void InitialiseFragments( Bundle savedInstanceState )
		{
			Android.Support.V4.App.Fragment[] fragments = 
				new Android.Support.V4.App.Fragment[]
				{
					new ArtistsFragment(), new PlaylistsFragment(), new NowPlayingFragment()
				};

			// Tab title array
			Java.Lang.ICharSequence[] titles = CharSequence.ArrayFromStringArray( new[] { "Library", "Playlists", "Now Playing" } );

			ViewPager viewPager = FindViewById<ViewPager>( Resource.Id.viewpager );

			// Viewpager holding fragment array and tab title text
			TabsFragmentPagerAdapter pageAdapter = new TabsFragmentPagerAdapter( SupportFragmentManager, fragments, titles );

			// Detect page changes
			PageChangeListener pageListener = new PageChangeListener( pageAdapter );
			viewPager.AddOnPageChangeListener( pageListener );

			// Set the adapter for the pager
			viewPager.Adapter = pageAdapter;

			// Give the TabLayout the ViewPager 
			FindViewById<TabLayout>( Resource.Id.sliding_tabs ).SetupWithViewPager( viewPager );

			// Attempt to restore the currently selected tab.
			// Only seems to work if performed after this method has finished, hence the post
			viewPager.Post( new PagerRunnable() { Pager = viewPager, Listener = pageListener } );
		}

		private PlaybackRouter playbackRouter = null;

		private PlaybackSelectionManager playbackSelector = null;

		/// <summary>
		/// The PagerRunnable class is used to restore a currently selected tab (after rotation)
		/// Need to be performed via a Run 
		/// </summary>
		private class PagerRunnable: Java.Lang.Object, Java.Lang.IRunnable
		{
			public void Run()
			{
				Listener.OnPageSelected( Pager.CurrentItem );
			}

			public ViewPager Pager { get; set; } = null;
			public PageChangeListener Listener { get; set; } = null;
		}

		private class PageChangeListener: Java.Lang.Object, ViewPager.IOnPageChangeListener
		{
			public PageChangeListener( TabsFragmentPagerAdapter pageAdapter )
			{
				adapter = pageAdapter;
			}

			public void OnPageScrolled( int position, float positionOffset, int positionOffsetPixels )
			{
			}

			public void OnPageScrollStateChanged( int state )
			{
			}

			public void OnPageSelected( int position )
			{
				if ( visibleFragment != position )
				{
					if ( visibleFragment != -1 )
					{
						( ( IPageVisible )adapter.GetItem( visibleFragment ) ).PageVisible( false );
					}

					( ( IPageVisible )adapter.GetItem( position ) ).PageVisible( true );
					visibleFragment = position;
				}
			}

			private int visibleFragment = -1;
			private TabsFragmentPagerAdapter adapter = null;
		}
	}
}

