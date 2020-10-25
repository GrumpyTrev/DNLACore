namespace DBTest
{
	static class AutoplayModel
	{
		/// <summary>
		/// The Autoplay record associated with the specified library ( could be null if no Autoplay record for the library )
		/// </summary>
		public static Autoplay CurrentAutoplay { get; set; } = null;

		/// <summary>
		/// The id of the library for which the autoplay has been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;
	}
}