namespace DBTest
{
	/// <summary>
	/// The MediaPlayingMessage class is used to notify whether or not the media is being played
	/// </summary>
	class MediaPlayingMessage : BaseMessage
	{
		/// <summary>
		/// The playback position
		/// </summary>
		public bool IsPlaying { get; set; }
	}
}