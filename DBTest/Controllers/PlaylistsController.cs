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
				if ( PlaylistsViewModel.DataValid == true )
				{
					Reporter?.PlaylistsDataAvailable();
				}
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
			// Delete the PlaylistItem items
			await PlaylistAccess.DeletePlaylistItemsAsync( thePlaylist, items );

			// Adjust the track numbers
			await BaseController.AdjustTrackNumbersAsync( thePlaylist );

			// Report the change
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
			await BaseController.AdjustTrackNumbersAsync( thePlaylist );

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
			await BaseController.AdjustTrackNumbersAsync( thePlaylist );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Check if the specified playlist exists in other libraries
		/// </summary>
		/// <param name="name"></param>
		/// <param name="playListLibrary"></param>
		/// <returns></returns>
		public static async Task<bool> CheckForOtherPlaylistsAsync( string name, int playListLibrary ) =>
			( await PlaylistAccess.GetAllPlaylists() ).Count( playlist => ( playlist.Name == name ) && ( playlist.LibraryId != playListLibrary ) ) > 0;

		/// <summary>
		/// Duplicate a playlist in the other libraries
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		public static async void DuplicatePlaylistAsync( Playlist playlistToDuplicate )
		{
			// Duplicate the playlist in all libraries except the one it is in
			List< Library > libraries = await LibraryAccess.GetLibrariesAsync();
			foreach ( Library library in libraries )
			{
				if ( library.Id != playlistToDuplicate.LibraryId )
				{
					// If a playlist with the same name already exists then delete its contents
					Playlist existingPlaylist = ( await PlaylistAccess.GetPlaylistDetailsAsync( library.Id ) )
						.Where( playlist => playlist.Name == playlistToDuplicate.Name ).SingleOrDefault();

					if ( existingPlaylist != null )
					{
						await PlaylistAccess.DeletePlaylistAsync( existingPlaylist );
					}

					// Now create a new playlist in the library with the same name
					await PlaylistAccess.AddPlaylistAsync( playlistToDuplicate.Name, library.Id );

					// Attempt to find matching songs for each PlaylistItem in the Playlist
					// Need to access the songs via the Sources associated with the Library
					List< Source > sources = await LibraryAccess.GetSourcesAsync( library.Id );

					// Keep track of the matching songs
					List<Song> songsToAdd = new List<Song>();

					foreach ( PlaylistItem item in playlistToDuplicate.PlaylistItems )
					{
						Song matchingSong = null;
						int sourceIndex = 0;

						while ( ( matchingSong == null ) && ( sourceIndex < sources.Count ) )
						{
							// Get a list of all the songs with matching Titles in the source
							List<Song> matchingTitles = await ArtistAccess.GetMatchingSongAsync( item.Song.Title, sources[ sourceIndex++ ].Id );

							// Now for each song access the associated artist
							int titleIndex = 0;
							while ( ( matchingSong == null ) && ( titleIndex < matchingTitles.Count ) )
							{
								ArtistAlbum artistAlbum = await ArtistAccess.GetArtistAlbumAsync( matchingTitles[ titleIndex ].ArtistAlbumId );
								Artist nameCheck = await ArtistAccess.GetArtistAsync( artistAlbum.ArtistId );

								// Correct name?
								if ( nameCheck.Name == item.Artist.Name )
								{
									matchingSong = matchingTitles[ titleIndex ];
									songsToAdd.Add( matchingSong );
								}

								titleIndex++;
							}
						}
					}

					if ( songsToAdd.Count > 0 )
					{
						// Add the songs to the new Playlist
						await PlaylistAccess.AddSongsToPlaylistAsync( songsToAdd, playlistToDuplicate.Name, library.Id );
					}
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

			PlaylistsViewModel.DataValid = true;

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