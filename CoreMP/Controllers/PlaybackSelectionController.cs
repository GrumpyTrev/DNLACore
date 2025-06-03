using System.Collections.Specialized;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackSelectionController maintains details of the selected device and available devices in the PlaybackSelectionModel 
	/// </summary>
	internal class PlaybackSelectionController
	{
		/// <summary>
		/// Constructor
		/// Register for the main data available event.
		/// </summary>
		public PlaybackSelectionController() =>	NotificationHandler.Register( typeof( StorageController ), () =>
		{
			StorageDataAvailable();

			// Once data is available register for DevicesModel change messages
			NotificationHandler.Register( typeof( DevicesModel ), "WifiAvailable", ( sender, propertyName ) =>
			{
				PlaybackSelectionModel.WifiAvailable = DevicesModel.WifiAvailable;

				// Report that the Playback Selection model has changed
				PlaybackSelectionModel.Available.IsSet = true;
			} );

			NotificationHandler.Register( typeof( DevicesModel ), "PlaybackDeviceCollectionChanged", ( sender, propertyName ) =>
			{
				// Update the model with the DevicesModel playback devices
				PlaybackSelectionModel.PlaybackCapableDevices = DevicesModel.RemoteDevices.PlaybackDeviceCollection.ToList();

				NotifyCollectionChangedEventArgs args = ( NotifyCollectionChangedEventArgs )sender;
				if ( args.Action == NotifyCollectionChangedAction.Add )
				{
					foreach ( object device in args.NewItems )
					{
						NewDeviceDetected( ( PlaybackDevice )device );
					}
				}
				else if ( args.Action == NotifyCollectionChangedAction.Remove )
				{
					foreach ( object device in args.OldItems )
					{
						DeviceNotAvailable( ( PlaybackDevice )device );
					}
				}
			} );
		} );

		/// <summary>
		/// Called when the user has selected a new playback device
		/// Save it in the model and report it
		/// </summary>
		/// <param name="deviceName"></param>
		public void SetSelectedPlayback( string deviceName )
		{
			PlaybackDevice selectedDevice = PlaybackSelectionModel.PlaybackCapableDevices.SingleOrDefault( dev => dev.FriendlyName == deviceName );
			if ( selectedDevice != null )
			{
				// Save in storage
				PlaybackSelectionModel.SelectedDeviceName = selectedDevice.FriendlyName;
				PlaybackSelectionModel.SelectedDevice = selectedDevice;

				// Report that the Playback Selection model has changed
				PlaybackSelectionModel.Available.IsSet = true;
			}
		}

		/// <summary>
		/// Called when the Playback details have been read in from storage
		/// </summary>
		/// <param name="message"></param>
		private void StorageDataAvailable()
		{
			// Initialise the locally held devices collection to hold the 'local' device and the currently available remote devices
			PlaybackSelectionModel.PlaybackCapableDevices = DevicesModel.RemoteDevices.PlaybackDeviceCollection.ToList();

			// Select the local device
			PlaybackSelectionModel.SelectedDeviceName = PlaybackDevices.LocalDeviceName;
			PlaybackSelectionModel.SelectedDevice = PlaybackSelectionModel.PlaybackCapableDevices.Single( dev => dev.FriendlyName == PlaybackSelectionModel.SelectedDeviceName );

			// Report that the Playback Selection modelis available
			PlaybackSelectionModel.Available.IsSet = true;
		}

		/// <summary>
		/// Called when a new remote media device has been detected
		/// </summary>
		/// <param name="device"></param>
		private void NewDeviceDetected( PlaybackDevice device )
		{
			// If this device is the currently selected device then report it as available
			if ( device.FriendlyName == PlaybackSelectionModel.SelectedDeviceName )
			{
				PlaybackSelectionModel.SelectedDevice = device;
			}

			// Report that the Playback Selection model has changed
			PlaybackSelectionModel.Available.IsSet = true;
		}

		/// <summary>
		/// Called when a previously available device is no longer available
		/// </summary>
		/// <param name="device"></param>
		private void DeviceNotAvailable( PlaybackDevice device )
		{
			// If this device is currently selected then report that there is no selected device
			if ( PlaybackSelectionModel.SelectedDevice == device )
			{
				PlaybackSelectionModel.SelectedDevice = null;
			}

			// Report that the Playback Selection model has changed
			PlaybackSelectionModel.Available.IsSet = true;
		}
	}
}
