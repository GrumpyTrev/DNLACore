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
		public static async void AddSongsToNowPlayingListAsync( List<Song> songsToAdd, bool clearFirst, int libraryId )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				// Before clearing it reset the selected song index
				await PlaybackAccess.SetSelectedSongAsync( -1 );
				new SongSelectedMessage() { ItemNo = -1 }.Send();

				// Now clear it
				await PlaylistAccess.ClearNowPlayingListAsync( libraryId );
				new NowPlayingClearedMessage().Send();
			}

			// Carry out the common processing to add songs to a playlist
			await PlaylistAccess.AddSongsToNowPlayingListAsync( songsToAdd, libraryId );
			new NowPlayingSongsAddedMessage() { SongsReplaced = clearFirst }.Send();

			// If the list was cleared and there are now some items in the list select the first entry
			if ( ( clearFirst == true ) & ( songsToAdd.Count > 0 ) )
			{
				await PlaybackAccess.SetSelectedSongAsync( 0 );
			}
		}
	}
}