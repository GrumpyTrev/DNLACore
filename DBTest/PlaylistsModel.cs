using System.Collections.Generic;

namespace DBTest
{
	class PlaylistsModel : ViewModel
	{
		public List<Artist> Artists { get; set; }
		public Dictionary<string, int> AlphaIndex { get; set; }
	}
}