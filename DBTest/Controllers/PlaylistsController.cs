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
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
		}

		/// <summary>
		/// Get the Playlist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise wait for the data to be made available
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetPlaylists( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( PlaylistsViewModel.LibraryId != libraryId )
			{
				PlaylistsViewModel.LibraryId = libraryId;

				// All Playlists are read at startup. So wait until that is available and then carry out the rest of the initialisation
				StorageController.RegisterInterestInDataAvailable( PlaylistDataAvailable );
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
			await Playlists.GetPlaylistContentsAsync( thePlaylist );

			// Sort the PlaylistItems by Track
			thePlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			// Delete the playlist and then refresh the data held by the model
			Playlists.DeletePlaylist( thePlaylist );

			// Refresh the playlists held by the model and report the change
			PlaylistDataAvailable();

			// Let other controllers know
			new PlaylistDeletedMessage().Send();
		}

		/// <summary>
		/// Delete the specified PlaylistItem items from its parent playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( Playlist thePlaylist, List< PlaylistItem > items )
		{
			// Delete the PlaylistItem items.
			Playlists.DeletePlaylistItems( thePlaylist, items );

			// Adjust the track numbers
			BaseController.AdjustTrackNumbers( thePlaylist );

			// Report the change
			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Add a new playlist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public static void AddPlaylist( string playlistName )
		{
			Playlists.AddPlaylist( new Playlist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId } );

			// Refresh the playlists held by the model and report the change
			PlaylistDataAvailable();

			// Let other controllers know
			new PlaylistAddedMessage().Send();
		}

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsDown( Playlist thePlaylist, List<PlaylistItem> items )
		{
			BaseController.MoveItemsDown( thePlaylist, items );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsUp( Playlist thePlaylist, List<PlaylistItem> items )
		{
			BaseController.MoveItemsUp( thePlaylist, items );

			Reporter?.PlaylistUpdated( thePlaylist.Name );
		}

		/// <summary>
		/// Check if the specified playlist exists in other libraries
		/// </summary>
		/// <param name="name"></param>
		/// <param name="playListLibrary"></param>
		/// <returns></returns>
		public static bool CheckForOtherPlaylists( string name, int playListLibrary ) =>
			Playlists.PlaylistCollection.Exists( list => ( list.Name == name ) && ( list.LibraryId != playListLibrary ) );

		/// <summary>
		/// Duplicate a playlist in the other libraries
		/// </summary>
		/// <param name="playlistToDuplicate"></param>
		public static async void DuplicatePlaylistAsync( Playlist playlistToDuplicate )
		{
			// Duplicate the playlist in all libraries except the one it is in
			foreach ( Library library in Libraries.LibraryCollection )
			{
				if ( library.Id != playlistToDuplicate.LibraryId )
				{
					// If a playlist with the same name already exists then delete its contents
					Playlist existingPlaylist = Playlists.PlaylistCollection
						.Where( playlist => ( playlist.Name == playlistToDuplicate.Name ) && ( playlist.LibraryId == library.Id ) ).SingleOrDefault();

					if ( existingPlaylist != null )
					{
						Playlists.DeletePlaylist( existingPlaylist );
					}

					// Now create a new playlist in the library with the same name
					Playlist duplicatedPlaylist = new Playlist() { Name = playlistToDuplicate.Name, LibraryId = library.Id };
					Playlists.AddPlaylist( duplicatedPlaylist );

					// Attempt to find matching songs for each PlaylistItem in the Playlist
					// Need to access the songs via the Sources associated with the Library
					List< Source > sources = Sources.GetSourcesForLibrary( library.Id );

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
								Artist nameCheck = Artists.GetArtistById( 
									ArtistAlbums.GetArtistAlbumById( matchingTitles[ titleIndex ].ArtistAlbumId ).ArtistId );

								// Correct name?
								if ( nameCheck.Name == item.Artist.Name )
								{
									matchingSong = matchingTitles[ titleIndex ];
									songsToAdd.Add( matchingSong );

									// Make sure that the Artist us stored with the song
									matchingSong.Artist = nameCheck;
								}

								titleIndex++;
							}
						}
					}

					if ( songsToAdd.Count > 0 )
					{
						// Add the songs to the new Playlist
						Playlists.AddSongsToPlaylistAsync( duplicatedPlaylist, songsToAdd );
					}
				}
			}
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Let the view know
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message ) => 
			Reporter?.PlaylistUpdated( ( ( PlaylistSongsAddedMessage )message ).PlaylistName );

		/// <summary>
		/// Called when the Playlist data is available to be displayed, or needs to be refreshed
		/// </summary>
		private static void PlaylistDataAvailable( object _ = null )
		{
			PlaylistsViewModel.Playlists = Playlists.GetPlaylistsForLibrary( PlaylistsViewModel.LibraryId );
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
			GetPlaylists( ConnectionDetailsModel.LibraryId );
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