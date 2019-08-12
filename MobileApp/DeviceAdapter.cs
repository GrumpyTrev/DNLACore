using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace MobileApp
{
	/// <summary>
	/// A custom adapter for the dropdown list of available DLNA devices
	/// </summary>
	class DeviceAdapter: ArrayAdapter< string >
	{
		/// <summary>
		/// Constructor. Save the view inflator and parent spinner control for later
		/// </summary>
		/// <param name="context"></param>
		/// <param name="layoutId"></param>
		/// <param name="deviceStrings"></param>
		public DeviceAdapter( Activity context, int layoutId, List<string> deviceStrings ) : base( context, layoutId, deviceStrings )
		{
			inflater = context.LayoutInflater;
		}

		/// <summary>
		/// Set the text for the specified item
		/// </summary>
		/// <param name="position"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			View view = convertView;

			if ( view == null )
			{
				view = inflater.Inflate( Resource.Layout.spinner_item, null );
			}

			view.FindViewById<TextView>( Resource.Id.deviceName ).Text = GetItem( position );

			return view;
		}

		/// <summary>
		/// Load the spinner with a new set of trips
		/// </summary>
		/// <param name="trips"></param>
		public void ReloadSpinner( List< string > devices )
		{
			Clear();
			AddAll( devices );
		}

		private LayoutInflater inflater = null;
	}
}