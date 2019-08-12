using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Util;

namespace DBTest
{
	internal class FTPStream : Stream
	{
		public FTPStream( Stream sourceStream, long streamLength )
		{
			BaseStream = sourceStream;
			BaseLength = streamLength;
			BasePosition = 0;
		}

		public override bool CanRead => BaseStream.CanRead;

		public override bool CanSeek => BaseStream.CanSeek;

		public override bool CanWrite => BaseStream.CanWrite;

		public override long Length => BaseLength;

		public override long Position { get => BasePosition; set => throw new NotImplementedException(); }

		public override void Flush()
		{
			BaseStream.Flush();
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			int bytesRead = BaseStream.Read( buffer, offset, count );
			BasePosition += bytesRead;
			return bytesRead;
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotImplementedException();
		}

		public override void SetLength( long value )
		{
			throw new NotImplementedException();
		}

		public override void Write( byte[] buffer, int offset, int count )
		{
			throw new NotImplementedException();
		}

		private Stream BaseStream { get; set; }
		private long BaseLength { get; set; }
		private long BasePosition { get; set; }
	}

	class FTPScanner
	{
		public class DirectoryItem
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

		private string ftpHeader = "";

		public async Task Scan( string ftpIpAddress )
		{
			ftpHeader = "ftp://" + ftpIpAddress;
			await ScanDirectory( "" );
		}

		public async Task ScanDirectory( string directoryName )
		{
			// Get the object used to communicate with the server.
			// Form name to use in the request replace '#' with '%23'

			string requestName = ftpHeader + directoryName.Replace( "#", "%23" );
			FtpWebRequest request = ( FtpWebRequest )WebRequest.Create( requestName );
			request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

			// This example assumes the FTP site uses anonymous logon.
			request.Credentials = new NetworkCredential( "anonymous", "anonymous" );

			request.UsePassive = true;
			request.UseBinary = false;
			request.KeepAlive = true;
			request.ConnectionGroupName = "Grumpy";

			FtpWebResponse response = ( FtpWebResponse )await request.GetResponseAsync();

			string rawDirectoryListing = null;

			using ( Stream responseStream = response.GetResponseStream() )
			using ( StreamReader reader = new StreamReader( responseStream ) )
			{
				rawDirectoryListing = await reader.ReadToEndAsync();
			}

			// Parse the response
			List<DirectoryItem> items = new List<DirectoryItem>();

			string[] list = rawDirectoryListing.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
			foreach ( string line in list )
			{
				string data = line;

				// Parse date
				string date = data.Substring( 0, 17 );
				DateTime dateTime = DateTime.MinValue;
				try
				{
					dateTime = DateTime.ParseExact( date, "MM-dd-yy  hh:mmtt", CultureInfo.InvariantCulture );
				}
				catch ( FormatException exception )
				{
				}

				data = data.Remove( 0, 24 );

				// Parse <DIR>
				string dir = data.Substring( 0, 5 );
				bool isDirectory = dir.Equals( "<dir>", StringComparison.InvariantCultureIgnoreCase );
				data = data.Remove( 0, 5 );
				data = data.Remove( 0, 10 );

				// Parse name
				string name = data;

				// Create directory info
				DirectoryItem item = new DirectoryItem();
				item.Created = dateTime;
				item.IsDirectory = isDirectory;
				item.Name = name;
				item.Base = directoryName;
				items.Add( item );
			}

			// Use a SongsScannedArgs to collect all the songs in this folder
			SongsScannedArgs songsArg = new SongsScannedArgs();

			foreach ( DirectoryItem item in items )
			{
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
						songsArg.ScannedSongs.Add( await GetFileTags( item ) );
					}
				}
			}

			SongsScanned?.Invoke(this, songsArg);
		}

		private async Task<ScannedSong> GetFileTags( DirectoryItem fileItem )
		{
			ScannedSong song = new ScannedSong();

			Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Processing song no {1} : {0}", fileItem.AbsolutePath, ++songCount ) );

			try
			{
				song.Modified = fileItem.Created;
				song.SourcePath = fileItem.AbsolutePath;

				string requestName = ftpHeader + fileItem.AbsolutePath.Replace( "#", "%23" );

				// Get the length of the file
				FtpWebRequest lengthRequest = ( FtpWebRequest )WebRequest.Create( requestName );
				lengthRequest.Method = WebRequestMethods.Ftp.GetFileSize;
				lengthRequest.Credentials = new NetworkCredential( "anonymous", "anonymous" );
				lengthRequest.UsePassive = true;
				lengthRequest.UseBinary = true;
				lengthRequest.KeepAlive = true;
				lengthRequest.ConnectionGroupName = "Grumpy";

				FtpWebResponse lengthResponse = ( FtpWebResponse )await lengthRequest.GetResponseAsync();
				long fileSize = lengthResponse.ContentLength;

				// Get the object used to communicate with the server.
				FtpWebRequest request = ( FtpWebRequest )WebRequest.Create( requestName );
				request.Method = WebRequestMethods.Ftp.DownloadFile;

				// This example assumes the FTP site uses anonymous logon.
				request.Credentials = new NetworkCredential( "anonymous", "anonymous" );
				request.UsePassive = true;
				request.UseBinary = true;
				request.KeepAlive = true;
				request.ConnectionGroupName = "Grumpy";

				FtpWebResponse response = ( FtpWebResponse )await request.GetResponseAsync();

				// Read the file to get the MP3 tags.
				FTPStream wrappedStream = new FTPStream( response.GetResponseStream(), fileSize );

				song.Tags = MP3TagExtractor.GetFileTags( wrappedStream );

				// Read the FTP response and prevent stale data on the socket
				request.Abort();
				wrappedStream.Close();
				response.Close();
			}
			catch ( Exception songProblem )
			{
				Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "FTP exception reading song: {0} : {1}",
					fileItem.AbsolutePath, songProblem.Message ) );
			}

			return song;
		}

		/// <summary>
		/// The event used to indicate that a set of songs have been scanned
		/// </summary>
		public event EventHandler< SongsScannedArgs > SongsScanned;

		/// <summary>
		/// Identity of a discovered device
		/// </summary>
		public class SongsScannedArgs: EventArgs
		{
			public List<ScannedSong> ScannedSongs { get; set; } = new List<ScannedSong>();
		}
		
		private static int songCount;
	}
}