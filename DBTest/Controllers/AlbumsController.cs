using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AlbumsController is the Controller for the AlbumsView. It responds to AlbumsView commands and maintains Albums data in the
	/// AlbumsViewModel
	/// </summary>
	class AlbumsController : BaseController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static AlbumsController()
		{
			Mediator.RegisterPermanent( TagMembershipChanged, typeof( TagMembershipChangedMessage ) );
			Mediator.RegisterPermanent( SelectedLibraryChanged, typeof( SelectedLibraryChangedMessage ) );
			Mediator.RegisterPermanent( TagDetailsChanged, typeof( TagDetailsChangedMessage ) );
			Mediator.RegisterPermanent( TagDeleted, typeof( TagDeletedMessage ) );
			Mediator.RegisterPermanent( AlbumChanged, typeof( AlbumPlayedStateChangedMessage ) );

			instance = new AlbumsController();
		}

		/// <summary>
		/// Get the Controller data
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Get the contents for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static async Task GetAlbumContentsAsync( Album theAlbum )
		{
			await theAlbum.GetSongsAsync();

			// Sort the songs by track number - UI thread but not many entries
			theAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="playlist"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, Playlist playlist ) => playlist.AddSongs( songsToAdd );

		/// <summary>
		/// Wrapper around ApplyFilterAsync to match delegate signature
		/// </summary>
		/// <param name="newFilter"></param>
		/// <returns></returns>
		public static async Task ApplyFilterDelegateAsync( Tag newFilter ) => await ApplyFilterAsync( newFilter );

		/// <summary>
		/// Sort the available data according to the current sort option
		/// </summary>
		public static async Task SortDataAsync( bool refreshData = false )
		{
			// Do the sorting and indexing off the UI task
			await Task.Run( () => 
			{
				// Use the sort order stored in the model
				SortSelector.SortOrder sortOrder = AlbumsViewModel.SortSelector.CurrentSortOrder;

				switch ( sortOrder )
				{
					case SortSelector.SortOrder.alphaAscending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
						break;
					}

					case SortSelector.SortOrder.alphaDescending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
						break;
					}

					case SortSelector.SortOrder.idAscending:
					case SortSelector.SortOrder.idDescending:
					{
						// If these entries are filtered then order them by the tag id rather than the album id
						if ( AlbumsViewModel.CurrentFilter == null )
						{
							if ( sortOrder == SortSelector.SortOrder.idAscending )
							{
								AlbumsViewModel.Albums.Sort( ( a, b ) => { return a.Id.CompareTo( b.Id ); } );
							}
							else
							{
								// Reverse the albums
								AlbumsViewModel.Albums.Sort( ( a, b ) => { return b.Id.CompareTo( a.Id ); } );
							}
						}
						else
						{
							// Form a list of all album ids in the same order as they are in the tag
							List<int> albumIds = AlbumsViewModel.CurrentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToList();

							if ( sortOrder == SortSelector.SortOrder.idDescending )
							{
								albumIds.Reverse();
							}

							// Order the albums by the album id list
							AlbumsViewModel.Albums = AlbumsViewModel.Albums.OrderBy( album => albumIds.IndexOf( album.Id ) ).ToList();
						}
						break;
					}

					case SortSelector.SortOrder.yearAscending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return a.Year.CompareTo( b.Year ); } );
						break;
					}

					case SortSelector.SortOrder.yearDescending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return b.Year.CompareTo( a.Year ); } );
						break;
					}

					case SortSelector.SortOrder.genreAscending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return a.Genre.CompareTo( b.Genre ); } );
						break;
					}

					case SortSelector.SortOrder.genreDescending:
					{
						AlbumsViewModel.Albums.Sort( ( a, b ) => { return b.Genre.CompareTo( a.Genre ); } );
						break;
					}
				}
			} );

			if ( refreshData == true )
			{
				// Publish the data
				instance.Reporter?.DataAvailable();
			}
		}

		/// <summary>
		/// Called when the Album data has been read in from storage
		/// </summary>
		/// <param name="message"></param>
		protected override async void StorageDataAvailable( object _ = null )
		{
			// Save the libray being used locally to detect changes
			AlbumsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			AlbumsViewModel.UnfilteredAlbums = Albums.AlbumCollection.Where( alb => alb.LibraryId == AlbumsViewModel.LibraryId ).ToList();

			// Revert to no filter and sort the data
			await ApplyFilterAsync( null, false );

			base.StorageDataAvailable();
		}

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		private static async Task ApplyFilterAsync( Tag newFilter, bool report = true )
		{
			// Update the model
			AlbumsViewModel.CurrentFilter = newFilter;

			// Make all sort orders available
			AlbumsViewModel.SortSelector.MakeAvailable( new List<SortSelector.SortType> { SortSelector.SortType.alphabetic, SortSelector.SortType.identity,
					SortSelector.SortType.year, SortSelector.SortType.genre } );

			// Check for no simple or group tag filters
			if ( ( AlbumsViewModel.CurrentFilter == null ) && ( AlbumsViewModel.TagGroups.Count == 0 ) )
			{
				AlbumsViewModel.Albums = new List<Album>( AlbumsViewModel.UnfilteredAlbums );
			}
			else
			{
				await Task.Run( () =>
				{
					// Combine the simple and group tabs
					HashSet<int> albumIds = BaseController.CombineAlbumFilters( AlbumsViewModel.CurrentFilter, AlbumsViewModel.TagGroups );

					// Now get all the albums that are tagged and in the current library
					AlbumsViewModel.Albums = AlbumsViewModel.UnfilteredAlbums.FindAll( album => albumIds.Contains( album.Id ) == true );

					// If the TagOrder flag is set then set the sort order to Id order.
					if ( ( AlbumsViewModel.CurrentFilter?.TagOrder ?? false ) == true )
					{
						AlbumsViewModel.SortSelector.SetActiveSortOrder( SortSelector.SortType.identity );
					}
				} );
			}

			// Sort the displayed albums to the order specified in the SortSelector
			await SortDataAsync();

			// Publish the data
			if ( report == true )
			{
				instance.Reporter?.DataAvailable();
			}
		}

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received
		/// If there is no filtering of if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="message"></param>
		private static void TagMembershipChanged( object message )
		{
			if ( ( AlbumsViewModel.CurrentFilter != null ) &&
				 ( ( AlbumsViewModel.TagGroups.Count > 0 ) ||
				   ( ( message as TagMembershipChangedMessage ).ChangedTags.Contains( AlbumsViewModel.CurrentFilter.Name ) == true ) ) )
			{
				ApplyFilterAsync( AlbumsViewModel.CurrentFilter );
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
			AlbumsViewModel.ClearModel();

			// Reload the library specific album data
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
			if ( AlbumsViewModel.CurrentFilter != null )
			{
				TagDetailsChangedMessage tagMessage = message as TagDetailsChangedMessage;
				if ( AlbumsViewModel.CurrentFilter.Name == tagMessage.PreviousName )
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
			if ( ( AlbumsViewModel.CurrentFilter != null ) && ( AlbumsViewModel.CurrentFilter.Name == ( message as TagDeletedMessage ).DeletedTag.Name ) )
			{
				ApplyFilterAsync( null );
			}
		}

		/// <summary>
		/// Called when a AlbumPlayedStateChangedMessage had been received.
		/// If this album is being displayed then inform the adapter of the data change
		/// </summary>
		/// <param name="message"></param>
		private static void AlbumChanged( object message )
		{
			Album changedAlbum = ( message as AlbumPlayedStateChangedMessage ).AlbumChanged;

			// Only process this if this album is in the library being displayed
			if ( changedAlbum.LibraryId == AlbumsViewModel.LibraryId )
			{
				// Is this album being displayed
				if ( AlbumsViewModel.Albums.Any( album => album.Id == changedAlbum.Id ) == true )
				{
					instance.Reporter?.DataAvailable();
				}
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
		/// The one and only AlbumsController instance
		/// </summary>
		private static readonly AlbumsController instance = null;
	}
}