using System.Collections.Generic;

namespace CoreMP
{
	public static class AutoplayModel
	{
		public static void ClearModel()
		{
		}

		/// <summary>
		/// The Autoplay record associated with the specified library
		/// </summary>
		public static Autoplay CurrentAutoplay { get; set; } = null;

		/// <summary>
		/// The id of the library for which the autoplay has been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;
	}
}
