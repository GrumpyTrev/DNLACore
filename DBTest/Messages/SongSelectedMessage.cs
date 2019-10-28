using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The SongSelectedMessage class is used to notify that a song has been selected
	/// </summary>
	class SongSelectedMessage: BaseMessage
	{
		/// <summary>
		/// Index of the song in the Now Playing list
		/// </summary>
		public int ItemNo { get; set; } = -1;
	}
}