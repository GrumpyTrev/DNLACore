namespace DBTest
{
	/// <summary>
	/// The SongFinishedMessage class is used to notify that a song has finished being played
	/// </summary>
	class SongFinishedMessage : BaseMessage
	{
		/// <summary>
		/// The song that has just finished being played
		/// </summary>
		public Song SongPlayed { get; set; } = null;
	}
}