namespace CoreMP
{
	/// <summary>
	/// The PlaybackSelectionModel holds the remote devices and associated data obtained by the PlaybackSelectionController
	/// </summary>
	public static class PlaybackSelectionModel
	{
		/// <summary>
		/// The remote devices that have been discovered
		/// </summary>
		public static PlaybackDevices RemoteDevices { get; } = new PlaybackDevices();

		/// <summary>
		/// The currently selected playback device
		/// This property is only set if the selected device name is available
		/// </summary>
		public static PlaybackDevice SelectedDevice { get; set; } = null;

		/// <summary>
		/// THe name of the currently selected device.
		/// At start up this device may not actually be available, so the SelectedDevice entry may not itself be set
		/// </summary>
		public static string SelectedDeviceName { get; set; } = "";

		/// <summary>
		/// The state of the wi-fi network 
		/// </summary>
		public static bool WifiAvailable { get; set; } = false;

		/// <summary>
		/// The name of the one and only local device
		/// </summary>
		public const string LocalDeviceName = "Local playback";
	}
}
