using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Source class specifies where a set of somngs can be found on a local or remote device
	/// </summary>
	public partial class Source
	{
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
		public void UpdateSource( Source newSource )
		{
			Name = newSource.Name;
			FolderName = newSource.FolderName;
			IPAddress = newSource.IPAddress;
			PortNo = newSource.PortNo;
			AccessMethod = newSource.AccessMethod;

			// Make sure that member variable that depend on this data are also updated
			InitialiseAccess();

			// No need to wait for this to be written to storage
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
		[Ignore]
		public string ScanSource { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them locally
		/// For remote devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// For local devices this will be '/{FolderName}'
		/// </summary>
		[Ignore]
		public string LocalAccess { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them remotely
		/// For both remote and local devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// </summary>
		[Ignore]
		public string RemoteAccess { get; set; }

		/// <summary>
		/// All the songs associated with this source
		/// </summary>
		[Ignore]
		public List<Song> Songs { get; set; }

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
