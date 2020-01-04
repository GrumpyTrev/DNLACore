using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// Simple Http server used to serve local files to remote devices
	/// </summary>
	class SimpleHTTPServer
	{
		/// <summary>
		/// Dictionary of mime mappings
		/// </summary>
		private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
			{".asf", "video/x-ms-asf"},
			{".asx", "video/x-ms-asf"},
			{".avi", "video/x-msvideo"},
			{".bin", "application/octet-stream"},
			{".cco", "application/x-cocoa"},
			{".crt", "application/x-x509-ca-cert"},
			{".css", "text/css"},
			{".deb", "application/octet-stream"},
			{".der", "application/x-x509-ca-cert"},
			{".dll", "application/octet-stream"},
			{".dmg", "application/octet-stream"},
			{".ear", "application/java-archive"},
			{".eot", "application/octet-stream"},
			{".exe", "application/octet-stream"},
			{".flv", "video/x-flv"},
			{".gif", "image/gif"},
			{".hqx", "application/mac-binhex40"},
			{".htc", "text/x-component"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".ico", "image/x-icon"},
			{".img", "application/octet-stream"},
			{".iso", "application/octet-stream"},
			{".jar", "application/java-archive"},
			{".jardiff", "application/x-java-archive-diff"},
			{".jng", "image/x-jng"},
			{".jnlp", "application/x-java-jnlp-file"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".mml", "text/mathml"},
			{".mng", "video/x-mng"},
			{".mov", "video/quicktime"},
			{".mp3", "audio/mpeg"},
			{".mpeg", "video/mpeg"},
			{".mpg", "video/mpeg"},
			{".msi", "application/octet-stream"},
			{".msm", "application/octet-stream"},
			{".msp", "application/octet-stream"},
			{".pdb", "application/x-pilot"},
			{".pdf", "application/pdf"},
			{".pem", "application/x-x509-ca-cert"},
			{".pl", "application/x-perl"},
			{".pm", "application/x-perl"},
			{".png", "image/png"},
			{".prc", "application/x-pilot"},
			{".ra", "audio/x-realaudio"},
			{".rar", "application/x-rar-compressed"},
			{".rpm", "application/x-redhat-package-manager"},
			{".rss", "text/xml"},
			{".run", "application/x-makeself"},
			{".sea", "application/x-sea"},
			{".shtml", "text/html"},
			{".sit", "application/x-stuffit"},
			{".swf", "application/x-shockwave-flash"},
			{".tcl", "application/x-tcl"},
			{".tk", "application/x-tcl"},
			{".txt", "text/plain"},
			{".war", "application/java-archive"},
			{".wbmp", "image/vnd.wap.wbmp"},
			{".wmv", "video/x-ms-wmv"},
			{".xml", "text/xml"},
			{".xpi", "application/x-xpinstall"},
			{".zip", "application/zip"},
		};

		/// <summary>
		/// Construct server with given port.
		/// </summary>
		/// <param name="path">Directory path to serve.</param>
		/// <param name="port">Port of the server.</param>
		public SimpleHTTPServer( string path, int port )
		{
			rootDirectory = path;

			// Start listening on the port
			listener = new HttpListener();
			listener.Prefixes.Add( "http://*:" + port.ToString() + "/" );
			listener.Start();
			Listen();
		}

		/// <summary>
		/// Stop server and dispose all functions.
		/// </summary>
		public void Stop()
		{
			listener.Close();
		}

		/// <summary>
		/// Listen for Http requests and process them
		/// </summary>
		private async void Listen()
		{
			while ( true )
			{
				HttpListenerContext context = await listener.GetContextAsync();
				Task.Factory.StartNew( () => Process( context ) );
			}
		}

		/// <summary>
		/// Process an Http request
		/// </summary>
		/// <param name="context"></param>
		private void Process( HttpListenerContext context )
		{
			string filename = context.Request.Url.AbsolutePath;
			filename = filename.Substring( 1 ).Replace( "%20", " " );
			filename = Path.Combine( rootDirectory, filename );

			HttpListenerRequest request = context.Request;

			Logger.Log( string.Format( "Server request - Length: {0}  Content type: {1} Method: {2} KeepAlive: {3} RawUrl: {4} ServiceName: {5} Url: {6}", 
				request.ContentLength64, request.ContentType,
				request.HttpMethod, request.KeepAlive, request.RawUrl, request.ServiceName, request.Url.OriginalString ) );

			if ( ( request.HttpMethod == "HEAD" ) || ( request.HttpMethod == "GET" ) )
			{
				if ( File.Exists( filename ) )
				{
					try
					{
						using ( StreamReader reader = new StreamReader( filename ) )
						{
							context.Response.ContentType = _mimeTypeMappings.TryGetValue( Path.GetExtension( filename ), out string mime ) ? mime : "application/octet-stream";
							context.Response.ContentLength64 = reader.BaseStream.Length;
							context.Response.AddHeader( "Date", DateTime.Now.ToString( "r" ) );
							context.Response.AddHeader( "Last-Modified", System.IO.File.GetLastWriteTime( filename ).ToString( "r" ) );

							if ( request.HttpMethod == "GET" )
							{
								Logger.Log( "Serving file: " + filename );

								using ( BinaryReader bReader = new BinaryReader( reader.BaseStream ) )
								{
									byte[] bytes = bReader.ReadBytes( ( int )reader.BaseStream.Length );
									context.Response.OutputStream.Write( bytes, 0, bytes.Length );
								}

								context.Response.OutputStream.Flush();
							}
						}

						context.Response.StatusCode = ( int )HttpStatusCode.OK;
					}
					catch ( Exception )
					{
						context.Response.StatusCode = ( int )HttpStatusCode.InternalServerError;
					}
				}
				else
				{
					context.Response.StatusCode = ( int )HttpStatusCode.NotFound;
				}
			}
			else
			{
				context.Response.StatusCode = ( int )HttpStatusCode.MethodNotAllowed;
			}

			context.Response.OutputStream.Close();
		}

		/// <summary>
		/// The root directory for the actual physical location of the files
		/// </summary>
		private readonly string rootDirectory;

		/// <summary>
		/// The Http listener
		/// </summary>
		private HttpListener listener;
	}
}