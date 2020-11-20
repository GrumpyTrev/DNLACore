using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DBTest
{
	public static class DlnaRequestHelper
	{
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
		public static string MakeRequest( string method, string Url, string SOAPAction, string ipAddress, int port, string content )
		{
			string request = $"{method.ToUpper()} /{Url} HTTP/1.1\r\nCache-Control: no-cache\r\nConnection: Close\r\nPragma: no-cache\r\n" + 
				$"Host: {ipAddress}:{port}\r\nUser-Agent: Microsoft-Windows/6.3 UPnP/1.0 Microsoft-DLNA DLNADOC/1.50\r\n" +
				$"FriendlyName.DLNA.ORG: {System.Environment.MachineName}\r\n";

			if ( content.Length > 0 )
			{
				request += $"Content-Length: {content.Length}\r\nContent-Type: text/xml; charset=\"utf-8\"\r\n";
			}

			if ( SOAPAction.Length > 0 )
			{
				request += $"SOAPAction: \"{SOAPAction}\"\r\n";
			}

			request += "\r\n";

			if ( content.Length > 0 )
			{
				request += content;
			}

			return request;
		}

		/// <summary>
		/// Format a SOAP request with an action and some action specific items
		/// </summary>
		/// <param name="action"></param>
		/// <param name="actionSpecific"></param>
		/// <returns></returns>
		public static string MakeSoapRequest( string action, string actionSpecific = "" ) =>
				"<?xml version=\"1.0\"?>\r\n" +
				"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
				"SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
				"<SOAP-ENV:Body>\r\n" +
				$"<u:{action} xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">\r\n" +
				"<InstanceID>0</InstanceID>\r\n" +
				$"{actionSpecific}" +
				$"</u:{action}>\r\n" +
				"</SOAP-ENV:Body>\r\n" +
				"</SOAP-ENV:Envelope>\r\n";

		/// <summary>
		/// Send a request to the device using TCP protocol and await a response
		/// </summary>
		/// <param name="targetDevice"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public static async Task<string> SendRequest( PlaybackDevice targetDevice, string request )
		{
			string response = "";

			await socketLock.WaitAsync();
			try
			{
				using ( TcpClient client = new TcpClient() )
				{
					try
					{
						// Connect to the client
						if ( client.ConnectAsync( IPAddress.Parse( targetDevice.IPAddress ), targetDevice.Port ).Wait( MillisecondsTimeout ) == true )
						{
							// Get the network stream and send out the request
							NetworkStream networkStream = client.GetStream();

							// Convert to bytes
							byte[] requestBytes = Encoding.UTF8.GetBytes( request );
							await networkStream.WriteAsync( requestBytes, 0, requestBytes.Length );

							int bytesRead;

							do
							{
								bytesRead = await networkStream.ReadAsync( readBuffer, 0, readBuffer.Length );
								if ( bytesRead > 0 )
								{
									response += Encoding.UTF8.GetString( readBuffer, 0, bytesRead );
								}
							}
							while ( bytesRead > 0 );
						}
						else
						{
							Logger.Error( "Timeout sending DLNA request" );
						}
					}
					catch ( SocketException sktProblem )
					{
						Logger.Error( sktProblem.ToString() );
						response = "";
					}
					catch ( IOException ioProblem )
					{
						Logger.Error( ioProblem.ToString() );
						response = "";
					}
					catch ( AggregateException combinedProblem )
					{
						Logger.Error( combinedProblem.ToString() );
						response = "";
					}
				}
			}
			finally
			{
				socketLock.Release();
			}

			return response;
		}

		/// <summary>
		/// Extract a response code from an HTTP response
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public static int GetResponseCode( string response )
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
		/// Lock object to prevent multiple threads accessing the device
		/// </summary>
		private static readonly SemaphoreSlim socketLock = new SemaphoreSlim( 1 );

		/// <summary>
		/// Timeout for DNLA connection
		/// </summary>
		private const int MillisecondsTimeout = 10000;

		/// <summary>
		/// Size of buffer to read DNLA response
		/// </summary>
		private const int ReadBufferSize = 2000;

		/// <summary>
		/// As only one request is allowed at a time make the buffer static
		/// </summary>
		private static readonly byte[] readBuffer = new byte[ ReadBufferSize ];
	}
}