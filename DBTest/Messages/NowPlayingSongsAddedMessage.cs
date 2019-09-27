using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The NowPlayingSongsAddedMessage class is used to notify that songs have been added to the Now Playing playlist
	/// </summary>
	class NowPlayingSongsAddedMessage: BaseMessage
	{
		/// <summary>
		/// The songs that have been added to the playlist
		/// </summary>
		public List<Song> Songs { get; set; } = null;
	}
}