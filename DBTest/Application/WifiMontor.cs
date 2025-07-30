using System;
using Android.Content;
using Android.Net;
using static Android.Net.ConnectivityManager;

namespace DBTest
{
	/// <summary>
	/// The WifiMontor class monitors the wifi network and reports its availability
	/// </summary>
	public class WifiMontor : NetworkCallback
	{
		/// <summary>
		/// Register this class to receive ConnectivityManager callbacks
		/// </summary>
		/// <param name="appContext"></param>
		public WifiMontor( Context appContext, Action<bool> callback )
		{
			// Register for network state changes
			FromContext( appContext ).RegisterDefaultNetworkCallback( this );
			wifiStateChangeAction = callback;
		}

		/// <summary>
		/// Called when the ConnectivityManager detects a change to the default network
		/// Update the stored state of the wifi network and report the change
		/// </summary>
		/// <param name="network"></param>
		/// <param name="linkProperties"></param>
		public override void OnLinkPropertiesChanged( Network network, LinkProperties linkProperties )
		{
			base.OnLinkPropertiesChanged( network, linkProperties );

			bool newWifiState = ( linkProperties.InterfaceName == "wlan0" );
			if ( newWifiState != wifiAvailable )
			{
				wifiAvailable = newWifiState;

				// Report the new network state
				wifiStateChangeAction.Invoke( wifiAvailable );
			}
		}

		/// <summary>
		/// Is the wifi network available
		/// </summary>
		private bool wifiAvailable = false;

		/// <summary>
		/// The action to perform when a change of wifi state is detected
		/// </summary>
		private readonly Action<bool> wifiStateChangeAction = null;
	}
}
