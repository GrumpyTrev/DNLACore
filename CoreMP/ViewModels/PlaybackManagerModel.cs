using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackManagerModel holds the song data and playback modes obtained from the PlaybackManagementController
	/// </summary>
	public static class PlaybackManagerModel
	{
		/// <summary>
		/// The current song being played
		/// </summary>
		public static Song CurrentSong { get; set; } = null;

		/// <summary>
		/// The sources associated with the library
		/// </summary>
		public static List<Source> Sources { get; set; } = null;

		/// <summary>
		/// The details of the selected available playback device
		/// </summary>
		public static PlaybackDevice AvailableDevice { get; set; } = null;
	}
}
