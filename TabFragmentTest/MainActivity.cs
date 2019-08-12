using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace TabFragmentTest
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
		TabLayout tabLayout;

		protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

			tabLayout = FindViewById<TabLayout>( Resource.Id.sliding_tabs );

			InitialiseFragments();
		}

		private void InitialiseFragments()
		{
			Android.Support.V4.App.Fragment[] fragments = new Android.Support.V4.App.Fragment[]
			{
				new BlueFragment(),
				new GreenFragment(),
				new YellowFragment(),
			};

			//Tab title array
			Java.Lang.ICharSequence[] titles = CharSequence.ArrayFromStringArray( new[] { "Library", "Playlists", "Playing" } );

			ViewPager viewPager = FindViewById<ViewPager>( Resource.Id.viewpager );

			//viewpager holding fragment array and tab title text
			viewPager.Adapter = new TabsFragmentPagerAdapter( SupportFragmentManager, fragments, titles );

			// Give the TabLayout the ViewPager 
			tabLayout.SetupWithViewPager( viewPager );
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

	}
}

