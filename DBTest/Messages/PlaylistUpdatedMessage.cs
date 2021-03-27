namespace DBTest
{
	/// <summary>
	/// The PlaylistUpdatedMessage class is used to notify that a playlist has changed in some way
	/// </summary>
	class PlaylistUpdatedMessage : BaseMessage
	{
		/// <summary>
		/// The song being played
		/// </summary>
		public Playlist UpdatedPlaylist { get; set; } = null;
	}
}