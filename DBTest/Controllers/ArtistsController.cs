using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	static class ArtistsController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static ArtistsController()
		{
			Mediator.RegisterPermanent( PlaylistAddedOrDeleted, typeof( PlaylistDeletedMessage ) );
			Mediator.RegisterPermanent( PlaylistAddedOrDeleted, typeof( PlaylistAddedMessage ) );
			Mediator.RegisterPermanent( TagMembershipChanged, typeof( TagMembershipChangedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( TagDetailsChanged, typeof( TagDetailsChangedMessage ) );
			Mediator.RegisterPermanent( TagDeleted, typeof( TagDeletedMessage ) );
			Mediator.RegisterPermanent( AlbumChanged, typeof( AlbumPlayedStateChangedMessage ) );
		}

		/// <summary>
		/// Get the Artist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetArtists( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( ArtistsViewModel.LibraryId != libraryId )
			{
				// Make sure that this data is not returned until all of it is available
				ArtistsViewModel.DataValid = false;

				// New data is required
				ArtistsViewModel.LibraryId = libraryId;

				// Get the list of current playlists and extract the names to a list
				GetPlayListNames();

				// All Artists are read as part of the storage data. So wait until that is available and then carry out the rest of the 
				// initialisation
				StorageController.RegisterInterestInDataAvailable( StorageDataAvailable );
			}
			else
			{
				// Publish the data if valid
				if ( ArtistsViewModel.DataValid == true )
				{
					Reporter?.ArtistsDataAvailable();
				}
			}
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// This amount of work could/should be done in a non-UI task, but not sure at the moment how to
		/// interact with the expanding UI.
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist )
		{
			// Have the contents been accessed before
			if ( theArtist.DetailsRead == false )
			{
				// No get them
				await ArtistAccess.GetArtistSongsAsync( theArtist );

				// Mark the details have been read
				theArtist.DetailsRead = true;

				// Sort the songs by track number
				foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
				{
					artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, string playlistName )
		{
			// Carry out the common processing to add songs to a playlist
			Playlists.GetPlaylist( playlistName, ArtistsViewModel.LibraryId ).AddSongs( songsToAdd );

			// Publish this event
			new PlaylistSongsAddedMessage() { PlaylistName = playlistName }.Send();
		}

		/// <summary>
		/// Apply the specified filter to the data being displayed
		/// Once the Artists have been filtered prepare them for display by sorting and combinig them with their ArtistAlbum entries
		/// </summary>
		/// <param name="newFilter"></param>
		public static async Task ApplyFilterAsync( Tag newFilter )
		{
			// Update the model
			ArtistsViewModel.CurrentFilter = newFilter;

			// alphabetic and identity sorting are available to the user
			ArtistsViewModel.SortSelector.MakeAvailable( new List<SortSelector.SortType> { SortSelector.SortType.alphabetic, SortSelector.SortType.identity } );

			// Check for no simple or group tag filters
			if ( ( ArtistsViewModel.CurrentFilter == null ) && ( ArtistsViewModel.TagGroups.Count == 0 ) )
			{
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists;
			}
			else
			{
				await Task.Run( () =>
				{
					// Combine the simple and group tabs
					List<TaggedAlbum> albumFilter = BaseController.CombineAlbumFilters( ArtistsViewModel.CurrentFilter, ArtistsViewModel.TagGroups );

					// First of all form a list of all the album identities in the selected filter
					HashSet<int> albumIds = albumFilter.Select( ta => ta.AlbumId ).ToHashSet();

					// Now get all the artist identities of the albums that are tagged
					HashSet<int> artistIds = ArtistAlbums.ArtistAlbumCollection.FindAll( aa => albumIds.Contains( aa.AlbumId ) ).Select( aa => aa.ArtistId ).ToHashSet();

					// Now get the Artists from the list of artist ids
					ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists.Where( art => artistIds.Contains( art.Id ) == true ).ToList();

					// If the TagOrder flag is set then set the sort order to Id order.
					if ( ( ArtistsViewModel.CurrentFilter?.TagOrder ?? false ) == true )
					{
						ArtistsViewModel.SortSelector.SetActiveSortOrder( SortSelector.SortType.identity );
					}
				} );
			}

			// Sort the artists to the order specified in the SortSelector and publish the data
			await SortArtistsAsync( true );
		}

		/// <summary>
		/// Sort the Artists according to the currently selected sort order
		/// </summary>
		public static async Task SortArtistsAsync( bool refreshData = false )
		{
			// Do the sorting and indexing off the UI task
			await Task.Run( () =>
			{
				switch ( ArtistsViewModel.SortSelector.CurrentSortOrder )
				{
					case SortSelector.SortOrder.alphaDescending:
					case SortSelector.SortOrder.alphaAscending:
					{
						if ( ArtistsViewModel.SortSelector.CurrentSortOrder == SortSelector.SortOrder.alphaAscending )
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
						}
						else
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
						}

						break;
					}

					case SortSelector.SortOrder.idAscending:
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Id.CompareTo( b.Id ); } );
						break;
					}

					case SortSelector.SortOrder.idDescending:
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Id.CompareTo( a.Id ); } );
						break;
					}
				}

				// Prepare the combined Artist/ArtistAlbum list - this has to be done after the Artists have been sorted
				PrepareCombinedList();
			} );

			if ( refreshData == true )
			{
				// Publish the data
				Reporter?.ArtistsDataAvailable();
			}
		}

		/// <summary>
		/// Sort the ArtistAlbum entries in each Artist by the album year
		/// </summary>
		private static async Task SortArtistAlbumsAsync()
		{
			await Task.Run( () =>
			{
				// Sort the ArtistAlbum entries in each Artist by the album year
				ArtistsViewModel.UnfilteredArtists.ForEach( art => art.ArtistAlbums.Sort( ( a, b ) => a.Album.Year.CompareTo( b.Album.Year ) ) );
			} );
		}

		/// <summary>
		/// Prepare the combined Artist/ArtistAlbum list from the current Artists list
		/// </summary>
		private static void PrepareCombinedList()
		{
			// Make sure the list is empty - it should be
			ArtistsViewModel.ArtistsAndAlbums.Clear();

			// The simple case is when there is no filter
			if ( ArtistsViewModel.CurrentFilter == null )
			{
				foreach ( Artist artist in ArtistsViewModel.Artists )
				{
					ArtistsViewModel.ArtistsAndAlbums.Add( artist );
					ArtistsViewModel.ArtistsAndAlbums.AddRange( artist.ArtistAlbums );
				}
			}
			else
			{
				// Third time we've done this?
				HashSet<int> albumIds = ArtistsViewModel.CurrentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToHashSet();

				foreach ( Artist artist in ArtistsViewModel.Artists )
				{
					ArtistsViewModel.ArtistsAndAlbums.Add( artist );

					// Only add the ArtistAlbums that are in the filter
					ArtistsViewModel.ArtistsAndAlbums.AddRange( artist.ArtistAlbums.Where( alb => albumIds.Contains( alb.AlbumId ) == true ) );
				}
			}
		}

		/// <summary>
		/// Called when a PlaylistDeletedMessage or PlaylistAddedMessage message has been received
		/// Update the list of playlists held by the model
		/// </summary>
		/// <param name="message"></param>
		private static void PlaylistAddedOrDeleted( object message ) => GetPlayListNames();

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received
		/// If there is no filtering of if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="message"></param>
		private static void TagMembershipChanged( object message )
		{
			if ( ( ArtistsViewModel.CurrentFilter != null ) &&
				 ( ( ArtistsViewModel.TagGroups.Count > 0 ) ||
				   ( ( message as TagMembershipChangedMessage ).ChangedTags.Contains( ArtistsViewModel.CurrentFilter.Name ) == true ) ) )
			{
				ApplyFilterAsync( ArtistsViewModel.CurrentFilter );
			}
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data and the filter and then reload
		/// </summary>
		/// <param name="message"></param>
		private static void SelectedLibraryChanged( object message )
		{
			// Clear the displayed data and filter
			ArtistsViewModel.ClearModel();

			// Publish the data
			Reporter?.ArtistsDataAvailable();

			// Reread the data
			GetArtists( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called when a TagDetailsChangedMessage has been received
		/// If the tag is currently being used to filter the albums then update the filter and
		/// redisplay the albums
		/// </summary>
		/// <param name="message"></param>
		private static void TagDetailsChanged( object message )
		{
			if ( ArtistsViewModel.CurrentFilter != null )
			{
				TagDetailsChangedMessage tagMessage = message as TagDetailsChangedMessage;
				if ( ArtistsViewModel.CurrentFilter.Name == tagMessage.PreviousName )
				{
					ApplyFilterAsync( tagMessage.ChangedTag );
				}
			}
		}

		/// <summary>
		/// Called when a TagDeletedMessage has been received
		/// If the tag is currently being used to filter the albums then remove the filter and redisplay
		/// </summary>
		/// <param name="message"></param>
		private static void TagDeleted( object message )
		{
			if ( ( ArtistsViewModel.CurrentFilter != null ) && ( ArtistsViewModel.CurrentFilter.Name == ( message as TagDeletedMessage ).DeletedTag.Name ) )
			{
				ApplyFilterAsync( null );
			}
		}

		/// <summary>
		/// Called when a AlbumPlayedStateChangedMessage had been received.
		/// If this album is being displayed then inform the adapter of the data change.
		/// To actually determine if the album is being displayed is not possible as we don't have access to that information here.
		/// As an approximation, if any of the Artists that have been expanded contain the album then the adapter will be informed
		/// </summary>
		/// <param name="message"></param>
		private static void AlbumChanged( object message )
		{
			Album changedAlbum = ( message as AlbumPlayedStateChangedMessage ).AlbumChanged;

			// Only process this album if it is in the same library as is being displayed
			// It may be in another library if this is being called as part of a library synchronisation process
			if ( changedAlbum.LibraryId == ArtistsViewModel.LibraryId )
			{
				// Keep track of any changes that need to be displayed
				bool anyChanges = false;

				// Look in each Artist
				List< Artist >.Enumerator enumerator = ArtistsViewModel.Artists.GetEnumerator();
				while ( ( enumerator.MoveNext() == true ) && ( anyChanges == false ) )
				{
					// Only look in this artist if it has been expanded
					if ( enumerator.Current.DetailsRead == true )
					{
						// Look for the changed album
						ArtistAlbum artistAlbum = enumerator.Current.ArtistAlbums.Find( al => al.AlbumId == changedAlbum.Id );
						if ( artistAlbum != null )
						{
							anyChanges = true;
						}
					}
				}

				// Report any changes
				if ( anyChanges == true )
				{
					Reporter?.ArtistsDataAvailable();
				}
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static async void StorageDataAvailable( object message )
		{
			ArtistsViewModel.UnfilteredArtists = Artists.ArtistCollection.Where( art => art.LibraryId == ArtistsViewModel.LibraryId ).ToList();

			// Do the sorting of ArtistAlbum entries off the UI thread
			await SortArtistAlbumsAsync();

			// Apply the current filter and get the data ready for display 
			await ApplyFilterAsync( ArtistsViewModel.CurrentFilter );

			// The data is now valid
			ArtistsViewModel.DataValid = true;
			Reporter?.ArtistsDataAvailable();
		}

		/// <summary>
		/// Get the names of all the user playlists
		/// </summary>
		private static void GetPlayListNames() => 
			ArtistsViewModel.PlaylistNames = Playlists.GetPlaylistsForLibrary(ArtistsViewModel.LibraryId ).Select( list => list.Name).ToList();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void ArtistsDataAvailable();
		}
	}
}