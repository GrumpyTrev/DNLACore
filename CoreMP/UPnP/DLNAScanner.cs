using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreMP
{
	/// <summary>
	/// The DLNAScanner class scans the network for DLNA devices capable of playing back media.
	/// It first of all sends out a multicast UDP discovery request to which UPnP devices respond.
	/// These UPnP devices are then queries to determine if they support the DLNA transport and content services.
	/// If during the scan a device is found that has never been seen before then it is queried for these services.
	/// If a device is found that has been previously seen then they will not be queries, the previous result of the query will be retained.
	/// This assumes that a device does not change its ability to support DLNA services.
	/// </summary>
	internal class DLNAScanner
	{
		/// <summary>
		/// Public constructor
		/// Save the PlaybackDevices used to report new DLNA capable devices
		/// </summary>
		/// <param name="devices"></param>
		public DLNAScanner( PlaybackDevices devices ) => activeDevices = devices;

		/// <summary>
		/// Send out a series of multicast DLNA search requests
		/// </summary>
		public void DiscoverDevices()
		{
			try
			{
				// Has a discovery client already been initialised
				if ( discoveryClient == null )
				{
					discoveryClient = new UdpClient();
					discoveryClient.BeginReceive( new AsyncCallback( OnDiscoveryResponse ), discoveryClient );
				}

				// Send the discovery request (twice)
				discoveryClient.Send( discoveryBytes, discoveryBytes.Length, discoveryEndPoint );
				discoveryClient.Send( discoveryBytes, discoveryBytes.Length, discoveryEndPoint );
			}
			catch ( SocketException exception )
			{
				Logger.Error( $"Error in DiscoverDevices {exception.Message}" );
			}
		}

		/// <summary>
		/// Called when a discovery response has been received.
		/// If this is a new device then determine which services it supports
		/// </summary>
		/// <param name="ar"></param>
		private void OnDiscoveryResponse( IAsyncResult ar )
		{
			UdpClient receiveClient = ( UdpClient )ar.AsyncState;

			try
			{
				IPEndPoint ep = null;
				byte[] buffer = receiveClient.EndReceive( ar, ref ep );

				if ( buffer != null )
				{
					// Attempt to extract device details from the response 
					PlaybackDevice newDevice = ExtractDeviceFromResponse( Encoding.UTF8.GetString( buffer, 0, buffer.Length ) );

					if ( newDevice != null )
					{
						// Has this device been seen before
						PlaybackDevice existingDevice = scannedDevices.SingleOrDefault( dev => newDevice.Equals( dev ) );
						if ( existingDevice != null )
						{
							existingDevice.CommunicationFailureCount = 0;

							// If this device supports the transport service then report it
							if ( existingDevice.CanPlayMedia == PlaybackDevice.CanPlayMediaType.Yes )
							{
								activeDevices.AddDevice( existingDevice );
							}
						}
						else
						{
							// This is a new device, add it to the scanned devices and determine if it supports the transport service
							scannedDevices.Add( newDevice );
							newDevice.CommunicationFailureCount = 0;

							GetTransportServiceAsync( newDevice );
						}
					}

					receiveClient.BeginReceive( new AsyncCallback( OnDiscoveryResponse ), receiveClient );
				}
				else
				{
					Logger.Error( $"Error in OnDiscoveryResponse - no data" );
					discoveryClient = null;
				}
			}
			catch ( SocketException exception )
			{
				Logger.Error( $"Error in OnDiscoveryResponse {exception.Message}" );
				discoveryClient = null;
			}
		}

		/// <summary>
		/// Extract the IP address and port from a search response.
		/// </summary>
		/// <param name="deviceResponse"></param>
		private PlaybackDevice ExtractDeviceFromResponse( string deviceResponse )
		{
			PlaybackDevice device = null;

			// Extract the location of the device by extracting the IP address, port and device URL  
			Match locationMatch = Regex.Match( deviceResponse, @"LOCATION: http:\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})\/(\S+)" );
			if ( locationMatch.Success == true )
			{
				device = new PlaybackDevice()
				{
					IPAddress = locationMatch.Groups[ 1 ].Value,
					Port = Int32.Parse( locationMatch.Groups[ 2 ].Value ),
					DescriptionUrl = locationMatch.Groups[ 3 ].Value
				};
			}

			return device;
		}

		/// <summary>
		/// Get the list of services supported by the device and check if one is the AVTransport service that indicates that the device
		/// supports media playback
		/// </summary>
		/// <param name="targetDevice"></param>
		private async void GetTransportServiceAsync( PlaybackDevice targetDevice )
		{
			string request = DlnaRequestHelper.MakeRequest( "GET", targetDevice.DescriptionUrl, "", targetDevice.IPAddress, targetDevice.Port, "" );
			string response = await DlnaRequestHelper.SendRequest( targetDevice, request );

			// Assume this device cannot play back media
			targetDevice.CanPlayMedia = PlaybackDevice.CanPlayMediaType.No;

			// Get the response code from the response string
			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				Logger.Log( $"Response from {targetDevice.DescriptionUrl}:{response}" );

				// Get the friendly name for the device
				Match friendlyMatch = Regex.Match( response, @"<friendlyName>(.*)</friendlyName>" );
				targetDevice.FriendlyName = ( friendlyMatch.Success == true ) ? friendlyMatch.Groups[ 1 ].Value : "No name";

				// Try and get all the services from the response
				bool match = true;
				string searchString = response;
				while ( match == true )
				{
					Match serviceMatch = Regex.Match( searchString, @"serviceId:([^<]*)[\s\S]*?<controlURL>([^<]*)([\s\S]*)" );
					if ( serviceMatch.Success == true )
					{
						string serviceName = serviceMatch.Groups[ 1 ].Value;
						string serviceControl = serviceMatch.Groups[ 2 ].Value;
						if ( serviceControl[ 0 ] == '/' )
						{
							serviceControl = serviceControl[ 1.. ];
						}

						searchString = serviceMatch.Groups[ 3 ].Value;

						Logger.Log( $"Service {serviceName} Control URL {serviceControl}" );

						if ( serviceName == "AVTransport" )
						{
							targetDevice.CanPlayMedia = PlaybackDevice.CanPlayMediaType.Yes;
							targetDevice.PlayUrl = serviceControl;

							Logger.Log( $"Can Play Media IP {targetDevice.IPAddress}:{targetDevice.Port} Url {targetDevice.FriendlyName}" );
						}
						else if ( serviceName == "ContentDirectory" )
						{
							targetDevice.ContentUrl = serviceControl;

							Logger.Log( $"Can provide content {targetDevice.IPAddress}:{targetDevice.Port} Url {targetDevice.FriendlyName}" );
						}
					}

					match = serviceMatch.Success;
				}

				// Only report this device if it can playback or supports the content service
				if ( ( targetDevice.CanPlayMedia == PlaybackDevice.CanPlayMediaType.Yes ) || ( targetDevice.ContentUrl.Length > 0 ) )
				{
					activeDevices.AddDevice( targetDevice );
				}
			}
		}

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
		/// The discovery message (as bytes)
		/// </summary>
		private readonly Byte[] discoveryBytes = Encoding.UTF8.GetBytes( string.Format( SearchRequest, MulticastIP, MulticastPort, 3 ) );

		/// <summary>
		/// Keep track of all devices discovered by this class. Devices are never removed from this list even if they are no longer
		/// available. The PlaybackDevices instance is used to keep track of devices that can be reached.
		/// </summary>
		private readonly List<PlaybackDevice> scannedDevices = new List<PlaybackDevice>();

		/// <summary>
		/// The devices that have been found by the DLNA scan. This collection is actively maintained by the DeviceDiscovery class
		/// </summary>
		private readonly PlaybackDevices activeDevices = null;

		/// <summary>
		/// The UdpClient used to receive SSDP discovery responses
		/// </summary>
		private UdpClient discoveryClient = null;

		/// <summary>
		/// The end point to send discovery requests to
		/// </summary>
		private readonly IPEndPoint discoveryEndPoint = new IPEndPoint( IPAddress.Parse( MulticastIP ), MulticastPort );
	}
}
