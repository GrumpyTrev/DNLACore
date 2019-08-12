using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MobileApp
{
	/// <summary>
	/// The DeviceDiscovery class uses the SSDP protocol to get the IP addresses of any UPnP on the local network
	/// </summary>
	class DeviceDiscovery
	{
		public DeviceDiscovery()
		{
		}

		/// <summary>
		/// Send out a multicast discovery request and await replies.
		/// Parse the reply to determine the ipaddress and port of the discovered device
		/// </summary>
		public async void GoDiscover()
		{
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

//							Log.WriteLine( LogPriority.Debug, "MobileApp", "Received: " + message );

							// Extract the location of the server by extracting the IP address and port 
							Match locationMatch = Regex.Match( message, @"LOCATION: http:\/\/(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})\/(\S+)" );
							if ( locationMatch.Success == true )
							{
								Device newDevice = new Device() { IPAddress = locationMatch.Groups[ 1 ].Value, DescriptionURL = locationMatch.Groups[ 3 ].Value,
									Port = Int32.Parse( locationMatch.Groups[ 2 ].Value ) };

								DeviceDiscovered?.Invoke( this, new DeviceDiscoveredArgs() { DeviceDiscovered = newDevice } );
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

			DiscoveryDone?.Invoke( this, new EventArgs() ); 
		}

		/// <summary>
		/// The event used to indicate that a device has been discovered
		/// </summary>
		public event EventHandler< DeviceDiscoveredArgs > DeviceDiscovered;

		/// <summary>
		/// Identity of a discovered device
		/// </summary>
		public class DeviceDiscoveredArgs : EventArgs
		{
			public Device DeviceDiscovered { get; set; }
		}

		public event EventHandler DiscoveryDone;

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
