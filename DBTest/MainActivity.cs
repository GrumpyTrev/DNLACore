using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace DBTest
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActionMode.ICallback, ArtistAlbumListViewAdapter.IArtistContentsProvider
    {

		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			SetContentView( Resource.Layout.activity_main );

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar );
			SetSupportActionBar( toolbar );

			listView = FindViewById<ExpandableListView>( Resource.Id.mainLayout );
			adapter = new ArtistAlbumListViewAdapter( this, listView, this );

			listView.SetAdapter( adapter );

			adapter.ActionModeRequested += Adapter_ActionModeRequested;

			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

//			if ( File.Exists( databasePath ) == true )
//			{
//				File.Delete( databasePath );
//			}

			try
			{
				dbAsynch = new SQLiteAsyncConnection( databasePath );

				db = new SQLiteConnection( databasePath );

				// Create a library entry and associated sources
				db.CreateTable<Library>();
				db.CreateTable<Source>();
				db.CreateTable<Artist>();
				db.CreateTable<Album>();
				db.CreateTable<Song>();
				db.CreateTable<ArtistAlbum>();

				// Check for an existing library
				Library songLibrary = db.GetAllWithChildren<Library>().SingleOrDefault();

				if ( songLibrary != null )
				{
					GetArtistDetails( songLibrary );
				}
				else
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
		}

		/// <summary>
		/// A request to enter action mode has been requested
		/// Display the Contextual Action Bar and move the Library list into action mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Adapter_ActionModeRequested( object sender, System.EventArgs e )
		{
			StartActionMode( this );

			( ( ArtistAlbumListViewAdapter )sender ).ActionMode = true;
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

			collapseItem = menu.FindItem( Resource.Id.action_collapse );
			collapseItem.SetVisible( false );

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
				adapter.OnCollapseRequest();
			}
			else if ( id == Resource.Id.action_playlist )
			{
				StartActivity( new Intent( this, typeof( PlayListActivity ) ) ); 
			}

            return base.OnOptionsItemSelected(item);
        }

		private async void GetArtistDetails( Library songLibrary )
		{
			// Get all of the artist details from the database
			for ( int artistIndex = 0; artistIndex < songLibrary.Artists.Count; ++artistIndex )
			{
				songLibrary.Artists[ artistIndex ] = await dbAsynch.GetAsync<Artist>( songLibrary.Artists[ artistIndex ].Id );
			}

			// Sort the list of artists by name
			songLibrary.Artists.Sort( ( a, b ) =>
			{
				// Do a normal comparison, except remove a leading 'The ' before comparing
				string artistA = a.Name;
				if ( a.Name.ToUpper().StartsWith( "THE " ) == true )
				{
					artistA = a.Name.Substring( 4 );
				}

				string artistB = b.Name;
				if ( b.Name.ToUpper().StartsWith( "THE " ) == true )
				{
					artistB = b.Name.Substring( 4 );
				}

				return artistA.CompareTo( artistB );
			} );

			// Work out the section indexes for the sorted data
			Dictionary< string, int >  alphaIndex = new Dictionary< string, int >();
			int index = 0;
			foreach ( Artist artist in songLibrary.Artists )
			{
				string key = artist.Name[ 0 ].ToString();
				if ( alphaIndex.ContainsKey( key ) == false )
				{
					alphaIndex[ key ] = index;
				}
				index++;
			}

			// Now load the adapter with this data
			adapter.SetData( songLibrary.Artists, alphaIndex );
		}

		public bool OnActionItemClicked( ActionMode mode, IMenuItem item )
		{
			return false;
		}

		public bool OnCreateActionMode( ActionMode mode, IMenu menu )
		{
			MenuInflater inflater = mode.MenuInflater;
			inflater.Inflate( Resource.Menu.action_mode, menu );
			return true;
		}

		public void OnDestroyActionMode( ActionMode mode )
		{
			adapter.ActionMode = false;
		}

		public bool OnPrepareActionMode( ActionMode mode, IMenu menu )
		{
			return false;
		}

		public void ProvideArtistContents( Artist theArtist )
		{
			db.GetChildren<Artist>( theArtist );

			// Sort the albums alphabetically
			theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				db.GetChildren<ArtistAlbum>( artistAlbum );

				// Sort the songs by track number
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}

			// Now all the ArtistAlbum and Song entries have been read form a single list from them
			theArtist.EnumerateContents();
		}

		public void ExpandedGroupCountChanged( int count )
		{
			collapseItem.SetVisible( count > 0 );
		}

		static SQLiteConnection db = null;
		static SQLiteAsyncConnection dbAsynch = null;

		static ArtistAlbumListViewAdapter adapter = null;

		private static IMenuItem collapseItem = null;

		private static ExpandableListView listView = null;
	}
}

