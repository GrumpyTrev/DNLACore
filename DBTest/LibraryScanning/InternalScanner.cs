using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DBTest
{
	class InternalScanner
	{
		/// <summary>
		/// Public constructor supplying the interface used to store scanned songs
		/// </summary>
		/// <param name="songInterface"></param>
		public InternalScanner( ISongStorage songInterface )
		{
			storageInterface = songInterface;
		}

		/// <summary>
		/// Start scanning the contents of the FTP server at the specified IP address
		/// </summary>
		/// <param name="fileRoot"></param>
		/// <returns></returns>
		public async Task Scan( string fileRoot )
		{
			rootDirectory = fileRoot;
			await ScanDirectory( fileRoot );
		}

		/// <summary>
		/// Scan a directory held at the FTP server 
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		public async Task ScanDirectory( string directoryName )
		{
			if ( ( CancelRequested?.Invoke() ?? false ) == false )
			{
				// Access the directory details
				DirectoryInfo info = new DirectoryInfo( directoryName );

				// Make sure it exists
				if ( info.Exists == true )
				{
					// Use a list to collect all the songs in this folder
					List<ScannedSong> songs = new List<ScannedSong>();

					foreach ( FileInfo fi in info.GetFiles() )
					{
						// Only process MP3 files
						if ( fi.Name.ToUpper().EndsWith( ".MP3" ) )
						{
							Logger.Log( string.Format( "Processing song no {1} : {0}", fi.FullName, ++songCount ) );

							// At this point if the library is only being rescanned then there may be no reason to actually start downloading the file
							if ( storageInterface.DoesSongRequireScanning( fi.FullName.Replace( rootDirectory, "" ) , fi.LastWriteTime ) == true )
							{
								songs.Add( await GetFileTags( fi ) );
							}
						}
					}

					// If any songs are available pass them back via the delegate
					if ( songs.Count > 0 )
					{
						await storageInterface.SongsScanned( songs );
					}

					// Now process the subdirectories
					foreach ( DirectoryInfo diSubDir in info.GetDirectories() )
					{
						await ScanDirectory( diSubDir.FullName );
					}
				}
			}
		}

		/// <summary>
		/// Transfer over enough of the file to extract the MP3 files tags
		/// </summary>
		/// <param name="fileItem"></param>
		/// <returns></returns>
		private async Task<ScannedSong> GetFileTags( FileInfo fileItem )
		{
			ScannedSong song = new ScannedSong();

			await Task.Run( () => 
			{
				try
				{
					song.Modified = fileItem.LastWriteTime;
					song.SourcePath = fileItem.FullName.TrimStart( rootDirectory );

					using ( FileStream fs = File.OpenRead( fileItem.FullName ) )
					{
						song.Tags = MP3TagExtractor.GetFileTags( fs );
					}
				}
				catch ( Exception songProblem )
				{
					Logger.Error( string.Format( "FTP exception reading song: {0} : {1}", fileItem.FullName, songProblem.Message ) );
				}
			} );

			return song;
		}

		/// <summary>
		/// The interface used to store scanned songs
		/// </summary>
		readonly ISongStorage storageInterface = null;

		/// <summary>
		/// Keep a running count of the number of songs scanned
		/// </summary>
		private int songCount;

		/// <summary>
		/// The root directory for this internal scan
		/// </summary>
		private string rootDirectory;

		/// <summary>
		/// Delegate used to determine if this task has been cancelled
		/// </summary>
		/// <returns></returns>
		public delegate bool CancelRequestedDelegate();

		/// <summary>
		/// Instance of CancelRequestedDelegate delegate
		/// </summary>
		public CancelRequestedDelegate CancelRequested { private get; set; } = null;
	}
}