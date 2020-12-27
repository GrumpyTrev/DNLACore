namespace DBTest
{
	/// <summary>
	/// The TagDetailsChangedMessage class is used to notify that the properties of a tag have been changed
	/// </summary>
	class TagDetailsChangedMessage: BaseMessage
	{
		/// <summary>
		/// The Tag that has been changed
		/// </summary>
		public Tag ChangedTag { get; set; } = null;
	}
}