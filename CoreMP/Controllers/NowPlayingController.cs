using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The NowPlayingController is the Controller for the NowPlayingView. It responds to NowPlayingView commands and maintains Now Playing data in the
	/// NowPlayingViewModel
	/// </summary>
	internal class NowPlayingController
	{
		/// <summary>
		/// Register for all data to be loaded.
		/// </summary>
		public NowPlayingController() => NotificationHandler.Register( typeof( StorageController ), () =>
		{
			// Initialise the model
			StorageDataAvailable();

			// Register for selected library changes
			SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );

			// Register for shuffle mode changes
			ShuffleModeChangedMessage.Register( ShuffleModeChanged );

			// Register for the SongFinished message
			SongFinishedMessage.Register( SongFinished );

			// When the current song index changes update the model
			NotificationHandler.Register( typeof( Playlists ), "CurrentSongIndex",
				() => NowPlayingViewModel.CurrentSongIndex = Playlists.CurrentSongIndex );

			// When the song is started or stopped update the model
			NotificationHandler.Register( typeof( PlaybackModel ), "IsPlaying",
				() => NowPlayingViewModel.IsPlaying = PlaybackModel.IsPlaying );
		} );

		/// <summary>
		/// Set the selected song in the database
		/// </summary>
		public void UserSongSelected( int songIndex ) => Playlists.CurrentSongIndex = songIndex;

		/// <summary>
		/// Add the songs from the playlist to the Now Playing list
		/// If this set is replacing the current contents (clearFirst == true ) then clear the Now Playing list
		/// first.
		/// If a resume has been selected and the current contents are being replaced then add all of the
		/// playlist's songs but set the current song to the resume point. If resume has been selected and the playlist is 
		/// being added to then only add the playlist songs from its resume point. Don't change the resume point in the source playlist.
		/// If resume has not been selected then add all the playlist's contents and reset the playlist's restore point
		/// </summary>
		/// <param name="playlistToAdd"></param>
		/// <param name="clearFirst"></param>
		/// <param name="resume"></param>
		public void AddPlaylistToNowPlayingList( Playlist playlistToAdd, bool clearFirst, bool resume )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				ClearNowPlayingList();
			}

			// Assume we're going to play a new list from the start
			int newCurrentIndex = 0;

			if ( resume == true )
			{
				if ( clearFirst == true )
				{
					nowPlayingPlaylist.AddSongs( playlistToAdd.GetSongsForPlayback( false ) );
					newCurrentIndex = playlistToAdd.InProgressIndex;
				}
				else
				{
					nowPlayingPlaylist.AddSongs( playlistToAdd.GetSongsForPlayback( true ) );
				}
			}
			else
			{
				nowPlayingPlaylist.AddSongs( ApplyShuffle( playlistToAdd.GetSongsForPlayback( false ) ) );
				playlistToAdd.SongIndex = 0;
			}

			// If the Now Playing list was cleared then play the specified song
			SetStartingPointForNewList( clearFirst, newCurrentIndex );

			// Report change to UI
			NowPlayingViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public void AddSongsToNowPlayingList( IEnumerable<Song> songsToAdd, bool clearFirst )
		{
			// Should the Now Playing playlist be cleared first
			if ( clearFirst == true )
			{
				ClearNowPlayingList();
			}

			// Add the songs to the playlist. Shuffled if necessary
			nowPlayingPlaylist.AddSongs( ApplyShuffle( songsToAdd.ToList() ) );

			// If the Now Playing list was cleared then play the first song
			SetStartingPointForNewList( clearFirst, 0 );

			// Report change to UI
			NowPlayingViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Called to delete one or more items from the Now Playing playlist.
		/// We need to determine the affect that this deletion may have on the index of the currently playing song.
		/// This can be done by examining the track numbers of the items to be deleted
		/// </summary>
		/// <param name="items"></param>
		public void DeleteNowPlayingItems( IEnumerable<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the delete
			PlaylistItem currentPlaylistItem = null;

			// If the currently selected song is going to be deleted then invalidate it now
			// Only carry out these checks if a song has been selected
			if ( Playlists.CurrentSongIndex != -1 )
			{
				// Check if the items to delete contains the currently selected song
				if ( items.Any( item => ( item.Index == Playlists.CurrentSongIndex ) ) == true )
				{
					// The currently selected song is going to be deleted. Set it to invalid
					Playlists.CurrentSongIndex = -1;
				}
				else
				{
					// Save the current item
					currentPlaylistItem = nowPlayingPlaylist.PlaylistItems[ Playlists.CurrentSongIndex ];
				}
			}

			// Delete the entries and report that the list has been updated
			nowPlayingPlaylist.DeletePlaylistItems( items.ToList() );

			// Adjust the track numbers
			nowPlayingPlaylist.AdjustTrackNumbers();

			// Determine the index of the currently selected song from it's possibly new track number
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report change to UI
			NowPlayingViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Move a set of selected items down the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsDown( IEnumerable<PlaylistItem> items ) => MoveItems( items, false );

		/// <summary>
		/// Move a set of selected items up the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsUp( IEnumerable<PlaylistItem> items ) => MoveItems( items, true );

		/// <summary>
		/// Move the selected items up or down the list
		/// </summary>
		/// <param name="items"></param>
		/// <param name="moveUp"></param>
		private void MoveItems( IEnumerable<PlaylistItem> items, bool moveUp )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( Playlists.CurrentSongIndex == -1 ) ? null : nowPlayingPlaylist.PlaylistItems[ Playlists.CurrentSongIndex ];

			if ( moveUp == true )
			{
				nowPlayingPlaylist.MoveItemsUp( items );
			}
			else
			{
				nowPlayingPlaylist.MoveItemsDown( items );
			}

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			NowPlayingViewModel.PlaylistUpdated = true;
		}

		/// <summary>
		/// Called when the SongPlaylist data is available to be displayed, or needs to be refreshed
		/// </summary>
		private void StorageDataAvailable()
		{
			// Get the NowPlaying playlist. Save it in the model and locally in the controller
			nowPlayingPlaylist = ( SongPlaylist )Playlists.GetNowPlayingPlaylist();
			NowPlayingViewModel.NowPlayingPlaylist = nowPlayingPlaylist;

			// Initialise the model with the index of the currently selected song
			NowPlayingViewModel.CurrentSongIndex = Playlists.CurrentSongIndex;

			NowPlayingViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Reload the data
		/// </summary>
		/// <param name="message"></param>
		private void SelectedLibraryChanged( int _ ) => StorageDataAvailable();

		/// <summary>
		/// Called when a ShuffleModeChangedMessage is received.
		/// Get the new mode and update the model.
		/// If Shuffle mode has been turned on then shuffle the current playlist
		/// </summary>
		/// <param name="message"></param>
		private void ShuffleModeChanged()
		{
			if ( Playback.ShuffleOn == true )
			{
				ShufflePlaylistItems();
			}
		}

		/// <summary>
		/// Select the next song in the playlist
		/// </summary>
		public void MediaControlPlayNext() => Playlists.CurrentSongIndex = 
			( Playlists.CurrentSongIndex == nowPlayingPlaylist.PlaylistItems.Count - 1 ) ? 0 : Playlists.CurrentSongIndex + 1;

		/// <summary>
		/// Select the previous song in the playlist
		/// </summary>
		/// <param name="_message"></param>
		public void MediaControlPlayPrevious() => Playlists.CurrentSongIndex = 
			( Playlists.CurrentSongIndex > 0 ) ? Playlists.CurrentSongIndex - 1 : nowPlayingPlaylist.PlaylistItems.Count - 1;

		/// <summary>
		/// Called when a SongFinishedMessage has been received.
		/// Play the next song. If the end of the playlist has been reached and repeat is on then go back to the first song.
		/// If shuffle mode is on then shuffle the songs before playimng the first one
		/// </summary>
		/// <param name="_"></param>
		private void SongFinished( Song _ )
		{
			if ( Playlists.CurrentSongIndex < ( nowPlayingPlaylist.PlaylistItems.Count - 1 ) )
			{
				// Play the next song
				Playlists.CurrentSongIndex++;
			}
			else if ( ( Playback.RepeatOn == true ) && ( nowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				// If shuffle mode is on then shuffle the items before playing them
				if ( Playback.ShuffleOn == true )
				{
					ShufflePlaylistItems();
				}

				Playlists.CurrentSongIndex = 0;
			}
			else
			{
				Playlists.CurrentSongIndex = -1;
			}
		}

		/// <summary>
		/// Shuffle the current playlist items
		/// Extract the songs from the playlist and add them back to it. The AddSongsToNowPlayingList method will appply the shuffle
		/// </summary>
		private void ShufflePlaylistItems() => AddSongsToNowPlayingList( ( ( SongPlaylist )Playlists.GetNowPlayingPlaylist() ).GetSongs(), true );

		/// <summary>
		/// Shuffle the list of specified songs if Shuffle is on
		/// </summary>
		/// <param name="songs"></param>
		private List<Song> ApplyShuffle( List<Song> songs )
		{
			if ( Playback.ShuffleOn == true )
			{
				int n = songs.Count;
				while ( n > 1 )
				{
					n--;
					int k = rng.Next( n + 1 );
					(songs[ n ], songs[ k ]) = (songs[ k ], songs[ n ]);
				}
			}

			return songs;
		}

		/// <summary>
		/// Adjust the index of the currently selected song if it does not match the adjusted track number of the song
		/// </summary>
		/// <param name="selectedSong"></param>
		private void AdjustSelectedSongIndex( PlaylistItem selectedSong )
		{
			if ( selectedSong != null )
			{
				int newSelectedIndex = selectedSong.Index;

				if ( newSelectedIndex != Playlists.CurrentSongIndex )
				{
					Playlists.CurrentSongIndex = newSelectedIndex;
				}
			}
		}

		/// <summary>
		/// Clear the Now Playing list - stopping any current song playing
		/// </summary>
		private void ClearNowPlayingList()
		{
			// Before clearing the list reset the selected song to stop it being played
			Playlists.CurrentSongIndex = -1;

			// Now clear the Now Playing list 
			nowPlayingPlaylist.Clear();
		}

		/// <summary>
		/// If this is a newly cleared list then set the index of the song to play
		/// </summary>
		/// <param name="isNew"></param>
		/// <param name="startingIndex"></param>
		private void SetStartingPointForNewList( bool isNew, int startingIndex )
		{
			// If the list was cleared and there are now some items in the list select the song to play and play it
			if ( ( isNew == true ) & ( nowPlayingPlaylist.PlaylistItems.Count > startingIndex ) )
			{
				Playlists.CurrentSongIndex = startingIndex;
			}
		}

		/// <summary>
		/// The random number generator used to shuffle the list
		/// </summary>
		private readonly Random rng = new Random();

		/// <summary>
		/// The curently selected NowPlaying playlist. 
		/// </summary>
		private SongPlaylist nowPlayingPlaylist = null;
	}
}
