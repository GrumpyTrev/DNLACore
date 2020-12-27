using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	class ArtistsController : BaseController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static ArtistsController()
		{
			Mediator.RegisterPermanent( TagMembershipChanged, typeof( TagMembershipChangedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( TagDetailsChanged, typeof( TagDetailsChangedMessage ) );
			Mediator.RegisterPermanent( TagDeleted, typeof( TagDeletedMessage ) );
			Mediator.RegisterPermanent( AlbumChanged, typeof( AlbumPlayedStateChangedMessage ) );

			instance = new ArtistsController(); 
		}

		/// <summary>
		/// Get the Artist data 
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist ) => await theArtist.GetSongsAsync();

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="playlist"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, Playlist playlist ) => playlist.AddSongs( songsToAdd );

		/// <summary>
		/// Apply the specified filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public static void SetNewFilter( Tag newFilter )
		{
			// Update the model
			ArtistsViewModel.FilterSelector.CurrentFilter = newFilter;

			// No need to wait for this to be applied
			ApplyFilterAsync();
		}

		/// <summary>
		/// Sort the Artists according to the currently selected sort order
		/// </summary>
		public static async Task SortArtistsAsync()
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

			// Publish the data
			instance.Reporter?.DataAvailable();
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		protected override async void StorageDataAvailable( object _ = null )
		{
			// Save the libray being used locally to detect changes
			ArtistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the Artists we are interested in
			ArtistsViewModel.UnfilteredArtists = Artists.ArtistCollection.Where( art => art.LibraryId == ArtistsViewModel.LibraryId ).ToList();

			// Do the sorting of ArtistAlbum entries off the UI thread
			await SortArtistAlbumsAsync();

			// Apply the current filter and get the data ready for display 
			await ApplyFilterAsync();

			// Call the base class
			base.StorageDataAvailable();
		}

		/// <summary>
		/// Apply the current filter to the data being displayed
		/// Once the Artists have been filtered prepare them for display by sorting and combinig them with their ArtistAlbum entries
		/// </summary>
		/// <param name="newFilter"></param>
		private static async Task ApplyFilterAsync()
		{
			// alphabetic and identity sorting are available to the user
			ArtistsViewModel.SortSelector.MakeAvailable( new List<SortSelector.SortType> { SortSelector.SortType.alphabetic, SortSelector.SortType.identity } );

			// Check for no simple or group tag filters
			if ( ArtistsViewModel.FilterSelector.FilterApplied == false )
			{
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists;
			}
			else
			{
				await Task.Run( () =>
				{
					// Combine the simple and group tabs
					ArtistsViewModel.FilteredAlbumsIds = ArtistsViewModel.FilterSelector.CombineAlbumFilters();

					// Now get all the artist identities of the albums that are tagged
					HashSet<int> artistIds = ArtistAlbums.ArtistAlbumCollection.
						Where( aa => ArtistsViewModel.FilteredAlbumsIds.Contains( aa.AlbumId ) ).Select( aa => aa.ArtistId ).Distinct().ToHashSet();

					// Now get the Artists from the list of artist ids
					ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists.Where( art => artistIds.Contains( art.Id ) ).ToList();

					// If the TagOrder flag is set then set the sort order to Id order.
					if ( ArtistsViewModel.FilterSelector.TagOrderFlag == true )
					{
						ArtistsViewModel.SortSelector.SetActiveSortOrder( SortSelector.SortType.identity );
					}
				} );
			}

			// Sort the artists to the order specified in the SortSelector and publish the data
			await SortArtistsAsync();
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

			// These have already been filtered
			foreach ( Artist artist in ArtistsViewModel.Artists )
			{
				ArtistsViewModel.ArtistsAndAlbums.Add( artist );

				// If there is no filter add all the albums, otherwise only add the albums that are in the filter
				ArtistsViewModel.ArtistsAndAlbums.AddRange( ( ArtistsViewModel.FilterSelector.FilterApplied == false ) ? artist.ArtistAlbums :
					artist.ArtistAlbums.Where( alb => ArtistsViewModel.FilteredAlbumsIds.Contains( alb.AlbumId ) == true ) );
			}
		}

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received
		/// If there is no filtering or if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="message"></param>
		private static void TagMembershipChanged( object message )
		{
			if ( ArtistsViewModel.FilterSelector.FilterContainsTags( ( ( TagMembershipChangedMessage )message ).ChangedTags ) == true )
			{
				// Reapply the same filter. No need to wait for this.
				ApplyFilterAsync();
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

			// Reload the library specific artist data
			instance.dataValid = false;
			instance.StorageDataAvailable();
		}

		/// <summary>
		/// Called when a TagDetailsChangedMessage has been received
		/// If the tag is currently being used to filter the albums then update the filter and
		/// redisplay the albums
		/// </summary>
		/// <param name="message"></param>
		private static void TagDetailsChanged( object message )
		{
			if ( ArtistsViewModel.FilterSelector.CurrentFilterName == ( ( TagDetailsChangedMessage )message ).ChangedTag.Name )
			{
				// Reapply the same filter
				ApplyFilterAsync();
			}
		}

		/// <summary>
		/// Called when a TagDeletedMessage has been received
		/// If the tag is currently being used to filter the albums then remove the filter and redisplay
		/// </summary>
		/// <param name="message"></param>
		private static void TagDeleted( object message )
		{
			if ( ArtistsViewModel.FilterSelector.CurrentFilterName == ( message as TagDeletedMessage ).DeletedTag.Name )
			{
				SetNewFilter( null );
			}
		}

		/// <summary>
		/// Called when a AlbumPlayedStateChangedMessage had been received.
		/// If the album is in the library being displayed then refresh the display
		/// </summary>
		/// <param name="message"></param>
		private static void AlbumChanged( object message )
		{
			Album changedAlbum = ( message as AlbumPlayedStateChangedMessage ).AlbumChanged;

			// Only process this album if it is in the same library as is being displayed
			// It may be in another library if this is being called as part of a library synchronisation process
			if ( changedAlbum.LibraryId == ArtistsViewModel.LibraryId )
			{
				instance.Reporter?.DataAvailable();
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter DataReporter
		{
			set => instance.Reporter = value;
		}

		/// <summary>
		/// The one and only ArtistsController instance
		/// </summary>
		private static readonly ArtistsController instance = null;
	}
}