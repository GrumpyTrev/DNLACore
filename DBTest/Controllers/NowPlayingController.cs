using System;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The NowPlayingController is the Controller for the NowPlayingView. It responds to NowPlayingView commands and maintains Now Playing data in the
	/// NowPlayingViewModel
	/// </summary>
	class NowPlayingController
	{
		/// <summary>
		/// Register for external Now Playing list change messages
		/// </summary>
		static NowPlayingController()
		{
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( ShuffleModeChanged, typeof( ShuffleModeChangedMessage ) );
			Mediator.RegisterPermanent( MediaControlPlayNext, typeof( MediaControlPlayNextMessage ) );
			Mediator.RegisterPermanent( MediaControlPlayPrevious, typeof( MediaControlPlayPreviousMessage ) );
			Mediator.RegisterPermanent( SongFinished, typeof( SongFinishedMessage ) );
		}

		/// <summary>
		/// Get the SongPlaylist data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Set the selected song in the database and play it
		/// </summary>
		public static void UserSongSelected( int songIndex, bool playSong = true )
		{
			NowPlayingViewModel.CurrentSongIndex = songIndex;

			// Make sure the new song is played if requested
			if ( playSong == true )
			{
				new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
			}
		}

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
				// Before clearing it reset the selected song to stop it being played
				NowPlayingViewModel.CurrentSongIndex = -1;
				new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();

				// Now clear the Now Playing list 
				NowPlayingViewModel.NowPlayingPlaylist.Clear();
			}

			// Add the songs to the playlist. Shuffled if necessary
			if ( Playback.ShufflePlayOn == true )
			{
				NowPlayingViewModel.NowPlayingPlaylist.AddSongs( ShuffleSongs( songsToAdd.ToList() ) );
			}
			else
			{
				NowPlayingViewModel.NowPlayingPlaylist.AddSongs( songsToAdd );
			}

			// If the list was cleared and there are now some items in the list select the first entry and play it
			if ( ( clearFirst == true ) & ( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				NowPlayingViewModel.CurrentSongIndex = 0;
				new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
			}

			// Report change to UI
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called to delete one or more items from the Now Playing playlist.
		/// We need to determine the affect that this deletion may have on the index of the currently playing song.
		/// This can be done by examining the track numbers of the items to be deleted
		/// </summary>
		/// <param name="items"></param>
		public static void DeleteNowPlayingItems( IEnumerable<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the delete
			PlaylistItem currentPlaylistItem = null;

			// If the currently selected song is going to be deleted then invalidate it now
			// Only carry out these checks if a song has been selected
			if ( NowPlayingViewModel.CurrentSongIndex != -1 )
			{
				// Check if the items to delete contains the currently selected song
				if ( items.Any( item => ( item.Index == NowPlayingViewModel.CurrentSongIndex ) ) == true )
				{
					// The currently selected song is going to be deleted. Set it to invalid
					NowPlayingViewModel.CurrentSongIndex = -1;
					new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
				}
				else
				{
					// Save the current item
					currentPlaylistItem = NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.CurrentSongIndex ];
				}
			}

			// Delete the entries and report that the list has been updated
			NowPlayingViewModel.NowPlayingPlaylist.DeletePlaylistItems( items );

			// Adjust the track numbers
			NowPlayingViewModel.NowPlayingPlaylist.AdjustTrackNumbers();

			// Determine the index of the currently selected song from it's possibly new track number
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report change to UI
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Move a set of selected items down the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsDown( IEnumerable<PlaylistItem> items ) => MoveItems( items, false );

		/// <summary>
		/// Move a set of selected items up the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsUp( IEnumerable<PlaylistItem> items ) => MoveItems( items, true );

		/// <summary>
		/// Move the selected items up or down the list
		/// </summary>
		/// <param name="items"></param>
		/// <param name="moveUp"></param>
		private static void MoveItems( IEnumerable<PlaylistItem> items, bool moveUp )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( NowPlayingViewModel.CurrentSongIndex == -1 ) ? null
				: NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.CurrentSongIndex ];

			if ( moveUp == true )
			{
				NowPlayingViewModel.NowPlayingPlaylist.MoveItemsUp( items );
			}
			else
			{
				NowPlayingViewModel.NowPlayingPlaylist.MoveItemsDown( items );
			}

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			DataReporter?.PlaylistUpdated();
		}

		/// <summary>
		/// Called when the SongPlaylist data is available to be displayed, or needs to be refreshed
		/// </summary>
		private static void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			NowPlayingViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the NowPlaying playlist.
			NowPlayingViewModel.NowPlayingPlaylist = ( SongPlaylist )Playlists.GetNowPlayingPlaylist( NowPlayingViewModel.LibraryId );

			// Let the playback manager know the current song but don't play it yet
			NowPlayingViewModel.CurrentSongIndex = Playlists.CurrentSongIndex;
			new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong, DontPlay = true }.Send();

			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// Inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object _ ) => DataReporter?.SongSelected();

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Reload the data
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object _ ) => StorageDataAvailable();

		/// <summary>
		/// Called when a ShuffleModeChangedMessage is received.
		/// Get the new mode and update the model.
		/// If Shuffle mode has been turned on then shuffle the current playlist
		/// </summary>
		/// <param name="message"></param>
		private static void ShuffleModeChanged( object _ )
		{
			if ( Playback.ShufflePlayOn == true )
			{
				ShufflePlaylistItems();
			}
		}

		/// <summary>
		/// Play the next song in the playlist
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPlayNext( object _ )
		{
			NowPlayingViewModel.CurrentSongIndex = ( NowPlayingViewModel.CurrentSongIndex == ( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Count - 1 ) ) ?
				0 : NowPlayingViewModel.CurrentSongIndex + 1;

			new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
		}

		/// <summary>
		/// Play the previous song in the playlist
		/// </summary>
		/// <param name="_message"></param>
		private static void MediaControlPlayPrevious( object _ )
		{
			NowPlayingViewModel.CurrentSongIndex = ( NowPlayingViewModel.CurrentSongIndex > 0 ) ? NowPlayingViewModel.CurrentSongIndex - 1 :
				NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Count - 1;

			new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
		}

		/// <summary>
		/// Called when a SongFinishedMessage has been received.
		/// Play the next song. If the end of the playlist has been reached and repeat is on then go back to the first song.
		/// If shuffle mode is on then shuffle the songs before playimng the first one
		/// </summary>
		/// <param name="_"></param>
		private static void SongFinished( object _ )
		{
			if ( NowPlayingViewModel.CurrentSongIndex < ( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Count - 1 ) )
			{
				// Play the next song
				NowPlayingViewModel.CurrentSongIndex++;
			}
			else if ( ( Playback.RepeatPlayOn == true ) && ( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				// If shuffle mode is on then shuffle the items before playing them
				if ( Playback.ShufflePlayOn == true )
				{
					ShufflePlaylistItems();
				}

				NowPlayingViewModel.CurrentSongIndex = 0;
			}
			else
			{
				NowPlayingViewModel.CurrentSongIndex = -1;
			}

			// Play the song
			new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();
		}

		/// <summary>
		/// Shuffle the current playlist items
		/// </summary>
		private static void ShufflePlaylistItems()
		{
			// Stop the current playback
			NowPlayingViewModel.CurrentSongIndex = -1;
			new PlaySongMessage() { SongToPlay = NowPlayingViewModel.CurrentSong }.Send();

			// Extract the songs from the playlist, shuffle them and add them back to the playlist
			AddSongsToNowPlayingList( ShuffleSongs( NowPlayingViewModel.NowPlayingPlaylist.GetSongs() ), true );
		}

		/// <summary>
		/// Shuffle the list of specified songs
		/// </summary>
		/// <param name="songs"></param>
		private static List<Song> ShuffleSongs( List<Song> songs )
		{
			int n = songs.Count;
			while ( n > 1 )
			{
				n--;
				int k = rng.Next( n + 1 );
				Song value = songs[ k ];
				songs[ k ] = songs[ n ];
				songs[ n ] = value;
			}

			return songs;
		}

		/// <summary>
		/// Adjust the index of the currently selected song if it does not match the adjusted track number of the song
		/// </summary>
		/// <param name="selectedSong"></param>
		private static void AdjustSelectedSongIndex( PlaylistItem selectedSong )
		{
			if ( selectedSong != null )
			{
				int newSelectedIndex = selectedSong.Index;

				if ( newSelectedIndex != NowPlayingViewModel.CurrentSongIndex )
				{
					NowPlayingViewModel.CurrentSongIndex = newSelectedIndex;
				}
			}
		}

		/// <summary>
		/// The random number generator used to shuffle the list
		/// </summary>
		private static readonly Random rng = new Random();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static INowPlayingReporter DataReporter
		{
			private get => ( INowPlayingReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface INowPlayingReporter : DataReporter.IReporter
		{
			void SongSelected();
			void PlaylistUpdated();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}
