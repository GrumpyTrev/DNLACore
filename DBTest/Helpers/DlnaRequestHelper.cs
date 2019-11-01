using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.Util;

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
			string request = string.Format(
				"{0} /{1} HTTP/1.1\r\nCache-Control: no-cache\r\nConnection: Close\r\nPragma: no-cache\r\nHost: {2}:{3}\r\n" +
				"User-Agent: Microsoft-Windows/6.3 UPnP/1.0 Microsoft-DLNA DLNADOC/1.50\r\nFriendlyName.DLNA.ORG: {4}\r\n",
				method.ToUpper(), Url, ipAddress, port, System.Environment.MachineName );

			if ( content.Length > 0 )
			{
				request += string.Format( "Content-Length: {0}\r\nContent-Type: text/xml; charset=\"utf-8\"\r\n", content.Length );
			}

			if ( SOAPAction.Length > 0 )
			{
				request += string.Format( "SOAPAction: \"{0}\"\r\n", SOAPAction );
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
		public static string MakeSoapRequest( string action, string actionSpecific )
		{
			return string.Format(
				"<?xml version=\"1.0\"?>\r\n" +
				"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
				"SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n" +
				"<SOAP-ENV:Body>\r\n" +
				"<u:{0} xmlns:u=\"urn:schemas-upnp-org:service:AVTransport:1\">\r\n" +
				"<InstanceID>0</InstanceID>\r\n" +
				"{1}" +
				"</u:{0}>\r\n" +
				"</SOAP-ENV:Body>\r\n" +
				"</SOAP-ENV:Envelope>\r\n",
				action, actionSpecific );
		}

		/// <summary>
		/// Send a request to the device using TCP protocol and await a response
		/// </summary>
		/// <param name="targetDevice"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public static async Task<string> SendRequest( Device targetDevice, string request )
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
						await client.ConnectAsync( IPAddress.Parse( targetDevice.IPAddress ), targetDevice.Port );

						// Get the network stream and send out the request
						NetworkStream networkStream = client.GetStream();

						// Convert to bytes
						byte[] requestBytes = Encoding.UTF8.GetBytes( request );
						await networkStream.WriteAsync( requestBytes, 0, requestBytes.Length );

						// Read into a fixed length byte array
						byte[] readBuffer = new byte[ 2000 ];
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
	}
}