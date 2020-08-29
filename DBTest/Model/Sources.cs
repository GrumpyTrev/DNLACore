﻿using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Sources class holds a collection of all the Source entries read from storage.
	/// </summary>	
	static class Sources
	{
		/// <summary>
		/// Get the Sources collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( SourceCollection == null )
			{
				// Get the current set of sources
				SourceCollection = await LibraryAccess.GetAllSourcesAsync();

				// Replace special placeholders with IP addresses
				foreach ( Source source in SourceCollection )
				{
					if ( source.ScanType == "Local" )
					{
						source.LocalAccess = source.LocalAccess.Replace( "[*]", LocalIPAddress );
						source.RemoteAccess = source.RemoteAccess.Replace( "[*]", LocalIPAddress );
						source.ScanSource = source.ScanSource.Replace( "[*]", LocalIPAddress );
					}
					else if ( source.ScanType == "FTP" )
					{
						source.LocalAccess = source.LocalAccess.Replace( "[s]", source.ScanSource );
						source.RemoteAccess = source.RemoteAccess.Replace( "[s]", source.ScanSource );
					}
				}
			}
		}

		/// <summary>
		/// Return a list of all the sources associated with the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static List<Source> GetSourcesForLibrary( int libraryId ) => SourceCollection.Where( sou => sou.LibraryId == libraryId ).ToList();

		/// <summary>
		/// Get the sources associated with a library and their associated songs
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task< List<Source> > GetSourcesAndSongsForLibraryAsync( int libraryId )
		{
			List<Source> sources = SourceCollection.Where( sou => sou.LibraryId == libraryId ).ToList();
			foreach ( Source source in sources )
			{
				if ( source.Songs == null )
				{
					source.Songs = await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => song.SourceId == source.Id ).ToListAsync();
				}
			}

			return sources;
		}

		/// <summary>
		/// The set of Albums currently held in storage
		/// </summary>
		public static List<Source> SourceCollection { get; set; } = null;

		/// <summary>
		/// Get the IP address of the local device
		/// </summary>
		/// <param name="_type"></param>
		/// <returns></returns>
		private static string GetLocalIPv4( NetworkInterfaceType _type )
		{
			string output = "";
			foreach ( NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces() )
			{
				if ( item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up )
				{
					foreach ( UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses )
					{
						if ( ip.Address.AddressFamily == AddressFamily.InterNetwork )
						{
							output = ip.Address.ToString();
						}
					}
				}
			}
			return output;
		}

		/// <summary>
		/// The IP address of the local device
		/// </summary>
		private static string LocalIPAddress
		{
			get
			{
				if ( localIPAddress == null )
				{
					localIPAddress = GetLocalIPv4( NetworkInterfaceType.Wireless80211 );
				}

				return localIPAddress;
			}
		}

		/// <summary>
		/// The local IP address stored as a string
		/// </summary>
		private static string localIPAddress = null;
	}
}