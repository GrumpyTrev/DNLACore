using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The PlaybackSelectionModel holds the remote devices and associated data obtained by the PlaybackSelectionController
	/// </summary>
	static class PlaybackSelectionModel
	{
		/// <summary>
		/// The remote devices that have been discovered
		/// </summary>
		public static Devices RemoteDevices { get; } = new Devices();

		/// <summary>
		/// The currently selected playback device
		/// This property is only set if the selected device name is available
		/// </summary>
		public static Device SelectedDevice { get; set; } = null;

		/// <summary>
		/// THe name of the currently selected device.
		/// At start up this device may not actually be available, so the SelectedDevice entry may not itself be set
		/// </summary>
		public static string SelectedDeviceName { get; set; } = "";
	}

	/// <summary>
	/// The Devices class contains a collection of unique Device entries.
	/// </summary>
	class Devices
	{
		/// <summary>
		/// Add a device to the collection if it is unique
		/// </summary>
		/// <param name="deviceToAdd"></param>
		/// <returns></returns>
		public bool AddDevice( Device deviceToAdd )
		{
			bool deviceUnique = true;

			int deviceIndex = 0;
			while ( ( deviceIndex < DeviceCollection.Count ) && ( deviceUnique == true ) )
			{
				Device deviceToCheck = DeviceCollection[ deviceIndex ];
				deviceUnique = ( ( deviceToCheck.IPAddress != deviceToAdd.IPAddress ) || ( deviceToCheck.DescriptionURL != deviceToAdd.DescriptionURL ) ||
					( deviceToCheck.Port != deviceToAdd.Port ) );
				deviceIndex++;
			}

			if ( deviceUnique == true )
			{
				DeviceCollection.Add( deviceToAdd );
			}

			return deviceUnique;
		}

		/// <summary>
		/// Return a list of all the device that can play back media
		/// </summary>
		/// <returns></returns>
		public List<string> ConnectedDevices()
		{
			return DeviceCollection.Select( device => device.FriendlyName ).ToList();
		}

		public Device FindDevice( string deviceName )
		{
			return DeviceCollection.SingleOrDefault( x => x.FriendlyName == deviceName );
		}

		/// <summary>
		/// The collection of devices
		/// </summary>
		public List<Device> DeviceCollection { get; set; } = new List<Device>();
	}

	/// <summary>
	/// The Device class represents a DNLA device
	/// </summary>
	public class Device
	{
		public string IPAddress { get; set; }
		public string DescriptionURL { get; set; }
		public int Port { get; set; }
		public bool CanPlayMedia { get; set; } = false;
		public string PlayUrl { get; set; }
		public string FriendlyName { get; set; }
		public bool IsLocal { get; set; } = false;
	}
}