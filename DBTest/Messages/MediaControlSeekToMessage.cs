namespace DBTest
{
	/// <summary>
	/// The MediaControlSeekToMessage class is used to notify that the Media Control seek button has been pressed
	/// </summary>
	class MediaControlSeekToMessage : BaseMessage
	{
		/// <summary>
		/// The position to seek to.
		/// </summary>
		public int Position { get; set; }
	}
}