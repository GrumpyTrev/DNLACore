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
		public NowPlayingController() => NotificationHandler.Register<StorageController>( nameof( StorageController.IsSet ), () =>
		{
			// Initialise the model
			StorageDataAvailable();

			// Register for selected library changes
			NotificationHandler.Register<Playback>( nameof( Playback.LibraryIdentity ), StorageDataAvailable );

			// Register for shuffle mode changes
			NotificationHandler.Register<Playback>( nameof( Playback.ShuffleOn ), () => InitialiseShuffleIfOn() );

			// Register interest in when the current song finished
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.SongStarted ), ( started ) =>
			{
				if ( ( bool )started == false )
				{
					SongFinished();
				}
			} );

			// When the current song index changes update the model
			NotificationHandler.Register<Playlists>( nameof( Playlists.CurrentSongIndex ),
				() => NowPlayingViewModel.CurrentSongIndex = Playlists.CurrentSongIndex );

			// When the song is started or stopped update the model
			NotificationHandler.Register<PlaybackModel>( nameof( PlaybackModel.IsPlaying ),
				() => NowPlayingViewModel.IsPlaying = PlaybackModel.IsPlaying );
		} );

		/// <summary>
		/// Set the selected song in the database
		/// </summary>
		public void UserSongSelected( int songIndex )
		{
			Playlists.CurrentSongIndex = songIndex;

			// Re-initialise the shuffle lists
			InitialiseShuffleIfOn();
		}

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

				// Add all the entries from the playlistToAdd
				nowPlayingPlaylist.AddSongs( playlistToAdd.GetSongsForPlayback( false ) );

				// If shuffle mode is off then the song to play depends on whether or not resume was selected
				if ( Playback.ShuffleOn == false )
				{
					Playlists.CurrentSongIndex = ( resume == true ) ? playlistToAdd.InProgressIndex : 0;
				}
				else
				{
					// Initialise shuffle mode
					InitialiseShuffleIfOn( false );

					// If this playlist is being resumed then remove all played items from the songsLeftToShuffle list
					if ( resume == true )
					{
						songsLeftToShuffle.RemoveRange( 0, playlistToAdd.InProgressIndex );
					}

					Playlists.CurrentSongIndex = SelectShuffleItem();
				}
			}
			else
			{
				// Items from the playlist are being added to the existing set
				List<Song> songsToAdd = playlistToAdd.GetSongsForPlayback( resume );
				nowPlayingPlaylist.AddSongs( songsToAdd );

				// If shuffle mode is on then copy the new items to the songsLeftToShuffle list
				if ( Playback.ShuffleOn == true )
				{
					songsLeftToShuffle.AddRange( nowPlayingPlaylist.PlaylistItems.TakeLast( songsToAdd.Count ).Cast<SongPlaylistItem>() );
				}
			}

			// If not resuming a playlist then reset it's song index
			if ( resume == false )
			{
				playlistToAdd.SongIndex = 0;
			}

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
				// Clear it
				ClearNowPlayingList();

				// Add the songs to the playlist.
				nowPlayingPlaylist.AddSongs( songsToAdd );

				// If shuffle mode is on then re-initialise it and select an item to play
				if ( Playback.ShuffleOn == true )
				{
					InitialiseShuffleIfOn( false );
					Playlists.CurrentSongIndex = SelectShuffleItem();
				}
				else
				{
					// Just play the first item
					Playlists.CurrentSongIndex = 0;
				}
			}
			else
			{
				// Add the songs to the playlist.
				nowPlayingPlaylist.AddSongs( songsToAdd );

				// If shuffle mode is on then add the new items to the songsLeftToShuffle list
				if ( Playback.ShuffleOn == true )
				{
					songsLeftToShuffle.AddRange( nowPlayingPlaylist.PlaylistItems.TakeLast( songsToAdd.Count() ).Cast<SongPlaylistItem>() );
				}
			}

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

			// Delete the entries.
			nowPlayingPlaylist.DeletePlaylistItems( items.ToList() );

			// Delete these entries from the songsLeftToShuffle and shuffledSongs lists
			if ( Playback.ShuffleOn == true )
			{
				songsLeftToShuffle.Where( shuffleSong => items.Any( item => ( item.Index == shuffleSong.Index ) ) ).ToList()
					.ForEach( item => songsLeftToShuffle.Remove( item ) );

				shuffledSongs.Where( shuffleSong => items.Any( item => ( item.Index == shuffleSong.Index ) ) ).ToList()
					.ForEach( item => shuffledSongs.Remove( item ) );
			}

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

			// If shuffle mode is on then initialise it
			InitialiseShuffleIfOn();

			NowPlayingViewModel.Available.IsSet = true;
		}

		/// <summary>
		/// Select the next song in the playlist
		/// </summary>
		public void MediaControlPlayNext()
		{
			// If shuffle mode is off then select the next song, cycling back to the start when the end if reached
			if ( Playback.ShuffleOn == false )
			{
				Playlists.CurrentSongIndex = ( Playlists.CurrentSongIndex == nowPlayingPlaylist.PlaylistItems.Count - 1 ) ? 0
					: Playlists.CurrentSongIndex + 1;
			}
			else
			{
				// If there are no more items to play in shuffle mode then re-initialise
				if ( songsLeftToShuffle.Count == 0 )
				{
					InitialiseShuffleIfOn( false );
				}

				Playlists.CurrentSongIndex = SelectShuffleItem();
			}
		}

		/// <summary>
		/// Select the previous song in the playlist
		/// </summary>
		/// <param name="_message"></param>
		public void MediaControlPlayPrevious()
		{
			// If shuffle mode is off then select the previous song, cycling back to the end when the start if reached
			if ( Playback.ShuffleOn == false )
			{
				Playlists.CurrentSongIndex = ( Playlists.CurrentSongIndex > 0 ) ? Playlists.CurrentSongIndex - 1
					: nowPlayingPlaylist.PlaylistItems.Count - 1;
			}
			else
			{
				// The current song being played is the last item in the shuffledSongs list.
				// Take that entry and put it back on the songsLeftToShuffle list.
				// Select the last item in the shuffledSongs list to play.
				// If there are no items in the shuffledSongs list then clear the CurrentSongIndex.
				if ( shuffledSongs.Count > 0 )
				{
					songsLeftToShuffle.Add( shuffledSongs.Last() );
					_ = shuffledSongs.Remove( shuffledSongs.Last() );
				}

				Playlists.CurrentSongIndex = ( shuffledSongs.Count > 0 ) ? shuffledSongs.Last().Index : -1;
			}
		}

		/// <summary>
		/// Called when the current song has finished playing
		/// Play the next song. If the end of the playlist has been reached and repeat is on then go back to the first song.
		/// If shuffle mode is on then randomly select the next song to play
		/// </summary>
		/// <param name="_"></param>
		private void SongFinished()
		{
			if ( Playback.ShuffleOn == false )
			{
				// Shuffle mode is not active, standard selection rules

				// If the last song has not been played then simply move on to the next song
				if ( Playlists.CurrentSongIndex < ( nowPlayingPlaylist.PlaylistItems.Count - 1 ) )
				{
					Playlists.CurrentSongIndex++;
				}
				// The end of the list has been reached. If repeat is on then start from the begining. Otherwise stop playing
				else
				{
					Playlists.CurrentSongIndex = ( Playback.RepeatOn == true ) ? 0 : -1;
				}
			}
			else
			{
				// Shuffle mode is on.
				// Are there any items left to play
				if ( songsLeftToShuffle.Count > 0 )
				{
					// Select one of these at random
					Playlists.CurrentSongIndex = SelectShuffleItem();
				}
				// If repeat is on then re-initialise shuffle mode and select an item to play. Otherwise stop playing
				else if ( Playback.RepeatOn == true )
				{
					InitialiseShuffleIfOn( false );
					Playlists.CurrentSongIndex = SelectShuffleItem();
				}
				else
				{
					Playlists.CurrentSongIndex = -1;
				}
			}
		}

		/// <summary>
		/// Initialise shuffle mode.
		/// Copy all the SongPlaylistItem items in the playlist to the songsLeftToShuffle list and clear the shuffledSongs list.
		/// Optionally remove the song currently selected from the songsLeftToShuffle and add it to the shuffledSongs list
		/// </summary>
		private void InitialiseShuffleIfOn( bool removeCurrentSong = true )
		{
			if ( Playback.ShuffleOn == true )
			{
				songsLeftToShuffle = nowPlayingPlaylist.PlaylistItems.Cast<SongPlaylistItem>().ToList();
				shuffledSongs.Clear();

				if ( ( removeCurrentSong == true ) && ( Playlists.CurrentSongIndex != -1 ) )
				{
					// Remove the currently playing song
					SongPlaylistItem itemPlaying = ( SongPlaylistItem )nowPlayingPlaylist.PlaylistItems[ Playlists.CurrentSongIndex ];
					_ = songsLeftToShuffle.Remove( itemPlaying );
					shuffledSongs.Add( itemPlaying );
				}
			}
		}

		/// <summary>
		/// Select an item from the songsLeftToShuffle list and return it's index 
		/// </summary>
		/// <returns></returns>
		private int SelectShuffleItem()
		{
			int selectedSongItem = -1;

			// Make sure there is something to select
			if ( songsLeftToShuffle.Count > 0 )
			{
				// Select an item from the songs left
				int shuffleItemIndex = rng.Next( songsLeftToShuffle.Count );

				// Remove this item from the list and add to the shuffledSongs list
				SongPlaylistItem itemToPlay = songsLeftToShuffle[ shuffleItemIndex ];
				songsLeftToShuffle.RemoveAt( shuffleItemIndex );
				shuffledSongs.Add( itemToPlay );

				selectedSongItem = itemToPlay.Index;
			}

			return selectedSongItem;
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
		/// The random number generator used to p;ick a song in shuffle mode
		/// </summary>
		private readonly Random rng = new Random();

		/// <summary>
		/// The curently selected NowPlaying playlist. 
		/// </summary>
		private SongPlaylist nowPlayingPlaylist = null;

		/// <summary>
		/// In shuffle mode, the songs that have not been played yet
		/// </summary>
		private List<SongPlaylistItem> songsLeftToShuffle;

		/// <summary>
		/// In shuffle mode, the songs that have been played
		/// </summary>
		private readonly List<SongPlaylistItem> shuffledSongs = new List<SongPlaylistItem>();
	}
}
