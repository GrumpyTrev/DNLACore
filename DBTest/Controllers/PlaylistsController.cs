using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains Playlist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	class PlaylistsController : BaseController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		static PlaylistsController()
		{
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );

			instance = new PlaylistsController();
		}

		/// <summary>
		/// Get the Playlist data
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			// Delete the playlist and then refresh the data held by the model
			Playlists.DeletePlaylist( thePlaylist );

			// Let other controllers know
			new PlaylistDeletedMessage().Send();

			// Refresh the playlists held by the model and report the change
			instance.StorageDataAvailable();
		}

		/// <summary>
		/// Delete the specified PlaylistItem items from its parent playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( Playlist thePlaylist, IEnumerable< PlaylistItem > items )
		{
			// Delete the PlaylistItem items.
			thePlaylist.DeletePlaylistItems( items );

			// Adjust the track numbers
			thePlaylist.AdjustTrackNumbers();

			// Report the change
			DataReporter?.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Add a new playlist with the specified name to the current library
		/// </summary>
		/// <param name="playlistName"></param>
		public static void AddPlaylist( string playlistName )
		{
			Playlists.AddPlaylist( new Playlist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId } );

			// Let other controllers know
			new PlaylistAddedMessage().Send();

			// Refresh the playlists held by the model and report the change
			instance.StorageDataAvailable();
		}

		/// <summary>
		/// Change the name of the specified playlist
		/// </summary>
		/// <param name="playlistName"></param>
		public static void RenamePlaylist( Playlist playlist, string newName )
		{
			playlist.Rename( newName );

			// Refresh the playlists held by the model and report the change
			instance.StorageDataAvailable();
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="playlist"></param>
		public static void AddSongsToPlaylist( IEnumerable<Song> songsToAdd, Playlist playlist ) => playlist.AddSongs( songsToAdd );

		/// <summary>
		/// Move a set of selected items down the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsDown( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsDown( items );

			DataReporter?.PlaylistUpdated( thePlaylist );
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the track numbers
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <param name="items"></param>
		public static void MoveItemsUp( Playlist thePlaylist, IEnumerable<PlaylistItem> items )
		{
			thePlaylist.MoveItemsUp( items );

			DataReporter?.PlaylistUpdated( thePlaylist );
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
							List<Song> matchingTitles = await SongAccess.GetMatchingSongAsync( item.Song.Title, sources[ sourceIndex++ ].Id );

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

									// Make sure that the Artist is stored with the song
									matchingSong.Artist = nameCheck;
								}

								titleIndex++;
							}
						}
					}

					if ( songsToAdd.Count > 0 )
					{
						// Add the songs to the new Playlist. No need to wait for this
						duplicatedPlaylist.AddSongs( songsToAdd );
					}
				}
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		protected override void StorageDataAvailable( object _ = null )
		{
			// Save the libray being used locally to detect changes
			PlaylistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			PlaylistsViewModel.Playlists = Playlists.GetPlaylistsForLibrary( PlaylistsViewModel.LibraryId );
			PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();

			base.StorageDataAvailable();
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Let the view know
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message ) => DataReporter?.PlaylistUpdated( ( ( PlaylistSongsAddedMessage )message ).Playlist );

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data then reload
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message )
		{
			// Clear the displayed data
			PlaylistsViewModel.ClearModel();

			// Reread the data
			instance.dataValid = false;
			instance.StorageDataAvailable();
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IPlaylistsReporter DataReporter
		{
			private get => ( IPlaylistsReporter )instance.Reporter;
			set => instance.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IPlaylistsReporter : IReporter
		{
			void PlaylistUpdated( Playlist playlist );
		}

		/// <summary>
		/// The one and only PlaylistsController instance
		/// </summary>
		private static readonly PlaylistsController instance = null;
	}
}