using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	[Activity( Label = "PlayListActivity", Theme = "@style/AppTheme.NoActionBar" )]
	public class PlayListActivity: AppCompatActivity
	{
		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			SetContentView( Resource.Layout.activity_playlist );

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar );
			SetSupportActionBar( toolbar );

			SupportActionBar.SetDisplayHomeAsUpEnabled( true );
			SupportActionBar.SetHomeButtonEnabled( true );

		}

		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			if ( item.ItemId == Android.Resource.Id.Home )
			{
				Finish();
			}

			return base.OnOptionsItemSelected( item );
		}
	}
}