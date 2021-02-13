namespace DBTest
{
	/// <summary>
	/// The DisplayGenreMessage class is used to notify the new state of the DisplayGenre flag
	/// </summary>
	class DisplayGenreMessage : BaseMessage
	{
		/// <summary>
		/// The new state of the DisplayGenre flag
		/// </summary>
		public bool DisplayGenre { get; set; }
	}
}