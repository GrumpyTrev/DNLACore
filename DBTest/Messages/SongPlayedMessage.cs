namespace DBTest
{
	/// <summary>
	/// The SongPlayedMessage class is used to notify that a song is being played
	/// </summary>
	class SongPlayedMessage: BaseMessage
	{
		/// <summary>
		/// Index of the song in the Now Playing list
		/// </summary>
		public Song SongPlayed { get; set; } = null;
	}
}