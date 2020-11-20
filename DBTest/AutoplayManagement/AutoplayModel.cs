using System.Collections.Generic;

namespace DBTest
{
	static class AutoplayModel
	{
		public static void ClearModel()
		{
			Populations.Clear();
			GenresAlreadyIncluded.Clear();
			AlbumsAlreadyIncluded.Clear();
		}

		/// <summary>
		/// The Autoplay record associated with the specified library ( could be null if no Autoplay record for the library )
		/// </summary>
		public static Autoplay CurrentAutoplay { get; set; } = null;

		/// <summary>
		/// The id of the library for which the autoplay has been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		public static List<Population> Populations = new List<Population>();

		public static HashSet<string> GenresAlreadyIncluded { get; set; } = new HashSet<string>();

		public static HashSet<int> AlbumsAlreadyIncluded { get; set; } = new HashSet<int>();

		public class Population
		{
			public List<string> Genres { get; set; } = new List<string>();
			public List<Album> Albums { get; set; } = new List<Album>();
		}
	}
}