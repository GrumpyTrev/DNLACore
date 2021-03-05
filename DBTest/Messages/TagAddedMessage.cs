namespace DBTest
{
	/// <summary>
	/// The TagAddedMessage class is used to notify that a Tag has been deleted
	/// </summary>
	class TagAddedMessage : BaseMessage
	{
		/// <summary>
		/// The Tag that has been added
		/// </summary>
		public Tag AddedTag { get; set; } = null;
	}
}