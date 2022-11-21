namespace CoreMP
{
	/// <summary>
	/// The ScanDevices class is used to discover UPnP devices that can be used as scan sources
	/// </summary>
	internal class ScanDevices : DeviceDiscovery.IDeviceDiscoveryChanges
	{
		/// <summary>
		/// Called to report the available devices - when registration is first made
		/// </summary>
		/// <param name="devices"></param>
		public void AvailableDevices( PlaybackDevices devices ) => devices.DeviceCollection.ForEach( device => NewDeviceDetected( device ) );

		/// <summary>
		/// Called when one or more devices are no longer available
		/// </summary>
		/// <param name="devices"></param>
		public void UnavailableDevices( PlaybackDevices devices ) => devices.DeviceCollection.ForEach( device => RemoteDevices.RemoveDevice( device ) );

		/// <summary>
		/// Called when the wifi network state changes
		/// </summary>
		/// <param name="state"></param>
		public void NetworkState( bool state ) { }

		/// <summary>
		/// Called when a new remote media device has been detected
		/// </summary>
		/// <param name="device"></param>
		public void NewDeviceDetected( PlaybackDevice device )
		{
			// Add this device to the model if it supports content discovery
			if ( device.ContentUrl.Length > 0 )
			{
				RemoteDevices.AddDevice( device );
			}
		}

		/// <summary>
		/// The remote devices that have been discovered
		/// </summary>
		public PlaybackDevices RemoteDevices { get; } = new PlaybackDevices();
	}
}
