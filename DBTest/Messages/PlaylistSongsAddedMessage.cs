using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistSongsAddedMessage class is used to notify that songs have been added to a playlist
	/// </summary>
	class PlaylistSongsAddedMessage: BaseMessage
	{
		/// <summary>
		/// The songs that have been added to the playlist
		/// </summary>
		public List<Song> Songs { get; set; } = null;

		/// <summary>
		/// The playlist that the songs have been added to
		/// </summary>
		public Playlist Playlist { get; set; } = null;
	}
}