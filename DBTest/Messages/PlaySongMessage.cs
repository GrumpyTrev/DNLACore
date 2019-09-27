using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaySongMessage class is used to notify that sa song has been selected to play
	/// </summary>
	class PlaySongMessage: BaseMessage
	{
		/// <summary>
		/// The songs that have been added to the playlist
		/// </summary>
		public Song SongToPlay { get; set; } = null;

		public int TrackId { get; set; }
	}
}