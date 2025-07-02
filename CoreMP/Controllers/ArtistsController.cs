using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	public class ArtistsController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// Register for the main data available event.
		/// </summary>
		public ArtistsController() => NotificationHandler.Register<StorageController>( () =>
		{
			StorageDataAvailable();

			// Once data is available register for library change messages
			NotificationHandler.Register<Playback>( nameof( Playback.LibraryIdentity ), () =>
			{
				// Clear the displayed data and filter
				ArtistsViewModel.ClearModel();

				// Reload the library specific album data
				StorageDataAvailable();
			} );

			NotificationHandler.Register<TagModel>( ( tagName ) => TagMembershipChanged( ( string )tagName ) );
		} );

		/// <summary>
		/// Apply the specified filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public void SetNewFilter( Tag newFilter )
		{
			// Update the model
			ArtistsViewModel.FilterSelection.CurrentFilter = newFilter;

			// No need to wait for this to be applied
			ApplyFilterAndSortSelections();
		}

		public void SortArtists() => Task.Run( () =>
		{
			ApplySortSelection();

			// Publish the data
			ArtistsViewModel.Available.IsSet = true;
		} );

		/// <summary>
		/// Apply the current filter and sort order to the data
		/// </summary>
		private void ApplyFilterAndSortSelections() => Task.Run( () =>
		{
			// Apply the filter specified in the FilterSelector
			ApplyFilterSelection();

			// Sort the displayed albums to the order specified in the SortSelector
			ApplySortSelection();

			// Publish the data
			ArtistsViewModel.Available.IsSet = true;
		} );

		/// <summary>
		/// Sort the Artists according to the currently selected sort order
		/// </summary>
		private void ApplySortSelection()
		{
			switch ( ArtistsViewModel.SortSelection.CurrentSortOrder )
			{
				case SortOrder.alphaDescending:
				case SortOrder.alphaAscending:
				{
					if ( ArtistsViewModel.SortSelection.CurrentSortOrder == SortOrder.alphaAscending )
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ) );
					}
					else
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ) );
					}

					// Prepare the combined Artist/ArtistAlbum list - this has to be done after the Artists have been sorted
					PrepareCombinedList();

					break;
				}

				case SortOrder.idAscending:
				case SortOrder.idDescending:
				{
					if ( ArtistsViewModel.SortSelection.CurrentSortOrder == SortOrder.idAscending )
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => a.Id.CompareTo( b.Id ) );
					}
					else
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => b.Id.CompareTo( a.Id ) );
					}

					// Prepare the combined Artist/ArtistAlbum list - this has to be done after the Artists have been sorted.
					PrepareCombinedList();

					break;
				}
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			ArtistsViewModel.LibraryId = Playback.LibraryIdentity;

			// Get the Artists we are interested in
			ArtistsViewModel.UnfilteredArtists = Artists.ArtistCollection.Where( art => art.LibraryId == Playback.LibraryIdentity ).ToList();

			// Do the sorting of ArtistAlbum entries off the UI thread
			SortArtistAlbums();

			// Apply the current filter and get the data ready for display 
			ApplyFilterAndSortSelections();
		}

		/// <summary>
		/// Apply the current filter to the data being displayed
		/// Once the Artists have been filtered prepare them for display by sorting and combinig them with their ArtistAlbum entries
		/// </summary>
		/// <param name="newFilter"></param>
		private void ApplyFilterSelection()
		{
			// alphabetic and identity sorting are available to the user
			ArtistsViewModel.SortSelection.MakeAvailable( new List<SortType> { SortType.alphabetic, SortType.identity } );

			// Check for no simple or group tag filters
			if ( ArtistsViewModel.FilterSelection.FilterApplied == false )
			{
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists;
			}
			else
			{
				// Combine the simple and group tabs
				ArtistsViewModel.FilteredAlbumsIds = ArtistsViewModel.FilterSelection.CombineAlbumFilters();

				// Now get all the artist identities of the albums that are tagged
				HashSet<int> artistIds = ArtistAlbums.ArtistAlbumCollection.
					Where( aa => ArtistsViewModel.FilteredAlbumsIds.Contains( aa.AlbumId ) ).Select( aa => aa.ArtistId ).Distinct().ToHashSet();

				// Now get the Artists from the list of artist ids
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists.Where( art => artistIds.Contains( art.Id ) ).ToList();

				// If the TagOrder flag is set then set the sort order to Id order.
				if ( ArtistsViewModel.FilterSelection.TagOrderFlag == true )
				{
					ArtistsViewModel.SortSelection.ActiveSortType = SortType.identity;
				}
			}
		}

		/// <summary>
		/// Sort the ArtistAlbum entries in each Artist by the album year
		/// </summary>
		private void SortArtistAlbums() => Task.Run( () =>
			ArtistsViewModel.UnfilteredArtists.ForEach( art => art.ArtistAlbums.Sort( ( a, b ) => a.Album.Year.CompareTo( b.Album.Year ) ) ) );

		/// <summary>
		/// Prepare the combined Artist/ArtistAlbum list from the current Artists list
		/// </summary>
		private void PrepareCombinedList()
		{
			// Make sure the list is empty - it should be
			ArtistsViewModel.ArtistsAndAlbums.Clear();

			// These have already been filtered
			foreach ( Artist artist in ArtistsViewModel.Artists )
			{
				ArtistsViewModel.ArtistsAndAlbums.Add( artist );

				// If there is no filter add all the albums, otherwise only add the albums that are in the filter
				ArtistsViewModel.ArtistsAndAlbums.AddRange( ( ArtistsViewModel.FilterSelection.FilterApplied == false ) ? artist.ArtistAlbums :
					artist.ArtistAlbums.Where( alb => ArtistsViewModel.FilteredAlbumsIds.Contains( alb.AlbumId ) == true ) );
			}
		}

		/// <summary>
		/// Called when a tag has changed.
		/// If there is no filtering or if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="message"></param>
		private void TagMembershipChanged( string changedTag )
		{
			if ( ArtistsViewModel.FilterSelection.FilterContainsTag( changedTag ) == true )
			{
				// Reapply the filter and sort selections
				ApplyFilterAndSortSelections();
			}
		}
	}
}
