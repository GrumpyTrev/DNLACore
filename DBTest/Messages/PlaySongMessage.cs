namespace DBTest
{
	/// <summary>
	/// The PlaySongMessage class is used to notify that the selected song should be played
	/// </summary>
	class PlaySongMessage : BaseMessage
	{
		/// <summary>
		/// The song to play
		/// </summary>
		public Song SongToPlay { get; set; } = null;

		/// <summary>
		/// Allow this message to be used to just set the song, without actually playing it
		/// </summary>
		public bool DontPlay { get; set; } = false;
	}
}