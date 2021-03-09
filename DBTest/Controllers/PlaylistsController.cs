using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains Playlist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	class PlaylistsController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		static PlaylistsController()
		{
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( DisplayGenreChanged, typeof( DisplayGenreMessage ) );
			Mediator.RegisterPermanent( TagAddedOrDeleted, typeof( TagAddedMessage ) );
			Mediator.RegisterPermanent( TagAddedOrDeleted, typeof( TagDeletedMessage ) );
			Mediator.RegisterPermanent( TagChanged, typeof( TagMembershipChangedMessage ) );
		}

		/// <summary>
		/// Get the Playlist data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Delete the specified playlist and its contents
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			// Delete the playlist and then refresh the data held by the model
			Playlists.DeletePlaylist( thePlaylist );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
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
		public static async Task<Playlist> AddPlaylistAsync( string playlistName )
		{
			Playlist newPlaylist = new Playlist() { Name = playlistName, LibraryId = PlaylistsViewModel.LibraryId };

			await Playlists.AddPlaylistAsync( newPlaylist );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();

			return newPlaylist;
		}

		/// <summary>
		/// Change the name of the specified playlist
		/// </summary>
		/// <param name="playlistName"></param>
		public static void RenamePlaylist( Playlist playlist, string newName )
		{
			playlist.Rename( newName );

			// Refresh the playlists held by the model and report the change
			StorageDataAvailable();
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
		/// Move a set of selected items down the specified playlist and update the tag index numbers
		/// </summary>
		/// <param name="theTag"></param>
		/// <param name="items"></param>
		public static void MoveItemsDown( Tag theTag, IEnumerable<TaggedAlbum> items )
		{
			theTag.MoveItemsDown( items );

			DataReporter?.PlaylistUpdated( theTag );

			// Report this tag change
			sendByMe = true;
			new TagMembershipChangedMessage() { ChangedTags = new List<string>() { theTag.Name } }.Send();
		}

		/// <summary>
		/// Move a set of selected items up the specified playlist and update the tag index numbers
		/// </summary>
		/// <param name="theTag"></param>
		/// <param name="items"></param>
		public static void MoveItemsUp( Tag theTag, IEnumerable<TaggedAlbum> items )
		{
			theTag.MoveItemsUp( items );

			DataReporter?.PlaylistUpdated( theTag );

			// Report this tag change
			sendByMe = true;
			new TagMembershipChangedMessage() { ChangedTags = new List<string>() { theTag.Name } }.Send();
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
					await Playlists.AddPlaylistAsync( duplicatedPlaylist );

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
							List<Song> matchingTitles = await DbAccess.GetMatchingSongAsync( item.Song.Title, sources[ sourceIndex++ ].Id );

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
						// Add the songs to the new Playlist.
						duplicatedPlaylist.AddSongs( songsToAdd );
					}
				}
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static async void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			PlaylistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the Playlists and playlist names. Make sure a copy of the list is used as we're going to sort it 
			PlaylistsViewModel.Playlists = Playlists.GetPlaylistsForLibrary( PlaylistsViewModel.LibraryId ).ToList();
			PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();

			// Get the user Tags
			PlaylistsViewModel.Tags = Tags.TagsCollection.Where( ta => ta.UserTag == true ).ToList();

			// Get the display genre flag
			PlaylistsViewModel.DisplayGenre = Playback.DisplayGenre;

			// To generate the data to be displayed the Playlists and Tags need to be sorted alphabetically and then combined into 
			// a list of objects. Do this off the UI thread
			await Task.Run( () =>
			{
				// Sort the playlists by name
				PlaylistsViewModel.Playlists.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );

				// Sort the tags by name
				PlaylistsViewModel.Tags.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );

				// Now copy to the combined list
				PlaylistsViewModel.CombinedList.Clear();
				PlaylistsViewModel.CombinedList.AddRange( PlaylistsViewModel.Playlists );
				PlaylistsViewModel.CombinedList.AddRange( PlaylistsViewModel.Tags );
			} );

			DataReporter?.DataAvailable();
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
		private static void SelectedLibraryChanged( object _ )
		{
			// Clear the displayed data
			PlaylistsViewModel.ClearModel();

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
			PlaylistsViewModel.DisplayGenre = ( ( DisplayGenreMessage )message ).DisplayGenre;
			DataReporter?.DisplayGenreChanged();
		}

		/// <summary>
		/// Called when a TagAddedMessage or TagDeletedMessage has been received
		/// </summary>
		/// <param name="message"></param>
		private static void TagAddedOrDeleted( object message )
		{
			if ( ( ( message is TagAddedMessage tagAdded ) && ( tagAdded.AddedTag.UserTag == true ) ) || 
				( ( message is TagDeletedMessage tagDeleted ) && ( tagDeleted.DeletedTag.UserTag == true ) ) )
			{
				// Reread the data as the source Tags collection has changed
				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received
		/// If this is a tag currently being displayed then report it.
		/// Currently this controller as well as others generates the same message, so for now use the 'sentByMe' flag to control
		/// whether or not this message is processed
		/// </summary>
		/// <param name="message"></param>
		private static void TagChanged( object message )
		{
			if ( sendByMe == true )
			{
				sendByMe = false;
			}
			else
			{
				List<string> changedTags = ( ( TagMembershipChangedMessage )message ).ChangedTags;
				foreach ( string tagName in changedTags )
				{
					Tag changedTag = Tags.GetTagByName( tagName );
					if ( ( changedTag != null ) && ( changedTag.UserTag == true ) )
					{
						DataReporter?.PlaylistUpdated( changedTag );
					}
				}
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IPlaylistsReporter DataReporter
		{
			get => ( IPlaylistsReporter )dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IPlaylistsReporter : DataReporter.IReporter
		{
			void PlaylistUpdated( object playlist );
			void DisplayGenreChanged();
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );

		/// <summary>
		/// Flag set when this controller sends TagMembershipChangedMessage messages to prevent them being processed here
		/// This should only be temporary.
		/// </summary>
		private static bool sendByMe = false;
	}
}