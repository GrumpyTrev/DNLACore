using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains Playlist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	static class PlaylistsController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		static PlaylistsController()
		{
			Mediator.RegisterPermanent( SongsAddedAsync, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
		}

		/// <summary>
		/// Get the Playlist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetPlaylistsAsync( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( PlaylistsViewModel.LibraryId != libraryId )
			{
				PlaylistsViewModel.LibraryId = libraryId;

				await RefreshModelData();
			}
			else
			{
				// Let the Views know that Playlists data is available
				Reporter?.PlaylistsDataAvailable();
			}
		}

		/// <summary>
		/// Get the contents for the specified Playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static async Task GetPlaylistContentsAsync( Playlist thePlaylist )
		{
			await PlaylistAccess.GetPlaylistContentsWithArtistsAsync( thePlaylist );

			// Sort the PlaylistItems by Track
			thePlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static async void DeletePlaylistAsync( Playlist thePlaylist )
		{
			// Delete the playlist and then refresh the data held by the model
			await PlaylistAccess.DeletePlaylistAsync( thePlaylist );

			// Refresh the playlists held by the model and report the change
			await RefreshModelData();

			// Let other controllers know
			new PlaylistDeletedMessage().Send();
		}

		/// <summary>
		/// Delete the specified PlaylistItem items from its parent playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static async void DeletePlaylistItemsAsync( Playlist thePlaylist, List< PlaylistItem > items )
		{
			// Delete the PlaylistItem items and then report that the playlist has changed
			await PlaylistAccess.DeletePlaylistItemsAsync( thePlaylist, items );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Add a new playlist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public static async void AddPlaylistAsync( string playlistName )
		{
			await PlaylistAccess.AddPlaylistAsync( playlistName, PlaylistsViewModel.LibraryId );

			// Refresh the playlists held by the model and report the change
			await RefreshModelData();

			// Let other controllers know
			new PlaylistAddedMessage().Send();
		}

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static async void MoveItemsDown( Playlist thePlaylist, List<PlaylistItem> items )
		{
			// There must be at least one PlayList entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			PlaylistItem itemToMove = thePlaylist.PlaylistItems[ items.Last().Track ];
			thePlaylist.PlaylistItems.RemoveAt( items.Last().Track );
			thePlaylist.PlaylistItems.Insert( items.First().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			await AdjustTrackNumbers( thePlaylist );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static async void MoveItemsUp( Playlist thePlaylist, List<PlaylistItem> items )
		{
			// There must be at least one PlayList entry above those that are selected. That entry needs to be moved to below the end of the selection
			PlaylistItem itemToMove = thePlaylist.PlaylistItems[ items.First().Track - 2 ];
			thePlaylist.PlaylistItems.RemoveAt( items.First().Track - 2 );
			thePlaylist.PlaylistItems.Insert( items.Last().Track - 1, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			await AdjustTrackNumbers( thePlaylist );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Adjust the track numbers to match the indexex in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		private static async Task AdjustTrackNumbers( Playlist thePlaylist )
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


		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Make sure that the specified playlist contents are refreshed and let the view know
		/// </summary>
		/// <param name="message"></param>
		private static async void SongsAddedAsync( object message )
		{
			PlaylistSongsAddedMessage songsAddedMessage = message as PlaylistSongsAddedMessage;

			// Get the playlist from the model (not the database) and refresh its contents.
			// If it can't be found then do nothing - report an error?
			Playlist addedToPlaylist = PlaylistsViewModel.Playlists.FirstOrDefault( d => ( d.Name == songsAddedMessage.PlaylistName ) );

			if ( addedToPlaylist != null )
			{
				await GetPlaylistContentsAsync( addedToPlaylist );
				Reporter?.PlaylistUpdated( songsAddedMessage.PlaylistName );
			}
		}

		/// <summary>
		/// Refresh the model data held by the model
		/// </summary>
		private static async Task RefreshModelData()
		{
			PlaylistsViewModel.Playlists = await PlaylistAccess.GetPlaylistDetailsAsync( PlaylistsViewModel.LibraryId );
			PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();

			// Let the views know that Playlists data is available
			Reporter?.PlaylistsDataAvailable();
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data then reload
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message )
		{
			// Clear the displayed data
			PlaylistsViewModel.ClearModel();

			// Publish the data
			Reporter?.PlaylistsDataAvailable();

			// Reread the data
			GetPlaylistsAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void PlaylistsDataAvailable();
			void PlaylistUpdated( string playlistName );
		}
	}
}