using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

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

			// Pass on the fragment manager to the CommandRouter
			CommandRouter.Manager = SupportFragmentManager;

			// Initialise the fragments showing the selected library
			InitialiseFragments();

			// Initialise the PlaybackRouter
			playbackRouter = new PlaybackRouter( this, FindViewById<LinearLayout>( Resource.Id.mainLayout ) );

			// Initialise the tag command handlers
			tagDeleteCommandHandler = new TagDeletor( this );
			tagEditCommandHandler = new TagEditor( this );

			// Link in to the LibraryNameDisplayController to be informed of library name changes
			LibraryNameDisplayController.Reporter = this;
			LibraryNameDisplayController.GetCurrentLibraryNameAsync();

			// Start the router and selector - via a Post so that any response comes back after the UI has been created
			// This didn't work when placed in OnStart() or OnResume(). Not sure why.
			view.Post( () => {
				playbackRouter.StartRouter();
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

			// Bind to any process wide command handler or monitors that require a menu item
			MainApp.BindToPlaybackMonitor( menu );

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

			// Let the CommandRouter have first go
			if ( CommandRouter.HandleCommand( id ) == true )
			{
				handled = true;
			}
			// Check for the show media UI option
			else if ( id == Resource.Id.show_media_controls )
			{
				playbackRouter.PlaybackControlsVisible = true;
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

			// Some of the managers need to remove themselves from the scene
			FragmentTitles.ParentActivity = null;
			LibraryNameDisplayController.Reporter = null;

			// Unbind from any process wide command handler or monitors that require a menu item
			MainApp.BindToPlaybackMonitor( null );

			base.OnDestroy();
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

