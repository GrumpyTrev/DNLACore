using System.Collections.Generic;
using System.Threading;

namespace CoreMP
{
	/// <summary>
	/// The DeviceDiscovery class monitors the wifi network and periodically scans for DLNA capabable devices
	/// </summary>
	public class DeviceDiscovery
	{
		/// <summary>
		/// Initialise the DLNAScanner and associated timer
		/// </summary>
		/// <param name="appContext"></param>
		public DeviceDiscovery()
		{
			// Provide the DNLA scanner with the device collection to add devices to
			scanner = new DLNAScanner( DevicesModel.RemoteDevices );

			// Initialise but don't start the timer
			scanTimer = new Timer( timer => StartDLNAScan(), null, Timeout.Infinite, Timeout.Infinite );
		}

		/// <summary>
		/// Called when the state of the WiFi has been reported as changed
		/// Update the stored state of the wifi network.
		/// If the network has just been reported as available then scan for DLNA capable devices
		/// If the network is not available then stop the periodic DLNA scans
		/// </summary>
		/// <param name="network"></param>
		/// <param name="linkProperties"></param>
		public void OnWiFiStateChanged( bool newWifiState )
		{
			if ( newWifiState != DevicesModel.WifiAvailable )
			{
				DevicesModel.WifiAvailable = newWifiState;

				if ( DevicesModel.WifiAvailable == true )
				{
					scanTimer.Change( 0, TimerPeriod );
				}
				else
				{
					// Turn off the timer
					scanTimer.Change( Timeout.Infinite, Timeout.Infinite );

					// Clear all the available devices
					DevicesModel.RemoteDevices.Clear();
				}
			}
		}

		/// <summary>
		/// Start the DLNA scanning process.
		/// Any devices found are reported back to the DeviceDiscovered delegate.
		/// At the end check if any previously found devicies are no longer available
		/// </summary>
		private void StartDLNAScan()
		{
			// Check if any existing devices have missed a number of consecutive scans.
			// This works on the results of the previous scan
			List<PlaybackDevice> missingDevices = DevicesModel.RemoteDevices.MissingDevices();

			// Remove the missing devices from the collection
			missingDevices.ForEach( dev => DevicesModel.RemoteDevices.RemoveDevice( dev ) );

			scanner.DiscoverDevices();
		}

		/// <summary>
		/// Timer used to schedulre DLNA scans
		/// </summary>
		private readonly Timer scanTimer = null;

		/// <summary>
		/// The DLNAScanner used to discover DNLA rendering devices
		/// </summary>
		private readonly DLNAScanner scanner = null;

		/// <summary>
		/// The time between DLNA scans
		/// </summary>
		private const int TimerPeriod = 30000;
	}
}
