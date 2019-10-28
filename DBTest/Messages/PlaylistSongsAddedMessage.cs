using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistSongsAddedMessage class is used to notify that songs have been added to a playlist
	/// </summary>
	class PlaylistSongsAddedMessage: BaseMessage
	{
		/// <summary>
		/// The playlist that the songs have been added to
		/// </summary>
		public string PlaylistName { get; set; } = "";
	}
}