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
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( DisplayGenreChanged, typeof( DisplayGenreMessage ) );
		}

		/// <summary>
		/// Get the Playlist data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Set the selected song in the database and raise the SongSelectedMessage
		/// Don't update the model at this stage. Update it when the SongSelectedMessage is received
		/// </summary>
		public static void SetSelectedSong( int songIndex, bool playSong = true )
		{
			Playlists.CurrentSong = songIndex;

			// Make sure the new song is played if requested
			if ( ( songIndex != -1 ) && ( playSong == true ) )
			{
				new PlayCurrentSongMessage().Send();
			}
		}

		/// <summary>
		/// Shuffle the NowPlayingPlaylist.PlaylistItems and select the first item to play
		/// </summary>
		public static void ShuffleNowPlayingList()
		{
			SetSelectedSong( -1 );

			// Extract the songs from the playlist and shuffle them
			List<Song> songs = NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Select( item => item.Song ).ToList();

			int n = songs.Count;
			while ( n > 1 )
			{
				n--;
				int k = rng.Next( n + 1 );
				Song value = songs[ k ];
				songs[ k ] = songs[ n ];
				songs[ n ] = value;
			}

			// Now make these songs the playlist
			BaseController.AddSongsToNowPlayingList( songs, true );
		}

		/// <summary>
		/// Called to delete one or more items from the Now Playing playlist.
		/// We need to determine the affect that this deletion may have on the index of the currently playing song.
		/// This can be done by examining the track numbers of the items to be deleted, the track number is the index + 1
		/// </summary>
		/// <param name="items"></param>
		public static void DeleteNowPlayingItems( IEnumerable<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the delete
			PlaylistItem currentPlaylistItem = null;

			// If the currently selected song is going to be deleted then invalidate it now
			// Only carry out these checks if a song has been selected
			if ( NowPlayingViewModel.SelectedSong != -1 )
			{
				// Check if the items to delete contains the currently selected song
				if ( items.Any( item => ( item.Track == ( NowPlayingViewModel.SelectedSong + 1 ) ) ) == true )
				{
					// The currently selected song is going to be deleted. Set it to invalid
					SetSelectedSong( -1 );
				}
				else
				{
					// Save the current item
					currentPlaylistItem = NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.SelectedSong ];
				}
			}

			// Delete the entries and report that the list has been updated
			NowPlayingViewModel.NowPlayingPlaylist.DeletePlaylistItems( items );

			// Adjust the track numbers
			NowPlayingViewModel.NowPlayingPlaylist.AdjustTrackNumbers();

			// Determine the index of the currently selected song from it's possibly new track number
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Move a set of selected items down the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsDown( IEnumerable<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( NowPlayingViewModel.SelectedSong == -1 ) ? null
				: NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.SelectedSong ];

			NowPlayingViewModel.NowPlayingPlaylist.MoveItemsDown( items );

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			DataReporter?.PlaylistUpdated();
		}

		/// <summary>
		/// Move a set of selected items up the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsUp( IEnumerable<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( NowPlayingViewModel.SelectedSong == -1 ) ? null
				: NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.SelectedSong ];

			NowPlayingViewModel.NowPlayingPlaylist.MoveItemsUp( items );

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			DataReporter?.PlaylistUpdated();
		}

		/// <summary>
		/// Called when the Playlist data is available to be displayed, or needs to be refreshed
		/// </summary>
		private static void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			NowPlayingViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the NowPlaying playlist.
			NowPlayingViewModel.NowPlayingPlaylist = Playlists.GetNowPlayingPlaylist( NowPlayingViewModel.LibraryId );

			// Get the selected song
			NowPlayingViewModel.SelectedSong = Playlists.CurrentSong;

			// Get the display genre flag
			NowPlayingViewModel.DisplayGenre = Playback.DisplayGenre;

			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Make the new contents available if already accessed
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			if ( ( ( PlaylistSongsAddedMessage )message ).Playlist == NowPlayingViewModel.NowPlayingPlaylist )
			{
				// Force a total refresh  by clearing the previous results
				NowPlayingViewModel.ClearModel();

				// Reread the data
				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// Update the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object _message )
		{
			NowPlayingViewModel.SelectedSong = Playlists.CurrentSong;

			DataReporter?.SongSelected();
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data and the filter and then reload
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message )
		{
			// Clear the displayed data
			NowPlayingViewModel.ClearModel();

			// Reread the data
			StorageDataAvailable();
		}

		/// <summary>
		/// Called when a DisplayGenreMessage is received.
		/// Update the model and report the change
		/// </summary>
		/// <param name="message"></param>
		private static void DisplayGenreChanged( object message )
		{
			NowPlayingViewModel.DisplayGenre = ( ( DisplayGenreMessage )message ).DisplayGenre;
			DataReporter?.DisplayGenreChanged();
		}

		/// <summary>
		/// Adjust the index of the currently selected song if it does not match the adjusted track number of the song
		/// </summary>
		/// <param name="selectedSong"></param>
		private static void AdjustSelectedSongIndex( PlaylistItem selectedSong )
		{
			if ( selectedSong != null )
			{
				int newSelectedIndex = selectedSong.Track - 1;

				if ( newSelectedIndex != NowPlayingViewModel.SelectedSong )
				{
					SetSelectedSong( newSelectedIndex, false );
				}
			}
		}

		/// <summary>
		/// The random number generator used to shuffle the list
		/// </summary>
		private static readonly Random rng = new Random();

		/// <summary>
		/// The name given to the Now Playing playlist
		/// </summary>
		public const string NowPlayingPlaylistName = "Now Playing";

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
			void DisplayGenreChanged();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}
