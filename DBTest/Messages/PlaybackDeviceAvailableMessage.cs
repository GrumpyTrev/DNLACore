namespace DBTest
{
	/// <summary>
	/// The PlaybackDeviceAvailableMessage is used to report that the selected playback device is available
	/// </summary>
	class PlaybackDeviceAvailableMessage : BaseMessage
	{
		public Device SelectedDevice { get; set; }
	}
}