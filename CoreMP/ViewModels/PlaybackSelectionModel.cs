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
		/// The name of the currently selected device.
		/// </summary>
		public static string SelectedDeviceName { get; set; } = "";
	}
}
