using Android.Content;
using Android.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Android.Net.ConnectivityManager;

namespace DBTest
{
	/// <summary>
	/// The PlaybackCapabilities class monitors the wifi network and periodically scans for DLNA capabable devices
	/// </summary>
	public class PlaybackCapabilities : NetworkCallback
	{
		/// <summary>
		/// Register this class to receive ConnectivityManager callbacks
		/// </summary>
		/// <param name="appContext"></param>
		public PlaybackCapabilities( Context appContext )
		{
			// Provide the DNLA scanner with its callback
			scanner = new DLNAScanner( DeviceDiscovered );

			// Initialise but don't start the timer
			scanTimer = new Timer( timer => StartDLNAScanAsync(), null, Timeout.Infinite, Timeout.Infinite );

			// Register for network stae changes
			FromContext( appContext ).RegisterDefaultNetworkCallback( this );
		}

		/// <summary>
		/// Add the specified interface to the callback colletion
		/// </summary>
		/// <param name="callback"></param>
		public void RegisterCallback( IPlaybackCapabilitiesChanges callback )
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
		public void UnregisterCallback( IPlaybackCapabilitiesChanges callback )
		{
			callbacks.Remove( callback );
		}

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
					scanTimer.Change( 0, timerPeriod );
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
		private async void StartDLNAScanAsync()
		{
			// Turn off the timer whilst the scan is going
			scanTimer.Change( Timeout.Infinite, Timeout.Infinite );

			PlaybackDevices justScannedDevices = await scanner.DiscoverDevicesAsync( DeviceDiscovered );

			// Are there any devices in the collection held by this class that are no longer available
			PlaybackDevices missingDevices = new PlaybackDevices 
			{
				DeviceCollection = devices.DeviceCollection.Where( dev => ( justScannedDevices.FindDevice( dev ) == null ) ).ToList()
			};

			// Remove the missing devices from the collection
			missingDevices.DeviceCollection.ForEach( dev => devices.DeviceCollection.Remove( dev ) );

			// Report any missing devices
			if ( missingDevices.DeviceCollection.Count > 0 )
			{
				callbacks.ForEach( callback => callback.UnavailableDevices( missingDevices ) );
			}

			// Turn the timer back on
			scanTimer.Change( timerPeriod, timerPeriod );
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
		private Timer scanTimer = null;

		/// <summary>
		/// The PlaybackDevices discovered 
		/// </summary>
		private PlaybackDevices devices = new PlaybackDevices();

		/// <summary>
		/// Registered IPlaybackCapabilitiesChanges callbacks
		/// </summary>
		private List<IPlaybackCapabilitiesChanges> callbacks = new List<IPlaybackCapabilitiesChanges>();

		/// <summary>
		/// The DLNAScanner used to discover DNLA rendering devices
		/// </summary>
		private DLNAScanner scanner = null;

		/// <summary>
		/// The time between DLNA scans
		/// </summary>
		private const int timerPeriod = 30000;

		/// <summary>
		/// Interface used to report back PlaybackCapabilities changes
		/// </summary>
		public interface IPlaybackCapabilitiesChanges
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