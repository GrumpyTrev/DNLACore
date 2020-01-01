using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DBTest
{
	class FTPScanner
	{
		/// <summary>
		/// Public constructor supplying the interface used to store scanned songs
		/// </summary>
		/// <param name="songInterface"></param>
		public FTPScanner( ISongStorage songInterface )
		{
			storageInterface = songInterface;
		}

		/// <summary>
		/// Start scanning the contents of the FTP server at the specified IP address
		/// </summary>
		/// <param name="ftpIpAddress"></param>
		/// <returns></returns>
		public async Task Scan( string ftpIpAddress )
		{
			ftpHeader = "ftp://" + ftpIpAddress;
			await ScanDirectory( "" );
		}

		/// <summary>
		/// Scan a directory held at the FTP server 
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		public async Task ScanDirectory( string directoryName )
		{
			// Get the directory contents from the server
			FtpWebResponse response = await new FtpRequest( ftpHeader + directoryName, WebRequestMethods.Ftp.ListDirectoryDetails, false ).MakeRequestAsync();

			// Read the response into a string
			string rawDirectoryListing = "";

			using ( Stream responseStream = response.GetResponseStream() )
			using ( StreamReader reader = new StreamReader( responseStream ) )
			{
				rawDirectoryListing = await reader.ReadToEndAsync();
			}

			// Parse the response into a list of DirectoryItem ( include files )
			List<DirectoryItem> items = new List<DirectoryItem>();

			foreach ( string line in rawDirectoryListing.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries ) )
			{
				// Parse date
				DateTime dateTime = DateTime.MinValue;
				try
				{
					dateTime = DateTime.ParseExact( line.Substring( 0, 17 ), "MM-dd-yy  hh:mmtt", CultureInfo.InvariantCulture );
				}
				catch ( FormatException )
				{
				}

				// Add to list
				items.Add( new DirectoryItem {
					Created = dateTime,
					IsDirectory = ( line.Substring( 24, 5 ).ToUpper() == "<DIR>" ),
					Name = line.Substring( 39 ),
					Base = directoryName
				} ); 
			}

			// Use a list to collect all the songs in this folder
			List<ScannedSong> songs = new List<ScannedSong>();

			// Use a while loop and check for cancellation
			int itemIndex = 0;
			while ( ( itemIndex < items.Count ) && ( ( CancelRequested?.Invoke() ?? false ) == false ) )
			{
				DirectoryItem item = items[ itemIndex++ ];

				// If this is a directory then scan it
				if ( item.IsDirectory == true )
				{
					await ScanDirectory( item.AbsolutePath );
				}
				else
				{
					// Only process MP3 files
					if ( item.Name.ToUpper().EndsWith( ".MP3" ) )
					{
						Logger.Log( string.Format( "Processing song no {1} : {0}", item.AbsolutePath, ++songCount ) );

						// At this point if the library is only being rescanned then there may be no reason to actually start downloading the file
						if ( storageInterface.DoesSongRequireScanning( item.AbsolutePath, item.Created ) == true )
						{
							songs.Add( await GetFileTags( item ) );
						}
					}
				}
			}

			// If any songs are available pass them back via the delegate
			if ( songs.Count > 0 )
			{
				await storageInterface.SongsScanned( songs );
			}
		}

		/// <summary>
		/// Transfer over enough of the file to extract the MP3 files tags
		/// </summary>
		/// <param name="fileItem"></param>
		/// <returns></returns>
		private async Task<ScannedSong> GetFileTags( DirectoryItem fileItem )
		{
			ScannedSong song = new ScannedSong();

			try
			{
				song.Modified = fileItem.Created;
				song.SourcePath = fileItem.AbsolutePath;

				string requestName = ftpHeader + fileItem.AbsolutePath;

				// Get the length of the file
				FtpWebResponse lengthResponse = await new FtpRequest( requestName, WebRequestMethods.Ftp.GetFileSize, true ).MakeRequestAsync();
				long fileSize = lengthResponse.ContentLength;

				// Start downloading the file
				FtpRequest request = new FtpRequest( requestName, WebRequestMethods.Ftp.DownloadFile, true );
				FtpWebResponse response = await request.MakeRequestAsync();

				// Read the file to get the MP3 tags.
				FTPStream wrappedStream = new FTPStream( response.GetResponseStream(), fileSize );

				song.Tags = MP3TagExtractor.GetFileTags( wrappedStream );

				// Read the FTP response and prevent stale data on the socket
				request.AbortRequest();
				wrappedStream.Close();
				response.Close();
			}
			catch ( Exception songProblem )
			{
				Logger.Error( string.Format( "FTP exception reading song: {0} : {1}", fileItem.AbsolutePath, songProblem.Message ) );
			}

			return song;
		}

		/// <summary>
		/// The interface used to store scanned songs
		/// </summary>
		readonly ISongStorage storageInterface = null;

		/// <summary>
		/// The header for all FTP requests, typically the ip address
		/// </summary>
		private string ftpHeader = "";

		/// <summary>
		/// Keep a running count of the number of songs scanned
		/// </summary>
		private int songCount;

		/// <summary>
		/// Delegate used to determine if this task has been cancelled
		/// </summary>
		/// <returns></returns>
		public delegate bool CancelRequestedDelegate();

		/// <summary>
		/// Instance of CancelRequestedDelegate delegate
		/// </summary>
		public CancelRequestedDelegate CancelRequested { private get; set; } = null;

		/// <summary>
		/// Directory and file information gathered during directory requests to the FTP server
		/// </summary>
		private class DirectoryItem
		{
			public DateTime Created { get; set; }
			public bool IsDirectory { get; set; }
			public string Name { get; set; }
			public string Base { get; set; }
			public string AbsolutePath
			{
				get
				{
					return string.Format( "{0}/{1}", Base, Name );
				}
			}
		}
	}

	/// <summary>
	/// The FTPRequest wraps up an FtpWebRequest and FtpWebResponse sequence
	/// </summary>
	internal class FtpRequest
	{
		/// <summary>
		/// Create an FtpWebRequest with the speciifed parameters
		/// </summary>
		/// <param name="requestName"></param>
		/// <param name="method"></param>
		/// <param name="binary"></param>
		public FtpRequest( string requestName, string method, bool binary )
		{
			Request = ( FtpWebRequest )WebRequest.Create( requestName.Replace( "#", "%23" ) );

			Request.Method = method;
			Request.Credentials = new NetworkCredential( "anonymous", "anonymous" );
			Request.UsePassive = true;
			Request.UseBinary = binary;
			Request.KeepAlive = true;
			Request.ConnectionGroupName = "Grumpy";
		}

		/// <summary>
		/// Make the request and get a response
		/// </summary>
		/// <returns></returns>
		public async Task<FtpWebResponse> MakeRequestAsync()
		{
			return ( FtpWebResponse )await Request.GetResponseAsync();
		}

		/// <summary>
		/// Abort a long running FtpWebRequest
		/// </summary>
		public void AbortRequest()
		{
			Request.Abort();
		}

		/// <summary>
		/// The FtpWebRequest used to carry out the request
		/// </summary>
		private FtpWebRequest Request { get; set; } = null;
	}

	/// <summary>
	/// The FTPStream is derived from the generic Stream class to enable incremental reads from the FTP server to be possible
	/// </summary>
	internal class FTPStream: Stream
	{
		public FTPStream( Stream sourceStream, long streamLength )
		{
			BaseStream = sourceStream;
			BaseLength = streamLength;
			BasePosition = 0;
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			int bytesRead = BaseStream.Read( buffer, offset, count );
			BasePosition += bytesRead;
			return bytesRead;
		}

		public override bool CanRead => BaseStream.CanRead;

		public override bool CanSeek => BaseStream.CanSeek;

		public override bool CanWrite => BaseStream.CanWrite;

		public override long Length => BaseLength;

		public override long Position { get => BasePosition; set => throw new NotImplementedException(); }

		public override void Flush() => BaseStream.Flush();

		public override long Seek( long offset, SeekOrigin origin ) => throw new NotImplementedException();

		public override void SetLength( long value ) => throw new NotImplementedException();

		public override void Write( byte[] buffer, int offset, int count ) => throw new NotImplementedException();

		private Stream BaseStream { get; set; }
		private long BaseLength { get; set; }
		private long BasePosition { get; set; }
	}
}