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
	/// The DLNAScanner class scans the network for DLNA devices capable of playing back media.
	/// It first of all sends out a multicast UDP discovery request to which UPnP devices respond.
	/// These UPnP devices are then queries to determine if they support the DLNA transport service.
	/// If during the scan a device is found that has never been seen before then it is queried for DLNA transport.
	/// If a device is found that has been previously seen then they will not be queries, the previous result of the query will be retained.
	/// This assumes that a device does not change its ability to support DLNA transport.
	/// If the scan fails to get a response from a previously available DLNA transport capable device then it is removed from the list of such devices.
	/// </summary>
	public class DLNAScanner
	{
		/// <summary>
		/// Public constructor
		/// </summary>
		/// <param name="callback"></param>
		public DLNAScanner( CapableDeviceDetectedDelegate callback )
		{
			// Save the callback used to report new DLNA capable devices
			deviceReporter = callback;
		}

		/// <summary>
		/// Send out a series of multicast DLNA search requests
		/// </summary>
		public async Task< PlaybackDevices > DiscoverDevicesAsync( CapableDeviceDetectedDelegate callback )
		{
			// Keep track of any devices scanned this time
			PlaybackDevices justScannedDevices = new PlaybackDevices();

			// Encode the discovery request
			Byte[] sendBytes = Encoding.UTF8.GetBytes( string.Format( SearchRequest, MulticastIP, MulticastPort, 3 ) );

			try
			{
				using ( UdpClient client = new UdpClient() )
				{
					// Send the discovery a few times in case its is missed
					for ( int loopCount = 1; loopCount < AttemptLimit; loopCount++ )
					{
						// Send the discovery request
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
								// Cancel the timer task
								token.Cancel();

								// Get the results from the receive task
								UdpReceiveResult result = await receiveTask;

								// Attempt to extract device details from the response 
								PlaybackDevice newDevice = ExtractDeviceFromResponse( Encoding.UTF8.GetString( result.Buffer, 0, result.Buffer.Length ) );

								if ( newDevice != null )
								{
									// Has this device been seen before
									PlaybackDevice existingDevice = scannedDevices.FindDevice( newDevice );
									if ( existingDevice != null )
									{
										justScannedDevices.AddDevice( existingDevice );

										// If this device supports the transport service then report it
										if ( existingDevice.CanPlayMedia == PlaybackDevice.CanPlayMediaType.Yes )
										{
											deviceReporter( existingDevice );
										}
									}
									else
									{
										// This is a new device, add it to the scanned devices and determine if it supports the transport service
										scannedDevices.AddDevice( newDevice );
										justScannedDevices.AddDevice( newDevice );

										GetTransportServiceAsync( newDevice );
									}
								}
							}
							else
							{
								// Get out of the loop on a timeout
								timedOut = true;
							}
						}
					}

					client.Close();
				}
			}
			catch ( SocketException exception )
			{
				Logger.Error( $"Error in device discovery {exception.Message}" );
			}

			return justScannedDevices;
		}

		/// <summary>
		/// Extract the IP address and port from a search response. If this is a newly discovered device then check it supports the transport service
		/// </summary>
		/// <param name="deviceResponse"></param>
		private PlaybackDevice ExtractDeviceFromResponse( string deviceResponse )
		{
			PlaybackDevice device = null;

			// Extract the location of the server by extracting the IP address and port 
			Match locationMatch = Regex.Match( deviceResponse, @"LOCATION: http:\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})\/(\S+)" );
			if ( locationMatch.Success == true )
			{
				device = new PlaybackDevice()
				{
					IPAddress = locationMatch.Groups[ 1 ].Value,
					DescriptionUrl = locationMatch.Groups[ 3 ].Value,
					Port = Int32.Parse( locationMatch.Groups[ 2 ].Value )
				};
			}

			return device;
		}

		/// <summary>
		/// Get the list of services supported by the device and check if one is the AVTransport service that indicates that the device
		/// suppports media playback
		/// </summary>
		/// <param name="targetDevice"></param>
		private async void GetTransportServiceAsync( PlaybackDevice targetDevice )
		{
			string request = DlnaRequestHelper.MakeRequest( "GET", targetDevice.DescriptionUrl, "", targetDevice.IPAddress, targetDevice.Port, "" );
			string response = await DlnaRequestHelper.SendRequest( targetDevice, request );

			// Get the response code from the response string
			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				// Look for the transport service and save its Url
				Match transportMatch = Regex.Match( response, @"AVTransport:1[\s\S]*?<controlURL>(.*)<\/controlURL>" );
				if ( transportMatch.Success == true )
				{
					targetDevice.CanPlayMedia = PlaybackDevice.CanPlayMediaType.Yes;
					targetDevice.PlayUrl = transportMatch.Groups[ 1 ].Value;

					// Remove leading '/' from the Url
					if ( targetDevice.PlayUrl[ 0 ] == '/' )
					{
						targetDevice.PlayUrl = targetDevice.PlayUrl.Substring( 1 );
					}

					// Get the device's friendly name for display purposes
					Match friendlyMatch = Regex.Match( response, @"<friendlyName>(.*)</friendlyName>" );
					targetDevice.FriendlyName = ( friendlyMatch.Success == true ) ? friendlyMatch.Groups[ 1 ].Value : targetDevice.PlayUrl;

					Logger.Log( $"Can Play Media IP {targetDevice.IPAddress}:{targetDevice.Port} Url {targetDevice.FriendlyName}" );

					deviceReporter( targetDevice );
				}
				else
				{
					targetDevice.CanPlayMedia = PlaybackDevice.CanPlayMediaType.No;
				}
			}
			else
			{
				targetDevice.CanPlayMedia = PlaybackDevice.CanPlayMediaType.No;
			}
		}

		/// <summary>
		/// Delegate to invoke when a playback capable device has been discovered
		/// </summary>
		/// <param name="device"></param>
		public delegate void CapableDeviceDetectedDelegate( PlaybackDevice device );

		/// <summary>
		/// Delegate type to invoke when a playback capable device has been discovered
		/// </summary>
		private CapableDeviceDetectedDelegate deviceReporter = null;

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

		/// <summary>
		/// Keep track of all devices discovered by this class
		/// </summary>
		private PlaybackDevices scannedDevices = new PlaybackDevices();
	}
}
