using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DBTest
{
	internal class UPnPScanner
	{
		/// <summary>
		/// Public constructor supplying the interface used to store scanned songs
		/// </summary>
		/// <param name="songInterface"></param>
		public UPnPScanner( SongStorage songInterface ) => storageInterface = songInterface;

		/// <summary>
		/// Start the process of retrieving music content from the specfied Content Server
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public async Task Scan( string serverName )
		{
			// See if there is a UPnP server available with the specified name
			server = RemoteDevices.FindDevice( serverName );
			if ( server != null )
			{
				// Traverse the directories returned from the server
				await ScanDirectory( "0", 0, false, 1 );
			}
		}

		/// <summary>
		/// Scan a directory held at the UPnP server 
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		public async Task ScanDirectory( string objectId, int directoryLevel, bool inStorageFolder, int childCount )
		{
			if ( ( CancelRequested?.Invoke() ?? false ) == false )
			{
				List<BrowseFolderItem> items = await GetDirectoryContents( objectId, childCount );

				foreach ( BrowseFolderItem item in items )
				{
					Logger.Log( $"Item id={item.id} title={item.title} child count={item.childCountContainer} class={item.@class} child type={item.containerContent}" );

					// Only process folders that contain music tracks
					if ( item.containerContent == "object.item.audioItem.musicTrack" )
					{
						// Is this contains sub-folders then scan them
						if ( item.childCountContainer != "0" )
						{
							// Only go down into containers if their type is 'object.container.storageFolder', or its the top level
							if ( ( item.@class == "object.container.storageFolder" ) || ( directoryLevel == 0 ) )
							{
								await ScanDirectory( item.id, directoryLevel + 1, ( item.@class == "object.container.storageFolder" ), 
									int.Parse( item.childCountContainer ) );
							}
						}
						else
						{
							// Only scan songs if we are in a storageFolder
							if ( inStorageFolder == true )
							{
								await ScanItems( item.id, item.childCount );
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Scan all the items in the speciifed directory and pass them to the song storage member
		/// </summary>
		/// <param name="directoryId"></param>
		/// <returns></returns>
		private async Task ScanItems( string directoryId, string itemCount )
		{
			if ( ( CancelRequested?.Invoke() ?? false ) == false )
			{
				int numItems = int.Parse( itemCount );
				int startingIndex = 0;
				bool error = false;

				// Continue reading items until they have all been read or there is an error
				while ( ( startingIndex < numItems ) && ( error == false ) )
				{
					// Get the next set of items
					string result = await SendBrowseRequest( directoryId, startingIndex );

					if ( result.Length > 0 )
					{
						// Deserialise the response into a collection of BrowseItems 
						BrowseItemResponse responseItems = BrowseItemResponse.DeserialiseResponse( result );

						if ( responseItems != null )
						{
							// Use a list to collect all the songs in this response
							List<ScannedSong> songs = new();

							// Process each BrowseItem
							foreach ( BrowseItem item in responseItems.Items )
							{
								Logger.Log( $"Processing song no {++songNo} : [{item.artist[ 0 ].Value}] [{item.title}] [{item.album}] [{item.originalTrackNumber}]" );

								// For the filepath use the item descriptor but remove the ip address and port as these are specific to the current device's 
								// network config which may change
								string filePath = item.res.Value.Replace( $"http://{server.IPAddress}:{server.Port}/", "" );

								DateTime modifiedTime = DateTimeOffset.FromUnixTimeSeconds( Int32.Parse( item.modificationTime ) ).LocalDateTime;

								// At this point if the library is only being rescanned then there may be no reason to actually add the song
								if ( storageInterface.DoesSongRequireScanning( filePath, modifiedTime ) == true )
								{
									// Create a ScannedSong from this item and add it to the list
									ScannedSong itemSong = MakeScannedSong( item );
									itemSong.Modified = modifiedTime;
									itemSong.SourcePath = filePath;

									songs.Add( itemSong );
								}
							}

							// If any songs are available pass them back via the delegate
							if ( songs.Count > 0 )
							{
								await storageInterface.SongsScanned( songs );
							}

							startingIndex += responseItems.Items.Length;
						}
						else
						{
							error = true;
						}
					}
					else
					{
						error = true;
					}
				}
			}
		}

		/// <summary>
		/// Read folder entries from the specified direcory
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="childCount"></param>
		/// <returns></returns>
		private async Task<List<BrowseFolderItem>> GetDirectoryContents( string objectId, int childCount )
		{
			List<BrowseFolderItem> items = new();

			int startingIndex = 0;
			bool error = false;

			// Continue reading directory entries until they have all been read or there is an error
			while ( ( startingIndex < childCount ) && ( error == false ) )
			{
				string result = await SendBrowseRequest( objectId, startingIndex );

				if ( result.Length > 0 )
				{
					BrowseFolderResponse responseItems = BrowseFolderResponse.DeserialiseResponse( result );

					if ( responseItems != null )
					{
						items.AddRange( responseItems.Items );
						startingIndex += responseItems.Items.Length;
					}
					else
					{
						error = true;
					}
				}
				else
				{
					error = true;
				}
			}

			return items;
		}

		/// <summary>
		/// Send a browse request to the server and return the response
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="startingIndex"></param>
		/// <returns></returns>
		private async Task<string> SendBrowseRequest( string objectId, int startingIndex )
		{
			string soapContent = DlnaRequestHelper.MakeSoapFromBody(
				"<u:Browse xmlns:u=\"urn:schemas-upnp-org:service:ContentDirectory:1\">\r\n" +
				$"<ObjectID>{objectId}</ObjectID>\r\n" +
				"<BrowseFlag>BrowseDirectChildren</BrowseFlag>\r\n" +
				"<Filter>*</Filter>\r\n" +
				$"<StartingIndex>{startingIndex}</StartingIndex>\r\n" +
				"<RequestedCount>200</RequestedCount>\r\n" +
				"<SortCriteria/>\r\n" +
				"</u:Browse>" );

			string request = DlnaRequestHelper.MakeRequest( "POST", server.ContentUrl, "urn:schemas-upnp-org:service:ContentDirectory:1#Browse",
				server.IPAddress, server.Port, soapContent );

			// Run off the calling thread
			string response = await Task.Run( () => DlnaRequestHelper.SendRequest( server, request ) );

			string result = "";

			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				// Get the Result field
				result = WebUtility.HtmlDecode( response.TrimStart( "<Result>" ).TrimAfter( "</Result>" ) );
			}

			return result;
		}

		/// <summary>
		/// Make a ScannedSong entry from a browsed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private ScannedSong MakeScannedSong( BrowseItem item )
		{
			ScannedSong itemSong = new() { Tags = new() };
			itemSong.Tags.Title = item.title;
			itemSong.Tags.Artist = item.artist[ 0 ].Value;
			itemSong.Tags.Track = item.originalTrackNumber;
			itemSong.Tags.Length = TimeSpan.Parse( item.res.duration );
			itemSong.Tags.Album = item.album;
			itemSong.Tags.Genre = item.genre;
			itemSong.Tags.Year = item.date?[ ..4 ] ?? "0";
			itemSong.Tags.AlbumArtist = item.albumArtist;

			return itemSong;
		}

		/// <summary>
		/// The Remote Devices available to be scanned
		/// </summary>
		public PlaybackDevices RemoteDevices { get; set; }

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
		/// The interface used to store scanned songs
		/// </summary>
		private readonly SongStorage storageInterface = null;

		/// <summary>
		/// The UPnP server to get content from
		/// </summary>
		private PlaybackDevice server = null;

		/// <summary>
		/// Running count of scanned songs
		/// </summary>
		private int songNo = 0;
	}
}
