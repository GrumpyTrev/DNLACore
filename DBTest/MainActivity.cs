using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
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
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			SetContentView( Resource.Layout.activity_main );

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar );
			SetSupportActionBar( toolbar );

			tabLayout = FindViewById<TabLayout>( Resource.Id.sliding_tabs );

			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			//			if ( File.Exists( databasePath ) == true )
			//			{
			//				File.Delete( databasePath );
			//			}

			Library songLibrary = null;

			try
			{
				SQLiteConnection db = new SQLiteConnection( databasePath );

				// Create a library entry and associated sources
				db.CreateTable<Library>();
				db.CreateTable<Source>();
				db.CreateTable<Artist>();
				db.CreateTable<Album>();
				db.CreateTable<Song>();
				db.CreateTable<ArtistAlbum>();

				// Check for an existing library
				songLibrary = db.GetAllWithChildren<Library>().SingleOrDefault();

				if ( songLibrary == null )
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
				}
			}
			catch ( SQLite.SQLiteException queryException )
			{
			}

			InitialiseFragments( databasePath, songLibrary );
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

			return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
			if ( id == Resource.Id.action_settings )
			{
				return true;
			}
			else if ( id == Resource.Id.action_collapse )
			{
//				adapter.OnCollapseRequest();
			}
			else if ( id == Resource.Id.action_playlist )
			{
				StartActivity( new Intent( this, typeof( PlayListActivity ) ) );
			}
			else if ( id == Resource.Id.action_select )
			{
				libraryFragment.OnSelection();
			}

			return base.OnOptionsItemSelected(item);
        }

		private void InitialiseFragments( string databasePath, Library songLibrary )
		{
			libraryFragment = new LibraryFragment( databasePath, songLibrary );
			playlistsFragment = new PlaylistsFragment( databasePath, songLibrary );

			Android.Support.V4.App.Fragment[] fragments = new Android.Support.V4.App.Fragment[]
			{
				libraryFragment, playlistsFragment
			};

			//Tab title array
			Java.Lang.ICharSequence[] titles = CharSequence.ArrayFromStringArray( new[] { "Library", "Playlists" } );

			ViewPager viewPager = FindViewById<ViewPager>( Resource.Id.viewpager );

			//viewpager holding fragment array and tab title text
			viewPager.Adapter = new TabsFragmentPagerAdapter( SupportFragmentManager, fragments, titles );

			// Give the TabLayout the ViewPager 
			tabLayout.SetupWithViewPager( viewPager );

			// Detect page changes
			viewPager.AddOnPageChangeListener( new PageChangeListener() );
		}

		private class PageChangeListener: Java.Lang.Object, ViewPager.IOnPageChangeListener
		{
			public void OnPageScrolled( int position, float positionOffset, int positionOffsetPixels )
			{
			}

			public void OnPageScrollStateChanged( int state )
			{
			}

			public void OnPageSelected( int position )
			{
				// Position 0 = LibraryFragment
				if ( position == 0 )
				{
					libraryFragment.PageVisible( true );
				}
				else
				{
					libraryFragment.PageVisible( false );
				}
			}
		}

		private TabLayout tabLayout = null;

		private static LibraryFragment libraryFragment = null;
		private static PlaylistsFragment playlistsFragment = null;
	}
}

