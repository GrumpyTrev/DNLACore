using Android.Content;
using Android.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Android.Net.ConnectivityManager;

namespace DBTest
{
	/// <summary>
	/// The DeviceDiscovery class monitors the wifi network and periodically scans for DLNA capabable devices
	/// </summary>
	public class DeviceDiscovery : NetworkCallback
	{
		/// <summary>
		/// Register this class to receive ConnectivityManager callbacks
		/// </summary>
		/// <param name="appContext"></param>
		public DeviceDiscovery( Context appContext )
		{
			// Provide the DNLA scanner with its callback
			scanner = new DLNAScanner( DeviceDiscovered );

			// Initialise but don't start the timer
			scanTimer = new Timer( timer => StartDLNAScan(), null, Timeout.Infinite, Timeout.Infinite );

			// Register for network stae changes
			FromContext( appContext ).RegisterDefaultNetworkCallback( this );
		}

		/// <summary>
		/// Add the specified interface to the callback colletion
		/// </summary>
		/// <param name="callback"></param>
		public void RegisterCallback( IDeviceDiscoveryChanges callback )
		{
			if ( callbacks.Contains( callback ) == false )
			{
				callbacks.Add( callback );
				callback.AvailableDevices( devices );
				callback.NetworkState( wifiAvailable );
			}
		}

		/// <summary>
		/// Remove the specified inteferace from the callback collection
		/// </summary>
		/// <param name="callback"></param>
		public void UnregisterCallback( IDeviceDiscoveryChanges callback ) => callbacks.Remove( callback );

		/// <summary>
		/// Called when the ConnectivityManager detects a change to the default network
		/// Update the stored state of the wifi network.
		/// If the network has just been reported as available then scan for DLNA capable devices
		/// If the network is not available then stop the periodic DLNA scans
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

				if ( wifiAvailable == true )
				{
					scanTimer.Change( 0, TimerPeriod );
				}
				else
				{
					// Turn off the timer
					scanTimer.Change( Timeout.Infinite, Timeout.Infinite );

					// Clear all the available devices
					callbacks.ForEach( callback => callback.UnavailableDevices( devices ) );
					devices.DeviceCollection.Clear();
				}

				// Report the new network state
				callbacks.ForEach( callback => callback.NetworkState( wifiAvailable ) );
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
			PlaybackDevices missingDevices = new()
			{
				DeviceCollection = devices.DeviceCollection.Where( dev => ( ++dev.CommunicationFailureCount > PlaybackDevice.CommunicationFailureLimit ) ).ToList()
			};

			// Remove the missing devices from the collection
			missingDevices.DeviceCollection.ForEach( dev => devices.DeviceCollection.Remove( dev ) );

			// Report any missing devices
			if ( missingDevices.DeviceCollection.Count > 0 )
			{
				callbacks.ForEach( callback => callback.UnavailableDevices( missingDevices ) );
			}

			scanner.DiscoverDevices();
		}

		/// <summary>
		/// Called when a playback device has been discovered.
		/// If this device has not already been found then report it to all registered interfaces
		/// </summary>
		/// <param name="device"></param>
		private void DeviceDiscovered( PlaybackDevice device )
		{
			if ( devices.AddDevice( device ) == true )
			{
				// Report this new device
				callbacks.ForEach( callback => callback.NewDeviceDetected( device ) );
			}
		}

		/// <summary>
		/// Is the wifi network available
		/// </summary>
		private bool wifiAvailable = false;

		/// <summary>
		/// Timer used to schedulre DLNA scans
		/// </summary>
		private readonly Timer scanTimer = null;

		/// <summary>
		/// The PlaybackDevices discovered 
		/// </summary>
		private readonly PlaybackDevices devices = new();

		/// <summary>
		/// Registered IDeviceDiscoveryChanges callbacks
		/// </summary>
		private readonly List<IDeviceDiscoveryChanges> callbacks = new();

		/// <summary>
		/// The DLNAScanner used to discover DNLA rendering devices
		/// </summary>
		private readonly DLNAScanner scanner = null;

		/// <summary>
		/// The time between DLNA scans
		/// </summary>
		private const int TimerPeriod = 30000;

		/// <summary>
		/// Interface used to report back DeviceDiscovery changes
		/// </summary>
		public interface IDeviceDiscoveryChanges
		{
			/// <summary>
			/// Called when the wifi network state changes
			/// </summary>
			/// <param name="state"></param>
			void NetworkState( bool state );

			/// <summary>
			/// Called when a new DLNA device has been detected
			/// </summary>
			/// <param name="device"></param>
			void NewDeviceDetected( PlaybackDevice device );

			/// <summary>
			/// Called to report the available devices - when registration is first made
			/// </summary>
			/// <param name="devices"></param>
			void AvailableDevices( PlaybackDevices devices );

			/// <summary>
			/// Called when one or more devices are no longer available
			/// </summary>
			/// <param name="devices"></param>
			void UnavailableDevices( PlaybackDevices devices );
		}
	}
}
