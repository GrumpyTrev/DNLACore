namespace DBTest
{
	/// <summary>
	/// The MediaProgressMessage class is used to notify the progress of the media being played
	/// </summary>
	class MediaProgressMessage : BaseMessage
	{
		/// <summary>
		/// The playback position
		/// </summary>
		public int CurrentPosition { get; set; }

		/// <summary>
		/// The reported duration of the song
		/// </summary>
		public int Duration { get; set; }
	}
}