﻿using System.Collections.Generic;
using System.Threading.Tasks;

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
				// Before clearing it reset the selected song index to stop the current song being played
				await PlaybackAccess.SetSelectedSongAsync( -1 );
				new SongSelectedMessage() { ItemNo = -1 }.Send();

				// Now clear the Now Playing list 
				await PlaylistAccess.ClearNowPlayingListAsync( libraryId );
			}

			// Carry out the common processing to add songs to a playlist
			await PlaylistAccess.AddSongsToNowPlayingListAsync( songsToAdd, libraryId );
			new NowPlayingSongsAddedMessage().Send();

			// If the list was cleared and there are now some items in the list select the first entry
			if ( ( clearFirst == true ) & ( songsToAdd.Count > 0 ) )
			{
				await PlaybackAccess.SetSelectedSongAsync( 0 );
				new SongSelectedMessage() { ItemNo = 0 }.Send();

				// Make sure the new song is played
				new PlayCurrentSongMessage().Send();
			}
		}

		/// <summary>
		/// Adjust the track numbers to match the indexes in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static async Task AdjustTrackNumbersAsync( Playlist thePlaylist )
		{
			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			for ( int index = 0; index < thePlaylist.PlaylistItems.Count; ++index )
			{
				PlaylistItem itemToCheck = thePlaylist.PlaylistItems[ index ];
				if ( itemToCheck.Track != ( index + 1 ) )
				{
					itemToCheck.Track = index + 1;

					// Update the item in the model
					await PlaylistAccess.UpdatePlaylistItemAsync( itemToCheck );
				}
			}
		}
	}
}