using System.IO;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SQLite;

namespace DBTest
{
	[Activity( Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true )]
	public class MainActivity: AppCompatActivity, Logger.ILogger, LibraryNameDisplayController.IReporter
	{
		/// <summary>
		/// Called to create the UI components of the activity
		/// </summary>
		/// <param name="savedInstanceState"></param>
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Create the view hierarchy
			View view = LayoutInflater.Inflate( Resource.Layout.activity_main, null );
			SetContentView( view );

			// Set the main top toolbar
			SetSupportActionBar( FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar ) );

			// Set up logging
			Logger.Reporter = this;

			// Path to the locally stored database
			string databasePath = Path.Combine( Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Test.db3" );

			// Check if the database connections are still there. They will be on an activity restart (configuration change)
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

			// Start the tab view data access as early as possible
			ArtistsController.GetArtistsAsync( ConnectionDetailsModel.LibraryId );
			AlbumsController.GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
			PlaylistsController.GetPlaylistsAsync( ConnectionDetailsModel.LibraryId );
			NowPlayingController.GetNowPlayingListAsync( ConnectionDetailsModel.LibraryId );

			// Initialise the PlaybackRouter
			playbackRouter = new PlaybackRouter( this, FindViewById<LinearLayout>( Resource.Id.mainLayout ) );

			// Initialise the PlaybackSelectionManager
			playbackSelector = new PlaybackSelectionManager( this );

			// Initialise the LibraryScanner
			libraryScanner = new LibraryScanner( this );

			// Initialise the LibrarySelection
			librarySelector = new LibrarySelection( this );

			// Initialise the LibraryClear
			libraryClearer = new LibraryClear( this );

			// Initialise the tag command handlers
			tagDeleteCommandHandler = new TagDeletor( this );
			tagEditCommandHandler = new TagEditor( this );

			// Link in to the LibraryNameDisplayController to be informed of library name changes
			LibraryNameDisplayController.Reporter = this;
			LibraryNameDisplayController.GetCurrentLibraryNameAsync();

			// Make sure someone reads the available filters before they are needed
			FilterManagementController.GetTagsAsync();

			// Start the router and selector - via a Post so that any response comes back after the UI has been created
			// This didn't work when placed in OnStart()
			view.Post( () => {
				playbackRouter.StartRouter();
				playbackSelector.StartSelection();
			} );

			if ( Build.VERSION.SdkInt >= BuildVersionCodes.M )
			{
				// Make sure that this application is not subject to battery optimisations
				if ( ( ( PowerManager )GetSystemService( Context.PowerService ) ).IsIgnoringBatteryOptimizations( PackageName ) == false )
				{
					StartActivity( new Intent().SetAction( Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations )
						.SetData( Uri.Parse( "package:" + PackageName ) ) );
				}
			}

			// Initialise the fragments showing the selected library
			InitialiseFragments();
		}

		/// <summary>
		/// Called to create the main toolbar menu
		/// </summary>
		/// <param name="menu"></param>
		/// <returns></returns>
		public override bool OnCreateOptionsMenu( IMenu menu )
		{
			MenuInflater.Inflate( Resource.Menu.menu_main, menu );

			// Keep a reference to the repeat off menu item
			repeatOffMenu = menu.FindItem( Resource.Id.action_repeat_off );
			repeatOffMenu.SetVisible( PlaybackManagerModel.RepeatOn );

			return true;
		}

		/// <summary>
		/// Called just before the options menu is shown
		/// </summary>
		/// <param name="menu"></param>
		/// <returns></returns>
		public override bool OnPrepareOptionsMenu( IMenu menu )
		{
			// Enable or disable the playback visible item according to the current media controller visibility
			menu.FindItem( Resource.Id.show_media_controls ).SetEnabled( playbackRouter.PlaybackControlsVisible == false );

			// Change the text for the repeat item according to the repeat mode
			menu.FindItem( Resource.Id.repeat_on_off ).SetTitle( PlaybackManagerModel.RepeatOn ? "Repeat off" : "Repeat on" );

			// Populate the rename and delete tag menus with submenus containing the user tags items
			int menuId = Menu.First;

			IMenuItem renameTag = menu.FindItem( Resource.Id.edit_tag );
			if ( renameTag != null )
			{
				tagEditCommandHandler.PrepareMenu( renameTag, ref menuId );
			}

			IMenuItem deleteTag = menu.FindItem( Resource.Id.delete_tag );
			if ( deleteTag != null )
			{
				tagDeleteCommandHandler.PrepareMenu( deleteTag, ref menuId );
			}

			return base.OnPrepareOptionsMenu( menu );
		}

		/// <summary>
		/// Called when one of the main toolbar menu items has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			bool handled = false;

			int id = item.ItemId;

			// Check for the device selection option
			if ( id == Resource.Id.select_playback_device )
			{
				playbackSelector.ShowSelection();
				handled = true;
			}
			// Check for the show media UI option
			else if ( id == Resource.Id.show_media_controls )
			{
				playbackRouter.PlaybackControlsVisible = true;
				handled = true;
			}
			else if ( id == Resource.Id.scan_library )
			{
				libraryScanner.ScanSelection();
				handled = true;
			}
			else if ( id == Resource.Id.select_library )
			{
				librarySelector.SelectLibrary();
				handled = true;
			}
			else if ( id == Resource.Id.clear_library )
			{
				libraryClearer.SelectLibraryToClear();
				handled = true;
			}
			else if ( id == Resource.Id.shuffle_now_playing )
			{
				NowPlayingController.ShuffleNowPlayingList();
				handled = true;
			}
			else if ( tagEditCommandHandler.OnOptionsItemSelected( id, item.TitleFormatted.ToString() ) == true )
			{
				handled = true;
			}
			else if ( tagDeleteCommandHandler.OnOptionsItemSelected( id, item.TitleFormatted.ToString() ) == true )
			{
				handled = true;
			}
			else if ( id == Resource.Id.add_tag )
			{
				TagCreator.AddNewTag( this );
				handled = true;
			}
			else if ( ( id == Resource.Id.repeat_on_off ) || ( id == Resource.Id.action_repeat_off ) )
			{
				// Toggle the repeat state
				PlaybackManagerModel.RepeatOn = ! PlaybackManagerModel.RepeatOn;
				repeatOffMenu.SetVisible( PlaybackManagerModel.RepeatOn );
				handled = true;
			}

			// If the selection has not been handled pass it on to the base class
			if ( handled == false )
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Called when the library name has been obtained at start-up or if it has changed
		/// </summary>
		/// <param name="libraryName"></param>
		public void LibraryNameAvailable( string libraryName ) => SupportActionBar.Title = libraryName;

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="message"></param>
		public void Log( string message ) => Android.Util.Log.WriteLine( Android.Util.LogPriority.Debug, "DBTest", message );

		/// <summary>
		/// Report an event 
		/// </summary>
		/// <param name="message"></param>
		public void Event( string message ) => RunOnUiThread( () => Toast.MakeText( this, message, ToastLength.Short ).Show() );

		/// <summary>
		/// Report an error
		/// </summary>
		/// <param name="message"></param>
		public void Error( string message ) => RunOnUiThread( () => Toast.MakeText( this, message, ToastLength.Long ).Show() );

		/// <summary>
		/// Called when the activity is being closed down.
		/// This can either be temporary to respond to a configuration change (rotation),
		/// or permanent if the user or system is shutting down the application
		/// </summary>
		protected override void OnDestroy()
		{
			// Remove any registrations made by components that are just about to be destroyed
			Mediator.RemoveTemporaryRegistrations();

			// Stop any media playback
			playbackRouter.StopRouter( ( IsFinishing == true ) );

			// If the activity is being permanently destroyed get rid of the synchronous and asynchronous connections
			if ( IsFinishing == true )
			{
				ConnectionDetailsModel.SynchConnection.Dispose();
				ConnectionDetailsModel.SynchConnection = null;

				SQLite.SQLiteAsyncConnection.ResetPool();
				ConnectionDetailsModel.AsynchConnection = null;

				localServer.Stop();
			}

			// Some of the managers need to remove themselves from the scene
			libraryScanner.ReleaseResources();
			FragmentTitles.ParentActivity = null;
			LibraryNameDisplayController.Reporter = null;

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
				ConnectionDetailsModel.SynchConnection.CreateTable<Playlist>();
				ConnectionDetailsModel.SynchConnection.CreateTable<PlaylistItem>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Playback>();
				ConnectionDetailsModel.SynchConnection.CreateTable<Tag>();
				ConnectionDetailsModel.SynchConnection.CreateTable<TaggedAlbum>();

				// Check for a Playback record which will tell us the currently selected library
				Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();

				currentLibraryId = playbackRecord.LibraryId;
			}
			catch ( SQLite.SQLiteException )
			{
			}

			return currentLibraryId;
		}

		/// <summary>
		/// Initialise the fragments showing the library contents
		/// </summary>
		private void InitialiseFragments()
		{
			// Create the fragments and give them titles
			Android.Support.V4.App.Fragment[] fragments = 
				new Android.Support.V4.App.Fragment[]
				{
					new ArtistsFragment(), new AlbumsFragment(), new PlaylistsFragment(), new NowPlayingFragment()
				};

			// Initialise the Fragment titles class
			FragmentTitles.SetInitialTitles( new[] { "Artists", "Albums", "Playlists", "Now Playing" }, fragments );

			// Get the ViewPager and link it to a TabsFragmentPagerAdapter
			ViewPager viewPager = FindViewById<ViewPager>( Resource.Id.viewPager );

			// Set the adapter for the pager
			viewPager.Adapter = new TabsFragmentPagerAdapter( SupportFragmentManager, fragments, FragmentTitles.GetTitles() );

			// Give the TabLayout the ViewPager 
			FindViewById<TabLayout>( Resource.Id.sliding_tabs ).SetupWithViewPager( viewPager );

			// Now that everything's been linked together let the FragmentTitles do some of it own initialisation
			FragmentTitles.ParentActivity = this;
		}

		/// <summary>
		/// The PlaybackRouter used to route playback commands to the selected device
		/// </summary>
		private PlaybackRouter playbackRouter = null;

		/// <summary>
		/// The PlaybackSelectionManager used to allow the user to select a playback device
		/// </summary>
		private PlaybackSelectionManager playbackSelector = null;

		/// <summary>
		/// The LibraryScanner class controls the rescanning of a library
		/// </summary>
		private LibraryScanner libraryScanner = null;

		/// <summary>
		/// The LibrarySelection class controls the selection of a library to be displayed
		/// </summary>
		private LibrarySelection librarySelector = null;

		/// <summary>
		/// The LibraryClear class controls the clearance of a library
		/// </summary>
		private LibraryClear libraryClearer = null;

		/// <summary>
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
		private static SimpleHTTPServer localServer = new SimpleHTTPServer( "", 8080 );

		/// <summary>
		/// The handler for the tag deletion command
		/// </summary>
		private TagDeletor tagDeleteCommandHandler = null;

		/// <summary>
		/// The handler for the tag editor command
		/// </summary>
		private TagEditor tagEditCommandHandler = null;

		/// <summary>
		/// A reference to the repeat off menu item so that it can be shown or hidden
		/// </summary>
		private IMenuItem repeatOffMenu = null;
	}
}

