using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The ArtistsViewModel holds the Artist data obtained from the ArtistsController
	/// </summary>
	static class ArtistsViewModel
	{
		/// <summary>
		/// The list of artists that has been obtained from the database
		/// </summary>
		public static List<Artist> Artists { get; set; } = null;

		/// <summary>
		/// Index into the list of Artists
		/// </summary>
		public static Dictionary<string, int> AlphaIndex { get; set; } = null;
	}
}