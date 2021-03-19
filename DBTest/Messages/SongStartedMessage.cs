namespace DBTest
{
	/// <summary>
	/// The SongStartedMessage class is used to notify that a song is being played
	/// </summary>
	class SongStartedMessage: BaseMessage
	{
		/// <summary>
		/// The song being played
		/// </summary>
		public Song SongPlayed { get; set; } = null;
	}
}