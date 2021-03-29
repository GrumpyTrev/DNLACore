using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	class ArtistsController
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
		}

		/// <summary>
		/// Get the Artist data 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist ) => await theArtist.GetSongsAsync();

		/// <summary>
		/// Get the contents for the specified ArtistAlbum
		/// </summary>
		/// <param name="artistAlbum"></param>
		/// <returns></returns>
		public static async Task GetArtistAlbumContentsAsync( ArtistAlbum artistAlbum ) => await artistAlbum.Artist.GetArtistAlbumSongs( artistAlbum );

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
				// Clear the indexing collections (in case they are not used in the new sort order)
				ArtistsViewModel.FastScrollSections = null;
				ArtistsViewModel.FastScrollSectionLookup = null;

				switch ( ArtistsViewModel.BaseModel.SortSelector.CurrentSortOrder )
				{
					case SortSelector.SortOrder.alphaDescending:
					case SortSelector.SortOrder.alphaAscending:
					{
						if ( ArtistsViewModel.BaseModel.SortSelector.CurrentSortOrder == SortSelector.SortOrder.alphaAscending )
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
						}
						else
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
						}

						// Prepare the combined Artist/ArtistAlbum list - this has to be done after the Artists have been sorted but before the scroll indexing
						PrepareCombinedList();

						// Generate the fast scroll data for alpha sorting
						GenerateIndex( ( artist ) => { return artist.Name.RemoveThe().Substring( 0, 1 ).ToUpper(); } );

						break;
					}

					case SortSelector.SortOrder.idAscending:
					case SortSelector.SortOrder.idDescending:
					{
						if ( ArtistsViewModel.BaseModel.SortSelector.CurrentSortOrder == SortSelector.SortOrder.idAscending )
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Id.CompareTo( b.Id ); } );
						}
						else
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Id.CompareTo( a.Id ); } );
						}

						// Prepare the combined Artist/ArtistAlbum list - this has to be done after the Artists have been sorted.
						// No fast scroll indexing is required for Id sort order
						PrepareCombinedList();

						break;
					}
				}
			} );

			// Publish the data
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static async void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			ArtistsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the Artists we are interested in
			ArtistsViewModel.UnfilteredArtists = Artists.ArtistCollection.Where( art => art.LibraryId == ArtistsViewModel.LibraryId ).ToList();
			
			// Do the sorting of ArtistAlbum entries off the UI thread
			await SortArtistAlbumsAsync();

			// Apply the current filter and get the data ready for display 
			await ApplyFilterAsync();
		}

		/// <summary>
		/// Apply the current filter to the data being displayed
		/// Once the Artists have been filtered prepare them for display by sorting and combinig them with their ArtistAlbum entries
		/// </summary>
		/// <param name="newFilter"></param>
		private static async Task ApplyFilterAsync()
		{
			// alphabetic and identity sorting are available to the user
			ArtistsViewModel.BaseModel.SortSelector.MakeAvailable( new List<SortSelector.SortType> { SortSelector.SortType.alphabetic, SortSelector.SortType.identity } );

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
						ArtistsViewModel.BaseModel.SortSelector.SetActiveSortOrder( SortSelector.SortType.identity );
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
			StorageDataAvailable();
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
				DataReporter?.DataAvailable();
			}
		}
		
		/// <summary>
		/// Generate the fast scroll indexes using the provided function to obtain the section name
		/// The sorted albums have already been moved/copied to the ArtistsViewModel.ArtistsAndAlbums list
		/// </summary>
		/// <param name="sectionNameProvider"></param>
		private static void GenerateIndex( Func<Artist, string> sectionNameProvider )
		{
			// Initialise the index collections - the 
			ArtistsViewModel.FastScrollSections = new List<Tuple<string, int>>();
			ArtistsViewModel.FastScrollSectionLookup = new int[ ArtistsViewModel.ArtistsAndAlbums.Count ];

			// Keep track of when a section has already been added to the FastScrollSections collection
			Dictionary<string, int> sectionLookup = new Dictionary<string, int>();

			// Keep track of the last section index allocated or found for an Artist as it will also be used for the associated ArtistAlbum entries
			int sectionIndex = -1;

			int index = 0;
			foreach ( object artistOrAlbum in ArtistsViewModel.ArtistsAndAlbums )
			{
				// Only add section names for Artists, not ArtistAlbums
				if ( artistOrAlbum is Artist artist )
				{
					// If this is the first occurrence of the section name then add it to the FastScrollSections collection together with the index 
					string sectionName = sectionNameProvider( artist );
					sectionIndex = sectionLookup.GetValueOrDefault( sectionName, -1 );
					if ( sectionIndex == -1 )
					{
						sectionIndex = sectionLookup.Count;
						sectionLookup[ sectionName ] = sectionIndex;
						ArtistsViewModel.FastScrollSections.Add( new Tuple<string, int>( sectionName, index ) );
					}
				}

				// Provide a quick section lookup for this entry
				ArtistsViewModel.FastScrollSectionLookup[ index++ ] = sectionIndex;
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static DataReporter.IReporter DataReporter
		{
			private get => dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}