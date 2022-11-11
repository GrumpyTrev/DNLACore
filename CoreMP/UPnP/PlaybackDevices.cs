using System.Collections.Generic;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackDevices contains a collection of PlaybackDevice instances
	/// </summary>
	public class PlaybackDevices
	{
		/// <summary>
		/// Add a device to the collection if it is unique
		/// </summary>
		/// <param name="deviceToAdd"></param>
		/// <returns></returns>
		public bool AddDevice( PlaybackDevice deviceToAdd )
		{
			bool deviceUnique = ( DeviceCollection.Contains( deviceToAdd ) == false );

			if ( deviceUnique == true )
			{
				DeviceCollection.Add( deviceToAdd );
			}

			return deviceUnique;
		}

		/// <summary>
		/// Rem ove this device from the collection if present
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public bool RemoveDevice( PlaybackDevice device ) => DeviceCollection.Remove( device );

		/// <summary>
		/// Return a list of all the devices that can play back media
		/// </summary>
		/// <returns></returns>
		public List<string> ConnectedDevices() => DeviceCollection.Select( device => device.FriendlyName ).ToList();

		/// <summary>
		/// Find a device given its friendly name
		/// </summary>
		/// <param name="deviceName"></param>
		/// <returns></returns>
		public PlaybackDevice FindDevice( string deviceName ) => DeviceCollection.SingleOrDefault( device => device.FriendlyName == deviceName );

		/// <summary>
		/// Find the specified device
		/// </summary>
		/// <param name="device"></param>
		/// <returns></returns>
		public PlaybackDevice FindDevice( PlaybackDevice device ) => DeviceCollection.SingleOrDefault( dev => device.Equals( dev ) );

		/// <summary>
		/// The collection of devices
		/// </summary>
		public List<PlaybackDevice> DeviceCollection { get; set; } = new List<PlaybackDevice>();
	}
}
