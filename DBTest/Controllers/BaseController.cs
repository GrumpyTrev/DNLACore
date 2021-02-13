using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The BaseController is the Controller for common actions carried out by the Base View component
	/// Actions can only be carried out here if they do not require any model data to be accessed.
	/// </summary>
	class BaseController
	{
		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToNowPlayingList( IEnumerable<Song> songsToAdd, bool clearFirst )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				// Before clearing it reset the selected song index to stop the current song being played
				Playback.SongIndex = -1;

				// Now clear the Now Playing list 
				NowPlayingViewModel.NowPlayingPlaylist.Clear();
			}

			// Carry out the common processing to add songs to a playlist
			NowPlayingViewModel.NowPlayingPlaylist.AddSongs( songsToAdd );

			// If the list was cleared and there are now some items in the list select the first entry
			if ( ( clearFirst == true ) & ( songsToAdd.Count() > 0 ) )
			{
				Playback.SongIndex = 0;

				// Make sure the new song is played
				new PlayCurrentSongMessage().Send();
			}
		}
	}
}