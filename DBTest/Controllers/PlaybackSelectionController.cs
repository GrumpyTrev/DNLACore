namespace DBTest
{
	/// <summary>
	/// The PlaybackSelectionController is maintains details of the selected device and available devices in the PlaybackSelectionModel 
	/// </summary>
	public static class PlaybackSelectionController
	{
		/// <summary>
		/// Static constructor
		/// </summary>
		static PlaybackSelectionController()
		{
		}

		/// <summary>
		/// Get the Controller data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Called when the user has selected a new playback device
		/// Save it in the model and report it
		/// </summary>
		/// <param name="deviceName"></param>
		public static void SetSelectedPlayback( string deviceName )
		{
			PlaybackDevice selectedDevice = PlaybackSelectionModel.RemoteDevices.FindDevice( deviceName );
			if ( selectedDevice != null )
			{
				PlaybackSelectionModel.SelectedDevice = selectedDevice;
				PlaybackSelectionModel.SelectedDeviceName = selectedDevice.FriendlyName;
				Playback.PlaybackDeviceName = PlaybackSelectionModel.SelectedDeviceName;

				new PlaybackDeviceAvailableMessage() { SelectedDevice = PlaybackSelectionModel.SelectedDevice }.Send();

				// Report that the Playback Selection model have changed
				new PlaybackModelChangedMessage().Send();
			}
		}

		/// <summary>
		/// Called when the Playback details have been read in from storage
		/// </summary>
		/// <param name="message"></param>
		private static void StorageDataAvailable()
		{
			// Initialise the locally held devices collection to hold the 'local' device
			PlaybackSelectionModel.RemoteDevices.AddDevice( new PlaybackDevice() { CanPlayMedia = PlaybackDevice.CanPlayMediaType.Yes, IsLocal = true,
				FriendlyName = PlaybackSelectionModel.LocalDeviceName
			} );

			// If the selected device is the 'local' device then report that it is available
			ReportLocalSelectedDevice();

			// Register interest in the availability of remote devices
			MainApp.RegisterPlaybackCapabilityCallback( deviceCallback );
		}

		/// <summary>
		/// Get the selected device from the database and if its the local device report is as available
		/// </summary>
		private static void ReportLocalSelectedDevice()
		{
			PlaybackDevice localDevice = PlaybackSelectionModel.RemoteDevices.DeviceCollection[ 0 ];

			// Use the Playback class to retrieve the last selected device
			PlaybackSelectionModel.SelectedDeviceName = Playback.PlaybackDeviceName;

			if ( PlaybackSelectionModel.SelectedDeviceName.Length == 0 )
			{
				// No device selected. Select the local device
				PlaybackSelectionModel.SelectedDeviceName = localDevice.FriendlyName;
				Playback.PlaybackDeviceName = PlaybackSelectionModel.SelectedDeviceName;
			}

			// If the selected device is the local device then report it as available
			if ( PlaybackSelectionModel.SelectedDeviceName == localDevice.FriendlyName )
			{
				PlaybackSelectionModel.SelectedDevice = localDevice;
				new PlaybackDeviceAvailableMessage() { SelectedDevice = localDevice }.Send();
			}

			// Report that the Playback Selection model have changed
			new PlaybackModelChangedMessage().Send();
		}

		/// <summary>
		/// Called when a new remote media device has been detected
		/// </summary>
		/// <param name="device"></param>
		private static void NewDeviceDetected( PlaybackDevice device )
		{
			// Add this device to the model
			if ( PlaybackSelectionModel.RemoteDevices.AddDevice( device ) == true )
			{
				// If this device is the currently selected device then report it as available
				if ( device.FriendlyName == PlaybackSelectionModel.SelectedDeviceName )
				{
					PlaybackSelectionModel.SelectedDevice = device;
					new PlaybackDeviceAvailableMessage() { SelectedDevice = device }.Send();
				}

				// Report that the Playback Selection model have changed
				new PlaybackModelChangedMessage().Send();
			}
		}

		/// <summary>
		/// Called when a previously available device is no longer available
		/// </summary>
		/// <param name="device"></param>
		private static void DeviceNotAvailable( PlaybackDevice device )
		{
			// Remove this device from the model
			if ( PlaybackSelectionModel.RemoteDevices.RemoveDevice( device ) == true )
			{
				// If this device is currently selected then report that there is no selected device
				if ( PlaybackSelectionModel.SelectedDevice == device )
				{
					PlaybackSelectionModel.SelectedDevice = null;
					new PlaybackDeviceAvailableMessage() { SelectedDevice = null }.Send();
				}

				// Report that the Playback Selection model have changed
				new PlaybackModelChangedMessage().Send();
			}
		}

		/// <summary>
		/// Called when the wifi network state changes
		/// </summary>
		/// <param name="state"></param>
		private static void NetworkState( bool state )
		{
			PlaybackSelectionModel.WifiAvailable = state;

			// Report that the Playback Selection model have changed
			new PlaybackModelChangedMessage().Send();
		}

		/// <summary>
		/// Implementation of the PlaybackCapabilities.IPlaybackCapabilitiesChanges interface
		/// </summary>
		private class RemoteDeviceCallback : PlaybackCapabilities.IPlaybackCapabilitiesChanges
		{
			/// <summary>
			/// Called to report the available devices - when registration is first made
			/// </summary>
			/// <param name="devices"></param>
			public void AvailableDevices( PlaybackDevices devices )
			{
				devices.DeviceCollection.ForEach( device => PlaybackSelectionController.NewDeviceDetected( device ) );
			}

			/// <summary>
			/// Called when one or more devices are no longer available
			/// </summary>
			/// <param name="devices"></param>
			public void UnavailableDevices( PlaybackDevices devices )
			{
				devices.DeviceCollection.ForEach( device => PlaybackSelectionController.DeviceNotAvailable( device ) );
			}

			/// <summary>
			/// Called when the wifi network state changes
			/// </summary>
			/// <param name="state"></param>
			public void NetworkState( bool state )
			{
				PlaybackSelectionController.NetworkState( state );
			}

			/// <summary>
			/// Called when a new DLNA device has been detected
			/// </summary>
			/// <param name="device"></param>
			public void NewDeviceDetected( PlaybackDevice device )
			{
				PlaybackSelectionController.NewDeviceDetected( device );
			}
		}

		/// <summary>
		/// The single instance of the RemoteDeviceCallback class
		/// </summary>
		private static readonly RemoteDeviceCallback deviceCallback = new RemoteDeviceCallback();

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}