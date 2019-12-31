namespace DBTest
{
	/// <summary>
	/// The AlbumAddedMessage class is used to notify that an album has been added to a library
	/// </summary>
	class AlbumAddedMessage: BaseMessage
	{
		/// <summary>
		/// The added album
		/// </summary>
		public Album AlbumAdded { get; set; } = null;
	}
}