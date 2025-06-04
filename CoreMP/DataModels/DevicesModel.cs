using System.Collections.Specialized;

namespace CoreMP
{
	/// <summary>
	/// The DevicesModel class contains a collection of all the remote (DLNA based) devices that can either be used for playback
	/// or as a media server
	/// </summary>
	internal static class DevicesModel
	{
		static DevicesModel()
		{
			// Register for changes to both the server and renderer collections
			RemoteDevices.BrowseableDeviceCollection.CollectionChanged += BrowseableDeviceCollectionChanged;
			RemoteDevices.PlaybackDeviceCollection.CollectionChanged += PlaybackDeviceCollectionChanged;
		}

		/// <summary>
		/// Pass on changes to the playback devices
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void PlaybackDeviceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) => NotificationHandler.NotifyPropertyChanged( e );

		/// <summary>
		/// Pass on changes to the server devices
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void BrowseableDeviceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) => NotificationHandler.NotifyPropertyChanged( e );

		/// <summary>
		/// The DLNA devices
		/// </summary>
		public static PlaybackDevices RemoteDevices { get; } = new PlaybackDevices();

		/// <summary>
		/// Is the wifi network available
		/// </summary>
		public static bool WifiAvailable { get; set; }

		/// <summary>
		/// The currently selected playback device. Initialised to the local device
		/// </summary>
		private static PlaybackDevice selectedDevice = RemoteDevices.LocalDevice;
		public static PlaybackDevice SelectedDevice
		{
			get => selectedDevice;
			set
			{
				selectedDevice = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}
	}
}
