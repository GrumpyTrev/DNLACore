using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackSelectionModel holds the remote devices and associated data obtained by the PlaybackSelectionController
	/// </summary>
	public static class PlaybackSelectionModel
	{
		/// <summary>
		/// Used to notify changes to the model
		/// </summary>
		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// The remote devices capable of playback that have been have been discovered
		/// </summary>
		public static List<PlaybackDevice> PlaybackCapableDevices { get; set; } = new List<PlaybackDevice>();

		/// <summary>
		/// The currently selected playback device
		/// This property is only set if the selected device name is available
		/// </summary>
		private static PlaybackDevice selectedDevice = null;
		public static PlaybackDevice SelectedDevice
		{
			get => selectedDevice;
			set
			{
				selectedDevice = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// THe name of the currently selected device.
		/// At start up this device may not actually be available, so the SelectedDevice entry may not itself be set
		/// </summary>
		public static string SelectedDeviceName { get; set; } = "";

		/// <summary>
		/// The state of the wi-fi network 
		/// </summary>
		public static bool WifiAvailable { get; set; } = false;
	}
}
