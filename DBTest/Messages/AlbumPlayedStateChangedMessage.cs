namespace DBTest
{
	/// <summary>
	/// The AlbumPlayedStateChangedMessage class is used to notify that the album's played state has changed
	/// </summary>
	class AlbumPlayedStateChangedMessage : BaseMessage
	{
		/// <summary>
		/// The changed album
		/// </summary>
		public Album AlbumChanged { get; set; } = null;
	}
}