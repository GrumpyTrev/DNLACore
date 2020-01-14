namespace DBTest
{
	/// <summary>
	/// The TagDeletedMessage class is used to notify that a Tag has been deleted
	/// </summary>
	class TagDeletedMessage: BaseMessage
	{
		/// <summary>
		/// The Tag that has been deleted
		/// </summary>
		public Tag DeletedTag { get; set; } = null;
	}
}