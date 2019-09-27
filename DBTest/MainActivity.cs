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

			SetContentView( Resource.Layout.activity_main );

			SetSupportActionBar( FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar ) );

			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			// Get the ConnectionDetailsModel and initialise it if required
			ConnectionDetailsModel model = StateModelProvider.Get( typeof( ConnectionDetailsModel ) ) as ConnectionDetailsModel;

			if ( model.IsNew == true )
			{
				// Save the database path and get the current library
				model.DatabasePath = databasePath;
				model.LibraryId = InitialiseDatabase( databasePath );
			}

			//			if ( File.Exists( databasePath ) == true )
			//			{
			//				File.Delete( databasePath );
			//			}

			// Initialise the view controllers
			ArtistsController.DatabasePath = databasePath;
			PlaylistsController.DatabasePath = databasePath;
			NowPlayingController.DatabasePath = databasePath;

			InitialiseFragments( savedInstanceState );
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
				return true;
			}

			return base.OnOptionsItemSelected( item );
		}

		protected override void OnDestroy()
		{
			Mediator.RemoveTemporaryRegistrations();

			base.OnDestroy();
		}

		/// <summary>
		/// Make sure that the database exists and extract the current library
		/// </summary>
		private int InitialiseDatabase( string databasePath )
		{
			int currentLibraryId = -1;

			try
			{
				SQLiteConnection db = new SQLiteConnection( databasePath );

				// Create the tables if they don't already exist
				db.CreateTable<Library>();
				db.CreateTable<Source>();
				db.CreateTable<Artist>();
				db.CreateTable<Album>();
				db.CreateTable<Song>();
				db.CreateTable<ArtistAlbum>();

				db.DropTable<Playlist>();

				db.CreateTable<Playlist>();
				db.CreateTable<PlaylistItem>();

				// Check for an existing library
				Library currentLibrary = db.Table<Library>().FirstOrDefault();

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

					db.Insert( lib1 );
					db.Insert( source1 );
					db.Insert( source2 );

					lib1.Sources = new List<Source> { source1, source2 };
					lib1.Artists = new List<Artist>();
					lib1.Albums = new List<Album>();

					db.UpdateWithChildren( lib1 );

					// Read the library definitions and process
					TableQuery<Library> libraries = db.Table<Library>();
					foreach ( Library lib in libraries )
					{
						LibraryScanner scanner = new LibraryScanner( lib, db );
						scanner.ScanLibrary();
					}

					currentLibrary = db.GetAllWithChildren<Library>().SingleOrDefault();
				}

				// Check for any playlists associated with the current library
				List<Playlist> playlists = db.Table<Playlist>().Where( d => ( d.LibraryId == currentLibrary.Id ) ).ToList();

				if ( playlists.Count == 0 )
				{
					// Add some playlists to the current library
					// First fully load the library so that it can be updated
					db.GetChildren<Library>( currentLibrary );

					// Add PlayLists to the databse and the library
					Playlist list = new Playlist() { Name = "Playlist 1" };
					db.Insert( list );
					currentLibrary.PlayLists.Add( list );

					list = new Playlist() { Name = "Playlist 2" };
					db.Insert( list );
					currentLibrary.PlayLists.Add( list );

					list = new Playlist() { Name = NowPlayingController.NowPlayingPlaylistName };
					db.Insert( list );
					currentLibrary.PlayLists.Add( list );

					db.UpdateWithChildren( currentLibrary );
				}

				currentLibraryId = currentLibrary.Id;
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
					new LibraryFragment(), new PlaylistsFragment(), new NowPlayingFragment()
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

