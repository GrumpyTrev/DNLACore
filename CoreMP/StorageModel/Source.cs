using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CoreMP
{
	/// <summary>
	/// The Source class specifies where a set of somngs can be found on a local or remote device
	/// </summary>
	public class Source
	{
		[Obsolete( "Do not create model instances directly", false )]
		public Source() { }

		public virtual int Id { get; set; }

		/// <summary>
		/// The name of this source for display purposes
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// How id this source accessed, typically for scanning purposes
		/// </summary>
		public AccessType AccessMethod { get; set; }

		/// <summary>
		/// The different ways in which the source is accessed (for scannig purposes mainly)
		/// </summary>
		public enum AccessType
		{
			Local, FTP, UPnP
		}

		/// <summary>
		/// The IP address of the device where the music is stored
		/// If left blank this indicates that the local IP address of the phone should be used
		/// </summary>
		public string IPAddress { get; set; }

		/// <summary>
		/// The location on the device where the songs can be found.
		/// For local devices this will be the full name of the directory on the device
		/// For remote devices this will be the name that the device's HTTP server responds to
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// The HTTP port number
		/// </summary>
		public int PortNo { get; set; }

		/// <summary>
		/// The library this source is associated with
		/// </summary>
		public int LibraryId { get; set; }

		/// <summary>
		/// Initialise the access attributes from those read from the database
		/// </summary>
		public void InitialiseAccess()
		{
			if ( AccessMethod == AccessType.Local )
			{
				// If the IPAddress is blank then use the local IP address
				if ( ( IPAddress == null ) || ( IPAddress.Length == 0 ) )
				{
					IPAddress = LocalIPAddress;
				}

				ScanSource = string.Format( "/{0}/", FolderName );
				LocalAccess = string.Format( "/{0}", FolderName );
				RemoteAccess = string.Format( "http://{0}:{1}/{2}", IPAddress, PortNo, FolderName );
			}
			else if ( AccessMethod == AccessType.FTP )
			{
				ScanSource = IPAddress;
				LocalAccess = string.Format( "http://{0}:{1}/{2}", IPAddress, PortNo, FolderName );
				RemoteAccess = string.Format( "http://{0}:{1}/{2}", IPAddress, PortNo, FolderName );
			}
			else
			{
				ScanSource = Name;
			}
		}

		/// <summary>
		/// Update the source and save it to storaage
		/// </summary>
		/// <param name="newSource"></param>
		public virtual void UpdateSource( Source newSource )
		{
			Name = newSource.Name;
			FolderName = newSource.FolderName;
			IPAddress = newSource.IPAddress;
			PortNo = newSource.PortNo;
			AccessMethod = newSource.AccessMethod;

			// Make sure that member variable that depend on this data are also updated
			InitialiseAccess();
		}

		/// <summary>
		/// Get all of the songs associated with this source
		/// </summary>
		public void GetSongs()
		{
			if ( Songs == null )
			{
				Songs = CoreMP.Songs.GetSourceSongs( Id );
			}
		}

		/// <summary>
		/// The source used when scanning - derived from above
		/// For remote devices this is '{IPAddress}'
		/// For local devices this is '/{FolderName}/'
		/// </summary>
		public virtual string ScanSource { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them locally
		/// For remote devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// For local devices this will be '/{FolderName}'
		/// </summary>
		public virtual string LocalAccess { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them remotely
		/// For both remote and local devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// </summary>
		public virtual string RemoteAccess { get; set; }

		/// <summary>
		/// All the songs associated with this source
		/// </summary>
		public virtual List<Song> Songs { get; set; }

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
		/// The local IP address stored as a string
		/// </summary>
		private static string localIPAddress = null;
	}
}
