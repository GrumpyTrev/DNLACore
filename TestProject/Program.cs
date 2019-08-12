using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject
{
	class Program
	{
		static void Main( string[] args )
		{
			FindDevices();

			while ( true )
			{
			}
		}

		/// <summary>
		/// Device search request
		/// </summary>
		private const string searchRequest = "M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1}\r\nMAN: \"ssdp:discover\"\r\nMX: {2}\r\nST: {3}\r\n";

		/// <summary>
		/// Advertisement multicast address
		/// </summary>
		private const string multicastIP = "239.255.255.250";

		/// <summary>
		/// Advertisement multicast port
		/// </summary>
		private const int multicastPort = 1900;

		private static Socket socket;

		private static SocketAsyncEventArgs sendEvent;

		private static bool socketClosed;

		private static Timer timer;

		private static int searchTimeOut = 4;

		private static int sendCount;

		private const int MaxResultSize = 8096;

		public static void FindDevices()
		{
			string request = string.Format( searchRequest, multicastIP, multicastPort, searchTimeOut, "ssdp:all" );
			socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			byte[] multiCastData = Encoding.UTF8.GetBytes( request );
			socket.SendBufferSize = multiCastData.Length;
			socket.Bind( new IPEndPoint( IPAddress.Parse( "192.168.1.9" ), 0 ) );

			sendEvent = new SocketAsyncEventArgs();
			sendEvent.RemoteEndPoint = new IPEndPoint( IPAddress.Parse( multicastIP ), multicastPort );
			sendEvent.SetBuffer( multiCastData, 0, multiCastData.Length );
			sendEvent.Completed += OnSocketSendEventCompleted;

			// Set a one-shot timer for the Search time plus a second
			TimerCallback cb = new TimerCallback( ( state ) => {
				socketClosed = true;
				socket.Close();
			} );

			timer = new Timer( cb, null, TimeSpan.FromSeconds( searchTimeOut + 1 ), new TimeSpan( -1 ) );

			// Kick off the initial Send
			sendCount = 3;
			socketClosed = false;
			socket.SendToAsync( sendEvent );
			//while (!this.socketClosed)
			//{
			//    Thread.Sleep(200);
			//}

			//Task.WaitAll(this.taskList.ToArray());
			//this.taskList.Clear();

		}

		private static void OnSocketSendEventCompleted( object sender, SocketAsyncEventArgs e )
		{
			if ( e.SocketError != SocketError.Success )
			{
				AddDevice( null );
			}
			else
			{
				if ( e.LastOperation == SocketAsyncOperation.SendTo )
				{
					if ( --sendCount != 0 )
					{
						if ( !socketClosed )
						{
							socket.SendToAsync( sendEvent );
						}
					}
					else
					{
						// When the initial multicast is done, get ready to receive responses
						e.RemoteEndPoint = new IPEndPoint( IPAddress.Any, 0 );
						socket.ReceiveBufferSize = MaxResultSize;
						byte[] receiveBuffer = new byte[ MaxResultSize ];
						e.SetBuffer( receiveBuffer, 0, MaxResultSize );
						socket.ReceiveFromAsync( e );
					}
				}
				else if ( e.LastOperation == SocketAsyncOperation.ReceiveFrom )
				{
					// Got a response, so decode it
					string result = Encoding.UTF8.GetString( e.Buffer, 0, e.BytesTransferred );
					if ( result.StartsWith( "HTTP/1.1 200 OK", StringComparison.InvariantCultureIgnoreCase ) )
					{
						//parse device and invoke callback
						AddDevice( result );
					}
					else
					{
						//Debug.WriteLine("INVALID SEARCH RESPONSE");
					}

					if ( !socketClosed )
					{
						// and kick off another read
						socket.ReceiveFromAsync( e );
					}
					else
					{
						// unless socket was closed, when declare the scan is complete
						//AddDevice(result);
					}
				}
			}
		}

		private static void AddDevice( string response )
		{
			Console.WriteLine( response );
			//Task addDeviceTask = Task.Run(() =>
			//{
			//    // parse the result and download the device description
			//    if (this.onDeviceFound != null && response != null)
			//    {
			//        Dictionary<string, string> ssdpResponse = ParseSSDPResponse(response);
			//        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(ssdpResponse["location"]);
			//        WebResponse webResponse = webRequest.GetResponse();
			//        using (DeviceXml deviceXml = new DeviceXml(webResponse.GetResponseStream()))
			//        {
			//            this.onDeviceFound(deviceXml.GetObject());
			//        }
			//    }
			//});

			//this.taskList.Add(addDeviceTask);
		}

	}
}
