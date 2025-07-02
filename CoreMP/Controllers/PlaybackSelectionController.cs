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
		public PlaybackSelectionController() =>	NotificationHandler.Register<StorageController>( () =>
		{
			// Initialise the locally held devices collection to hold the 'local' device and the currently available remote devices
			PlaybackSelectionModel.PlaybackCapableDevices = DevicesModel.RemoteDevices.PlaybackDeviceCollection.ToList();
			UpdateSelectedDevice();

			// Once data is available register for DevicesModel change messages
			NotificationHandler.Register<DevicesModel>( nameof(DevicesModel.PlaybackDeviceCollectionChanged ), ( sender ) =>
			{
				// Update the model with the DevicesModel playback devices
				PlaybackSelectionModel.PlaybackCapableDevices = DevicesModel.RemoteDevices.PlaybackDeviceCollection.ToList();

				NotifyCollectionChangedEventArgs args = ( NotifyCollectionChangedEventArgs )sender;
				if ( args.Action == NotifyCollectionChangedAction.Remove )
				{
					foreach ( object device in args.OldItems )
					{
						// If this device is currently selected then revert to the local device
						if ( DevicesModel.SelectedDevice == device )
						{
							DevicesModel.SelectedDevice = DevicesModel.RemoteDevices.LocalDevice;
						}
					}
				}
				else if ( args.Action == NotifyCollectionChangedAction.Reset )
				{
					// All of the devices have been removed. revert to the local device
					DevicesModel.SelectedDevice = DevicesModel.RemoteDevices.LocalDevice;
				}

				// Report that the model has changed
				PlaybackSelectionModel.Available.IsSet = true;
			} );

			NotificationHandler.Register<DevicesModel>( nameof( DevicesModel.SelectedDevice ), UpdateSelectedDevice );
		} );

		/// <summary>
		/// Called when the user has selected a new playback device
		/// Save the selection in the DevicesModel
		/// </summary>
		/// <param name="deviceName"></param>
		public void SetSelectedPlayback( string deviceName )
		{
			PlaybackDevice selectedDevice = PlaybackSelectionModel.PlaybackCapableDevices.SingleOrDefault( dev => dev.FriendlyName == deviceName );
			if ( selectedDevice != null )
			{
				// Save in storage
				DevicesModel.SelectedDevice = selectedDevice;
			}
		}

		/// <summary>
		/// Called at startup and when the selected device has changed
		/// </summary>
		private void UpdateSelectedDevice()
		{
			// Save in the PlaybackSelectionModel
			PlaybackSelectionModel.SelectedDeviceName = DevicesModel.SelectedDevice.FriendlyName;

			// Report that the PlaybackSelectionModel has changed
			PlaybackSelectionModel.Available.IsSet = true;
		}
	}
}
