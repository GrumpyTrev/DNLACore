using System;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The NowPlayingController is the Controller for the NowPlayingView. It responds to NowPlayingView commands and maintains Now Playing data in the
	/// NowPlayingViewModel
	/// </summary>
	static class NowPlayingController
	{
		/// <summary>
		/// Register for external Now Playing list change messages
		/// </summary>
		static NowPlayingController()
		{
			Mediator.RegisterPermanent( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
			Mediator.RegisterPermanent( SongSelected, typeof( SongSelectedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
		}

		/// <summary>
		/// Get the Now Playing data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetNowPlayingListAsync( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( NowPlayingViewModel.LibraryId != libraryId )
			{
				NowPlayingViewModel.LibraryId = libraryId;

				// Get the list witrh artists
				NowPlayingViewModel.NowPlayingPlaylist = await PlaylistAccess.GetNowPlayingListAsync( NowPlayingViewModel.LibraryId, true );

				// Sort the PlaylistItems by Track
				NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );

				// Get the selected song
				NowPlayingViewModel.SelectedSong = PlaybackDetails.SongIndex;

				NowPlayingViewModel.DataValid = true;
			}

			// Publish this data unless it is still being obtained
			if ( NowPlayingViewModel.DataValid == true )
			{
				Reporter?.NowPlayingDataAvailable();
			}
		}

		/// <summary>
		/// Set the selected song in the database and raise the SongSelectedMessage
		/// Don't update the model at this stage. Update it when the SongSelectedMessage is received
		/// </summary>
		public static void SetSelectedSong( int songIndex, bool playSong = true )
		{
			PlaybackDetails.SongIndex = songIndex;
			new SongSelectedMessage() { ItemNo = songIndex }.Send();

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
			BaseController.AddSongsToNowPlayingListAsync( songs, true, NowPlayingViewModel.LibraryId );
		}

		/// <summary>
		/// Called to delete one or more items from the Now Playing playlist.
		/// We need to determine the affect that this deletion may have on the index of the currently playing song.
		/// This can be done by examining the track numbers of the items to be deleted, the track number is the index + 1
		/// </summary>
		/// <param name="items"></param>
		public static void DeleteNowPlayingItems( List<PlaylistItem> items )
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
			PlaylistAccess.DeletePlaylistItemsAsync( NowPlayingViewModel.NowPlayingPlaylist, items );

			// Adjust the track numbers
			BaseController.AdjustTrackNumbers( NowPlayingViewModel.NowPlayingPlaylist );

			// Determine the index of the currently selected song from it's possibly new track number
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report
			Reporter?.NowPlayingDataAvailable();
		}

		/// <summary>
		/// Move a set of selected items down the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsDown( List<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( NowPlayingViewModel.SelectedSong == -1 ) ? null
				: NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.SelectedSong ];

			BaseController.MoveItemsDown( NowPlayingViewModel.NowPlayingPlaylist, items );

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			Reporter?.PlaylistUpdated();
		}

		/// <summary>
		/// Move a set of selected items up the Now Playing playlist and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public static void MoveItemsUp( List<PlaylistItem> items )
		{
			// Record the currently selected song so its track number can be checked after the move
			PlaylistItem currentPlaylistItem = ( NowPlayingViewModel.SelectedSong == -1 ) ? null
				: NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems[ NowPlayingViewModel.SelectedSong ];

			BaseController.MoveItemsUp( NowPlayingViewModel.NowPlayingPlaylist, items );

			// Now adjust the index of the selected song
			AdjustSelectedSongIndex( currentPlaylistItem );

			// Report that the playlist has been updated - don't use NowPlayingDataAvailable as that will clear the selections
			Reporter?.PlaylistUpdated();
		}


		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Make the new contents available if already accessed
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			// Force a total refresh  by clearing the previous results
			NowPlayingViewModel.ClearModel();
			GetNowPlayingListAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// Update the local model and inform the reporter 
		/// </summary>
		/// <param name="message"></param>
		private static void SongSelected( object message )
		{
			NowPlayingViewModel.SelectedSong = ( ( SongSelectedMessage )message ).ItemNo;
			Reporter?.SongSelected();
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
			GetNowPlayingListAsync( ConnectionDetailsModel.LibraryId );
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
		private static Random rng = new Random();

		/// <summary>
		/// The name given to the Now Playing playlist
		/// </summary>
		public const string NowPlayingPlaylistName = "Now Playing";

		// The interface instance used to report back controller results
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void NowPlayingDataAvailable();
			void SongSelected();
			void PlaylistUpdated();
		}
	}
}
