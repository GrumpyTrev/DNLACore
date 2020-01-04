using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaybackSelectionController is the Controller for the RemotePlayback. It responds to RemotePlayback commands and maintains 
	/// remoteplayback data in the PlaybackSelectionModel
	/// </summary>
	public static class PlaybackSelectionController
	{
		/// <summary>
		/// Load up the discovered devices with the local device
		/// </summary>
		static PlaybackSelectionController()
		{
			InitialiseDiscoveredDevices();
		}

		/// <summary>
		/// Send out a multicast discovery request and await replies.
		/// Parse the reply to determine the ipaddress and port of the discovered device
		/// </summary>
		public static async void DiscoverDevicesAsync()
		{
			// Only start discovering device if no remote devices have already been discovered (remember the collection always include the local device)
			if ( PlaybackSelectionModel.RemoteDevices.DeviceCollection.Count == 1 )
			{
				// Before discovering remote devices get the last selected device from the database and if it was the
				// local device then report that device as available for playback
				ReportLocalSelectedDevice();

				// Send the discovery a few times in case its is missed
				for ( int loopCount = 1; loopCount < AttemptLimit; loopCount++ )
				{
					using ( UdpClient client = new UdpClient() )
					{
						// Send a discovery request
						Byte[] sendBytes = Encoding.UTF8.GetBytes( string.Format( SearchRequest, MulticastIP, MulticastPort, 3 ) );
						await client.SendAsync( sendBytes, sendBytes.Length, new IPEndPoint( IPAddress.Parse( MulticastIP ), MulticastPort ) );

						// Loop receiving replies until the reply times out
						bool timedOut = false;
						while ( timedOut == false )
						{
							// Need a token to cancel the timer if a reply is received
							CancellationTokenSource token = new CancellationTokenSource();

							// Use delay and receive tasks
							Task waitTask = Task.Delay( 2000, token.Token );
							Task<UdpReceiveResult> receiveTask = client.ReceiveAsync();

							// Wait for one of the tasks to finish
							Task finishedTask = await Task.WhenAny( receiveTask, waitTask );

							// If data has been received then process the data
							if ( finishedTask == receiveTask )
							{
								UdpReceiveResult result = await receiveTask;
								string message = Encoding.UTF8.GetString( result.Buffer, 0, result.Buffer.Length );

								// Extract the location of the server by extracting the IP address and port 
								Match locationMatch = Regex.Match( message, @"LOCATION: http:\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})\/(\S+)" );
								if ( locationMatch.Success == true )
								{
									Device newDevice = new Device() {
										IPAddress = locationMatch.Groups[ 1 ].Value, DescriptionURL = locationMatch.Groups[ 3 ].Value,
										Port = Int32.Parse( locationMatch.Groups[ 2 ].Value )
									};

									// Add this device to the candidate devices
									if ( CandidateDevices.AddDevice( newDevice ) == true )
									{
										Logger.Log( string.Format( "Discovered IP {0}:{1} Url {2}", newDevice.IPAddress, newDevice.Port,
											newDevice.DescriptionURL ) );

										GetTransportService( newDevice );
									}
								}

								// Cancel the timer
								token.Cancel();
							}
							else
							{
								// Get out of the loop and close the socket
								timedOut = true;
								client.Close();
							}
						}
					}
				}
			}

			Reporter?.DiscoveryFinished();
		}

		/// <summary>
		/// Clear the device list and scan again
		/// </summary>
		public static void ReDiscoverDevices()
		{
			InitialiseDiscoveredDevices();
			DiscoverDevicesAsync();
		}

		/// <summary>
		/// Called when the user has selected a new playback device
		/// Save it in the model and report it
		/// </summary>
		/// <param name="deviceName"></param>
		public static async void SetSelectedPlaybackAsync( string deviceName )
		{
			Device selectedDevice = PlaybackSelectionModel.RemoteDevices.FindDevice( deviceName );
			if ( selectedDevice != null )
			{
				PlaybackSelectionModel.SelectedDevice = selectedDevice;
				PlaybackSelectionModel.SelectedDeviceName = selectedDevice.FriendlyName;
				await PlaybackAccess.SetPlaybackDeviceAsync( PlaybackSelectionModel.SelectedDeviceName );

				new PlaybackDeviceAvailableMessage() { SelectedDevice = PlaybackSelectionModel.SelectedDevice }.Send();
			}
		}

		/// <summary>
		/// This method is called when a rescan has been requested by the user
		/// Pass this back to the selection manager
		/// </summary>
		public static void RescanRequested()
		{
			Reporter?.RescanRequested();
		}

		/// <summary>
		/// Get the selected device from the database and if its the local device report is as available
		/// </summary>
		private static async void ReportLocalSelectedDevice()
		{
			Device localDevice = PlaybackSelectionModel.RemoteDevices.DeviceCollection[ 0 ];

			// Use the PlaybackAccess class to retrieve the last selected device
			PlaybackSelectionModel.SelectedDeviceName = await PlaybackAccess.GetPlaybackDeviceAsync();

			if ( PlaybackSelectionModel.SelectedDeviceName.Length == 0 )
			{
				// No device selected. Select the local device
				PlaybackSelectionModel.SelectedDeviceName = localDevice.FriendlyName;
				await PlaybackAccess.SetPlaybackDeviceAsync( PlaybackSelectionModel.SelectedDeviceName );
			}

			// If the selected device is the local device then report it as available
			if ( PlaybackSelectionModel.SelectedDeviceName == localDevice.FriendlyName )
			{
				PlaybackSelectionModel.SelectedDevice = localDevice;
				new PlaybackDeviceAvailableMessage() { SelectedDevice = localDevice }.Send();
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void PlaybackSelectionDataAvailable();
			void DiscoveryFinished();
			void RescanRequested();
		}

		/// <summary>
		/// Get the list of services supported by the device and check if one is the AVTransport service that indicates that the device
		/// suppports media playback
		/// </summary>
		/// <param name="targetDevice"></param>
		private static async void GetTransportService( Device targetDevice )
		{
			string request = DlnaRequestHelper.MakeRequest( "GET", targetDevice.DescriptionURL, "", targetDevice.IPAddress, targetDevice.Port, "" );
			string response = await DlnaRequestHelper.SendRequest( targetDevice, request );

			Logger.Log( request );
			Logger.Log( response );

			// Get the response code from the response string
			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				// Look for the transport service and save its Url
				Match transportMatch = Regex.Match( response, @"AVTransport:1[\s\S]*?<controlURL>(.*)<\/controlURL>" );
				if ( transportMatch.Success == true )
				{
					targetDevice.CanPlayMedia = true;
					targetDevice.PlayUrl = transportMatch.Groups[ 1 ].Value;

					// Remove leading '/' from the Url
					if ( targetDevice.PlayUrl[ 0 ] == '/' )
					{
						targetDevice.PlayUrl = targetDevice.PlayUrl.Substring( 1 );
					}

					Logger.Log( string.Format( "Can Play Media IP {0}:{1} Url {2}", targetDevice.IPAddress, targetDevice.Port, 
						targetDevice.DescriptionURL ) );

					// Get the device's friendly name for display purposes
					Match friendlyMatch = Regex.Match( response, @"<friendlyName>(.*)</friendlyName>" );
					if ( friendlyMatch.Success == true )
					{
						targetDevice.FriendlyName = friendlyMatch.Groups[ 1 ].Value;
					}
					else
					{
						targetDevice.FriendlyName = targetDevice.PlayUrl;
					}

					// Add this device to the model and inform the reporter
					PlaybackSelectionModel.RemoteDevices.AddDevice( targetDevice );
					Reporter?.PlaybackSelectionDataAvailable();

					// If this device is the currently selected device then report it as available
					if ( targetDevice.FriendlyName == PlaybackSelectionModel.SelectedDeviceName )
					{
						PlaybackSelectionModel.SelectedDevice = targetDevice;
						new PlaybackDeviceAvailableMessage() { SelectedDevice = targetDevice }.Send();
					}
				}
			}
		}

		/// <summary>
		/// Clear the Remote devices list and add the always present internal device
		/// </summary>
		private static void InitialiseDiscoveredDevices()
		{
			CandidateDevices.DeviceCollection.Clear();
			PlaybackSelectionModel.RemoteDevices.DeviceCollection.Clear();
			PlaybackSelectionModel.RemoteDevices.AddDevice( new Device() { CanPlayMedia = true, IsLocal = true, FriendlyName = "Local playback" } );
		}

		/// <summary>
		/// The collection of devices that have been discovered but for which playback has not yet been verified
		/// </summary>
		private static Devices CandidateDevices { get; } = new Devices();

		/// <summary>
		/// The number of times to send the discovery telegram
		/// </summary>
		private const int AttemptLimit = 2;

		/// <summary>
		/// Advertisement multicast address
		/// </summary>
		private const string MulticastIP = "239.255.255.250";

		/// <summary>
		/// Advertisement multicast port
		/// </summary>
		private const int MulticastPort = 1900;

		/// <summary>
		/// Device search request
		/// </summary>
		private const string SearchRequest = "M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1}\r\nMAN: \"ssdp:discover\"\r\nMX: {2}\r\nST: ssdp:all\r\n\r\n";

		/// <summary>
		/// Prefix for IP address in reply
		/// </summary>
		private const string locationId = "LOCATION: ";
	}
}