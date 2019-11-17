using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The BaseController is the Controller for common actions carried out by the Base View component
	/// Actions can only be carried out here if they do not require any model data to be accessed.
	/// </summary>
	static class BaseController
	{
		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToNowPlayingList( List<Song> songsToAdd, bool clearFirst, int libraryId )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				// Before clearing it reset the selected song index
				PlaybackAccess.SetSelectedSong( -1 );
				new SongSelectedMessage() { ItemNo = -1 }.Send();

				// Now clear it
				PlaylistAccess.ClearNowPlayingList( libraryId );
				new NowPlayingClearedMessage().Send();
			}

			// Carry out the common processing to add songs to a playlist
			PlaylistAccess.AddSongsToNowPlayingList( songsToAdd, libraryId );
			new NowPlayingSongsAddedMessage() { SongsReplaced = clearFirst }.Send();
		}
	}
}