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

		/// <summary>
		/// The previous name of the tag - just in case it has changed
		/// </summary>
		public string PreviousName { get; set; } = "";
	}
}