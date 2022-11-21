using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The AlbumsController is the Controller for the AlbumsView. It responds to AlbumsView commands and maintains Albums data in the
	/// AlbumsViewModel
	/// </summary>
	internal class AlbumsController
	{
		/// <summary>
		/// Public constructor to allow message registrations
		/// </summary>
		public AlbumsController()
		{
			TagMembershipChangedMessage.Register( TagMembershipChanged );

			// Register for the main data available event.
			NotificationHandler.Register( typeof( StorageController ), () => 
			{ 
				StorageDataAvailable();

				// Once data is available register for library change messages
				NotificationHandler.Register( typeof( ConnectionDetailsModel ), () =>
				{
					// Clear the displayed data and filter
					AlbumsViewModel.ClearModel();

					// Reload the library specific album data
					StorageDataAvailable();
				} );
			} );
		}

		/// <summary>
		/// Apply the specified filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public void SetNewFilter( Tag newFilter )
		{
			// Update the model
			AlbumsViewModel.FilterSelection.CurrentFilter = newFilter;

			// Apply the changes
			ApplyFilterAndSortSelections();
		}

		/// <summary>
		/// Sort the Album data and publish it
		/// </summary>
		public void SortData() => Task.Run( () =>
		{
			ApplySortSelection();

			// Publish the data
			AlbumsViewModel.Available.IsSet = true;
		} );

		/// <summary>
		/// Called when the Album data has been read in from storage
		/// </summary>
		/// <param name="message"></param>
		private void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			AlbumsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get all the albums associated with the library
			AlbumsViewModel.UnfilteredAlbums = Albums.AlbumCollection.Where( alb => alb.LibraryId == AlbumsViewModel.LibraryId ).ToList();

			// Apply the current filter and sort selections
			ApplyFilterAndSortSelections();
		}

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
			AlbumsViewModel.Available.IsSet = true;
		} );

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		private void ApplyFilterSelection()
		{
			// Make all sort types available
			AlbumsViewModel.SortSelection.MakeAvailable( new List<SortType> { SortType.alphabetic, SortType.identity, SortType.year, SortType.genre } );

			// Clear the Genre sorted albums list. It will only be set if a Genre sort is applied
			AlbumsViewModel.GenreSortedAlbums = null;

			// Check for no simple or group tag filters
			if ( AlbumsViewModel.FilterSelection.FilterApplied == false )
			{
				AlbumsViewModel.FilteredAlbums = AlbumsViewModel.UnfilteredAlbums.ToList();
			}
			else
			{
				// Combine the simple and group tabs
				HashSet<int> albumIds = AlbumsViewModel.FilterSelection.CombineAlbumFilters();

				// Now get all the albums in the current library that are tagged
				AlbumsViewModel.FilteredAlbums = AlbumsViewModel.UnfilteredAlbums.Where( album => albumIds.Contains( album.Id ) == true ).ToList();

				// If the TagOrder flag is set then set the sort order to Id order.
				if ( AlbumsViewModel.FilterSelection.TagOrderFlag == true )
				{
					AlbumsViewModel.SortSelection.ActiveSortType = SortType.identity;
				}
			}
		}

		/// <summary>
		/// Sort the available data according to the current sort option
		/// </summary>
		private void ApplySortSelection()
		{
			// Use the sort order stored in the model
			SortOrder sortOrder = AlbumsViewModel.SortSelection.CurrentSortOrder;

			// Now do the sorting according to the sort order
			switch ( sortOrder )
			{
				case SortOrder.alphaAscending:
				case SortOrder.alphaDescending:
				{
					if ( sortOrder == SortOrder.alphaAscending )
					{
						AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ) );
					}
					else
					{
						AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ) );
					}

					AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
					break;
				}

				case SortOrder.idAscending:
				case SortOrder.idDescending:
				{
					// If these entries are filtered then order them by the tag id rather than the album id
					if ( AlbumsViewModel.FilterSelection.CurrentFilter == null )
					{
						if ( sortOrder == SortOrder.idAscending )
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => a.Id.CompareTo( b.Id ) );
						}
						else
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => b.Id.CompareTo( a.Id ) );
						}
					}
					else
					{
						// Form a lookup table from album identity to index in tagged albums.
						Dictionary<int, int> albumIdLookup = AlbumsViewModel.FilterSelection.CurrentFilter.TaggedAlbums
							.Select( ( ta, index ) => new { ta.AlbumId, index } ).ToDictionary( pair => pair.AlbumId, pair => pair.index );

						// Order the albums by the album id list
						if ( sortOrder == SortOrder.idAscending )
						{
							AlbumsViewModel.FilteredAlbums = AlbumsViewModel.FilteredAlbums.OrderBy( album => albumIdLookup[ album.Id ] ).ToList();
						}
						else
						{
							AlbumsViewModel.FilteredAlbums = AlbumsViewModel.FilteredAlbums.OrderByDescending( album => albumIdLookup[ album.Id ] ).ToList();
						}
					}

					AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
					break;
				}

				case SortOrder.yearAscending:
				case SortOrder.yearDescending:
				{
					if ( sortOrder == SortOrder.yearAscending )
					{
						AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => a.Year.CompareTo( b.Year ) );
					}
					else
					{
						AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => b.Year.CompareTo( a.Year ) );
					}

					AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
					break;
				}

				case SortOrder.genreAscending:
				case SortOrder.genreDescending:
				{
					// If there is no GenreSortAlbums collection then make one now
					if ( AlbumsViewModel.GenreSortedAlbums == null )
					{
						GenerateGenreAlbumList();
					}

					// We want to keep the AlbumsViewModel.GenreSortedAlbums in ascending order.
					// So rather than sort the AlbumsViewModel.GenreSortedAlbums we copy it and sort the copy.
					AlbumsViewModel.Albums = AlbumsViewModel.GenreSortedAlbums.ToList();

					// We only need to sort if the order is descending
					if ( sortOrder == SortOrder.genreDescending )
					{
						// Reverse it
						AlbumsViewModel.Albums.Reverse();
					}

					break;
				}
			}
		}

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received.
		/// If the CurrentFilter has been changed or if the TagGroups contains this tag then the data must be refreshed
		/// If there is no filtering of if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="changedTags"></param>
		private void TagMembershipChanged( List< string > changedTags )
		{
			if ( AlbumsViewModel.FilterSelection.FilterContainsTags( changedTags ) == true )
			{
				// Reapply the filter and sort selections
				ApplyFilterAndSortSelections();
			}
		}

		/// <summary>
		/// Form a new Albums collection where albums with multiple genres are given multiple entries. The Albums will be in genre order
		/// </summary>
		private void GenerateGenreAlbumList()
		{
			// This is the album list that we'll be generating
			AlbumsViewModel.GenreSortedAlbums = new List<Album>();

			// This genre list is generated alongside the main Album list
			AlbumsViewModel.AlbumIndexToGenreLookup = new List<string>();

			// We need a lookup table for all the Albums in the current filtered Album list
			Dictionary<int, Album> albumIds = AlbumsViewModel.FilteredAlbums.ToDictionary( alb => alb.Id );

			// The Albums need to be sorted in genre order. If there is no genre filter then use all the genre tags, otherwise just use
			// the tags in the filter
			TagGroup genreTags = AlbumsViewModel.FilterSelection.TagGroups.SingleOrDefault( ta => ta.Name == "Genre" ) ?? FilterManagementModel.GenreTags;

			// Get the Genre GroupTag and order the Tags by name. Copy the list so that we don't change it
			List <Tag> sortedTags = genreTags.Tags.ToList();
			sortedTags.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			// Use the Genre GroupTag to order the Album entries
			foreach ( Tag genreTag in sortedTags )
			{
				// For each TaggedAlbum in this tag that refers to an Album in the Album list add a new entry to the new Genre album list
				foreach ( TaggedAlbum taggedAlbum in genreTag.TaggedAlbums )
				{
					Album genreAlbum = albumIds.GetValueOrDefault( taggedAlbum.AlbumId );
					if ( genreAlbum != null )
					{
						AlbumsViewModel.AlbumIndexToGenreLookup.Add( genreTag.Name );
						AlbumsViewModel.GenreSortedAlbums.Add( genreAlbum );
					}
				}
			}
		}
	}
}
