using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	static class NowPlayingViewModel
	{
		/// <summary>
		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static Playlist NowPlayingPlaylist { get; set; } = null;
	}
}