using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackDevices contains a collection of PlaybackDevice instances
	/// In fact two collections are kept, devices that support pplayback and those that support browsing
	/// </summary>
	internal class PlaybackDevices
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public PlaybackDevices()
		{
			// Create a device for local playback and add it to the playback collection
			localDevice = new PlaybackDevice()
			{
				CanPlayMedia = PlaybackDevice.CanPlayMediaType.Yes,
				IsLocal = true,
				FriendlyName = LocalDeviceName
			};

			PlaybackDeviceCollection.Add( localDevice );
		}

		/// <summary>
		/// Add a device to the collection(s) if it is unique
		/// </summary>
		/// <param name="deviceToAdd"></param>
		/// <returns></returns>
		public bool AddDevice( PlaybackDevice deviceToAdd )
		{
			bool deviceUnique = ( DeviceCollection.Contains( deviceToAdd ) == false );

			if ( deviceUnique == true )
			{
				DeviceCollection.Add( deviceToAdd );

				if ( deviceToAdd.ContentUrl.Length > 0 )
				{
					BrowseableDeviceCollection.Add( deviceToAdd );
				}

				if ( deviceToAdd.CanPlayMedia == PlaybackDevice.CanPlayMediaType.Yes )
				{
					PlaybackDeviceCollection.Add( deviceToAdd );
				}
			}

			return deviceUnique;
		}

		/// <summary>
		/// Remove this device from the collection(s)
		/// </summary>
		/// <param name="device"></param>
		public void RemoveDevice( PlaybackDevice device )
		{
			DeviceCollection.Remove( device );
			PlaybackDeviceCollection.Remove( device );
			BrowseableDeviceCollection.Remove( device );
		}

		/// <summary>
		/// Clear the collections
		/// </summary>
		public void Clear()
		{
			DeviceCollection.Clear();
			PlaybackDeviceCollection.Clear();
			PlaybackDeviceCollection.Add( localDevice );
			BrowseableDeviceCollection.Clear();
		}

		/// <summary>
		/// Which devices have exceeded their communication failure limit
		/// </summary>
		/// <returns></returns>
		public List<PlaybackDevice> MissingDevices() => 
			DeviceCollection.Where( dev => ( ++dev.CommunicationFailureCount > PlaybackDevice.CommunicationFailureLimit ) ).ToList();

		/// <summary>
		/// Return a server with the specified name
		/// </summary>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public PlaybackDevice FindServer( string serverName ) => BrowseableDeviceCollection.SingleOrDefault( dev => dev.FriendlyName == serverName );

		/// <summary>
		/// The collection of devices that support playback
		/// </summary>
		public ObservableCollection<PlaybackDevice> PlaybackDeviceCollection { get; set; } = new ObservableCollection<PlaybackDevice>();

		/// <summary>
		/// The collection of devices that support browsing
		/// </summary>
		public ObservableCollection<PlaybackDevice> BrowseableDeviceCollection { get; set; } = new ObservableCollection<PlaybackDevice>();

		/// <summary>
		/// The collection of all discovered devices
		/// </summary>
		private List<PlaybackDevice> DeviceCollection { get; set; } = new List<PlaybackDevice>();

		private readonly PlaybackDevice localDevice = null;

		/// <summary>
		/// The name of the one and only local device
		/// </summary>
		public const string LocalDeviceName = "Local playback";
	}
}
