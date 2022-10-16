using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;

namespace DBTest
{
	[Activity( Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true )]
	public class MainActivity: AppCompatActivity
	{
		/// <summary>
		/// Called to create the UI components of the activity
		/// </summary>
		/// <param name="savedInstanceState"></param>
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Create the view hierarchy
			contentView = LayoutInflater.Inflate( Resource.Layout.activity_main, null );
			SetContentView( contentView );

			// Set the main top toolbar
			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar );
			SetSupportActionBar( toolbar );

			// Don't display the title as this is now done by the LibraryNameDisplay class (see below )
			SupportActionBar.SetDisplayShowTitleEnabled( false );

			// Allow controls to bind to items on the toolbar
			MainApp.BindView( contentView, this );

			// Pass on the fragment manager to the CommandRouter
			CommandRouter.Manager = SupportFragmentManager;

			// Initialise the fragments showing the selected library
			InitialiseFragments();

			// Make sure the app keeps going even though the system thinks it is using too much battery
			if ( Build.VERSION.SdkInt >= BuildVersionCodes.M )
			{
				// Make sure that this application is not subject to battery optimisations
				if ( ( ( PowerManager )GetSystemService( Context.PowerService ) ).IsIgnoringBatteryOptimizations( PackageName ) == false )
				{
					StartActivity( new Intent().SetAction( Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations )
						.SetData( Uri.Parse( "package:" + PackageName ) ) );
				}
			}

			// Make sure the app has been given the correct storage permission.
			if ( ContextCompat.CheckSelfPermission( this, Manifest.Permission.WriteExternalStorage ) != Permission.Granted )
			{
				// Request the permission
				ActivityCompat.RequestPermissions( this, new string[] { Manifest.Permission.WriteExternalStorage }, 1 );
			}
			else
			{
				// Tell the main app it can access storage
				MainApp.StoragePermissionGranted();
			}
		}

		/// <summary>
		/// Called in response to a request for a permission. Make sure it is the storage permission request, and if granted tell the main application
		/// </summary>
		/// <param name="requestCode"></param>
		/// <param name="permissions"></param>
		/// <param name="grantResults"></param>
		public override void OnRequestPermissionsResult( int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults )
		{
			if ( requestCode == 1 )
			{
				if ( ( grantResults.Length == 1 ) && ( grantResults[ 0 ] == Permission.Granted ) )
				{
					MainApp.StoragePermissionGranted();
				}	
			}
			else
			{
				base.OnRequestPermissionsResult( requestCode, permissions, grantResults );
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

			// Bind to any process wide controls using a menu item
			MainApp.BindMenu( menu, this, contentView );

			return true;
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

			// If the selection has not been handled pass it on to the base class
			if ( handled == false )
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Called when the activity is being closed down.
		/// This can either be temporary to respond to a configuration change (rotation),
		/// or permanent if the user or system is shutting down the application
		/// </summary>
		protected override void OnDestroy()
		{
			// Some of the managers need to remove themselves from the scene
			FragmentTitles.ParentActivity = null;

			// Unbind from any process wide command handler or monitors that require a menu item
			MainApp.BindMenu( null, null, null );
			MainApp.BindView( null, null );

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
		/// Make the main view for the activity accessible so that it can be passed to controls requireing to be bound to it
		/// </summary>
		private View contentView = null;
	}
}
