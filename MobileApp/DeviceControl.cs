using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Environment = System.Environment;

namespace MobileApp
{
	class DeviceControl
	{
		class QueuedRequest
		{
			public Device Target { get; set; }
			public string Request { get; set; }
			public string Response { get; set; }
			public async Task DoRequest()
			{
				Response = await DeviceControl.SendRequest( Target, Request );
			}
		}

		readonly static object _locker = new object();
		static WeakReference<Task> _lastTask;

		public static Task Enqueue( Action action )
		{
			return Enqueue<bool>( () => {
				action();
				return true;
			} );
		}

		public static Task<T> Enqueue<T>( Func<T> function )
		{
			lock ( _locker )
			{
				Task lastTask = null;
				Task<T> resultTask = null;

				if ( _lastTask != null && _lastTask.TryGetTarget( out lastTask ) )
				{
					resultTask = lastTask.ContinueWith( _ => function(), TaskContinuationOptions.ExecuteSynchronously );
				}
				else
				{
					resultTask = Task.Run( function );
				}

				_lastTask = new WeakReference<Task>( resultTask );
				return resultTask;
			}
		}

		public static Task Enqueue( Func<Task> asyncAction )
		{
			lock ( _locker )
			{
				Task lastTask = null;
				Task resultTask = null;

				if ( _lastTask != null && _lastTask.TryGetTarget( out lastTask ) )
				{
					resultTask = lastTask.ContinueWith( _ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously ).Unwrap();
				}
				else
				{
					resultTask = Task.Run( asyncAction );
				}

				_lastTask = new WeakReference<Task>( resultTask );
				return resultTask;
			}
		}

		public static Task<T> Enqueue<T>( Func<Task<T>> asyncFunction )
		{
			lock ( _locker )
			{
				Task lastTask = null;
				Task<T> resultTask = null;

				if ( _lastTask != null && _lastTask.TryGetTarget( out lastTask ) )
				{
					resultTask = lastTask.ContinueWith( _ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously ).Unwrap();
				}
				else
				{
					resultTask = Task.Run( asyncFunction );
				}

				_lastTask = new WeakReference<Task>( resultTask );
				return resultTask;
			}
		}

		/// <summary>
		/// Get the list of services supported by the device and check if one is the AVTransport service that indicates that the device
		/// suppports media playback
		/// </summary>
		/// <param name="targetDevice"></param>
		public async void GetTransportService( Device targetDevice )
		{
			string request = MakeRequest( "GET", targetDevice.DescriptionURL, 0, "", targetDevice.IPAddress, targetDevice.Port );
			string response = await SendRequest( targetDevice, request );

//			QueuedRequest qRequest = new QueuedRequest() { Request = request, Target = targetDevice };

//			await Enqueue( ( Func< Task > )qRequest.DoRequest );

//			string response = qRequest.Response;

			Log.WriteLine( LogPriority.Debug, "MobileApp", request );
			Log.WriteLine( LogPriority.Debug, "MobileApp", response );

			// Get the response code from the response string
			if ( GetResponseCode( response ) == 200 )
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

					Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Can Play Media IP {0}:{1} Url {2}", targetDevice.IPAddress,
						targetDevice.Port, targetDevice.DescriptionURL ) );

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

					if ( DeviceSupportsPlayback != null )
					{
						DeviceSupportsPlayback.Invoke( this, new DevicePlaybackArgs() { PlaybackDevice = targetDevice } );
					}
				}
			}
		}

		/// <summary>
		/// The event used to indicate that a device can support playback
		/// </summary>
		public event EventHandler< DevicePlaybackArgs > DeviceSupportsPlayback;

		/// <summary>
		/// Identity of a device that can playback media
		/// </summary>
		public class DevicePlaybackArgs: EventArgs
		{
			public Device PlaybackDevice { get; set; }
		}

		public async void Play( Device playDevice, string urlToPlay )
		{
			string xmlString = string.Format(
				"<?xml version=\"1.0\"?>\r\n" +
				"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
				"SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
				"<SOAP-ENV:Body>\r\n" +
				"<u:SetAVTransportURI xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">\r\n" +
				"<InstanceID>0</InstanceID>\r\n" +
				"<CurrentURI>{0}</CurrentURI>\r\n" +
				"<CurrentURIMetaData>{1}</CurrentURIMetaData>\r\n" +
				"</u:SetAVTransportURI>\r\n" +
				"</SOAP-ENV:Body>\r\n" +
				"</SOAP-ENV:Envelope>\r\n",
				urlToPlay.Replace( " ", "%20" ), ServerDescription );

			string request = MakeRequest( "POST", playDevice.PlayUrl, xmlString.Length, "urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI",
				playDevice.IPAddress, playDevice.Port ) + xmlString;
			string response = await SendRequest( playDevice, request );

			// Get the response code from the response string
			if ( GetResponseCode( response ) == 200 )
			{
				xmlString = string.Format(
					"<?xml version=\"1.0\"?>\r\n" +
					"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
					"SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
					"<SOAP-ENV:Body>\r\n" +
					"<u:Play xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">\r\n" +
					"<InstanceID>0</InstanceID>\r\n" +
					"<Speed>1</Speed>\r\n" +
					"</u:Play>\r\n" +
					"</SOAP-ENV:Body>\r\n" +
					"</SOAP-ENV:Envelope>\r\n" );

				request = MakeRequest( "POST", playDevice.PlayUrl, xmlString.Length, "urn:schemas-upnp-org:service:AVTransport:1#Play",
					playDevice.IPAddress, playDevice.Port ) + xmlString;
				response = await SendRequest( playDevice, request );
			}
		}

		private string ServerDescription = "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" " +
			"xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">\r\n" +
			"<item>\r\n<dc:title>Capital Edinburgh " + DateTime.Now.Millisecond + "</dc:title>\r\n" +
			"<upnp:class>object.item.audioItem.audioBroadcast</upnp:class>\r\n" +
			"<desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">SA_RINCON65031_</desc>\r\n" +
			"</item>\r\n" +
			"</DIDL-Lite>\r\n";

		/// <summary>
		/// Make a DNLA TCP request 
		/// </summary>
		/// <param name="methord"></param>
		/// <param name="Url"></param>
		/// <param name="ContentLength"></param>
		/// <param name="SOAPAction"></param>
		/// <param name="IP"></param>
		/// <param name="Port"></param>
		/// <returns></returns>
		private static string MakeRequest( string methord, string Url, int contentLength, string SOAPAction, string ipAddress, int port )
		{
			string request = string.Format(
				"{0} /{1} HTTP/1.1\r\nCache-Control: no-cache\r\nConnection: Close\r\nPragma: no-cache\r\nHost: {2}:{3}\r\n" +
				"User-Agent: Microsoft-Windows/6.3 UPnP/1.0 Microsoft-DLNA DLNADOC/1.50\r\nFriendlyName.DLNA.ORG: {4}\r\n",
				methord.ToUpper(), Url, ipAddress, port, Environment.MachineName );

			if ( contentLength > 0 )
			{
				request =  request + string.Format( "Content-Length: {0}\r\nContent-Type: text/xml; charset=\"utf-8\"\r\n", contentLength );
			}

			if ( SOAPAction.Length > 0 )
			{
				request = request + string.Format( "SOAPAction: \"{0}\"\r\n", SOAPAction );
			}

			return request + "\r\n";
		}

		/// <summary>
		/// Extract a response code from an HTTP response
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private static int GetResponseCode( string response )
		{
			int responseCode = 0;

			Match locationMatch = Regex.Match( response, @"HTTP\/1.1 (\d{1,3})" );
			if ( locationMatch.Success == true )
			{
				responseCode = Int32.Parse( locationMatch.Groups[ 1 ].Value );
			}

			return responseCode;
		}

		/// <summary>
		/// Send a request to the device using TCP protocol and await a response
		/// </summary>
		/// <param name="targetDevice"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		private static async Task< string > SendRequest( Device targetDevice, string request )
		{
			string response = "";

			using ( TcpClient client = new TcpClient() )
			{
				try
				{
					// Connect to the client
					await client.ConnectAsync( IPAddress.Parse( targetDevice.IPAddress ), targetDevice.Port );

					// Get the network stream and send out the request
					NetworkStream networkStream = client.GetStream();

					// Convert to bytes
					byte[] requestBytes = Encoding.UTF8.GetBytes( request );
					await networkStream.WriteAsync( requestBytes, 0, requestBytes.Length );

					// Read into a fixed length byte array
					byte[] readBuffer = new byte[ 8000 ];
					int bytesRead;

					do
					{
						bytesRead = await networkStream.ReadAsync( readBuffer, 0, readBuffer.Length );
						if ( bytesRead > 0 )
						{
//							Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "SendRequest Read; {0} bytes from IP {1}:{2}", bytesRead,
//								targetDevice.IPAddress, targetDevice.Port ) );
							response += Encoding.UTF8.GetString( readBuffer, 0, bytesRead );
						}
					}
					while ( bytesRead > 0 );
				}
				catch ( IOException ioProblem )
				{
					Log.WriteLine( LogPriority.Debug, "MobileApp", ioProblem.InnerException.ToString() );
				}
			}

			return response;
		}
	}
}