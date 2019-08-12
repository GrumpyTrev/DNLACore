using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MobileApp
{
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
			return DeviceCollection.Where( device => device.CanPlayMedia == true ).Select( device => device.FriendlyName ).ToList();
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
}