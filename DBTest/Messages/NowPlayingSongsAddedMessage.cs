using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The NowPlayingSongsAddedMessage class is used to notify that songs have been added to the Now Playing playlist
	/// </summary>
	class NowPlayingSongsAddedMessage: BaseMessage
	{
		/// <summary>
		/// Indicate whether or not the playlist has been added to or replaced
		/// </summary>
		public bool SongsReplaced { get; set; } = false;
	}
}