using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AlbumsController is the Controller for the AlbumsView. It responds to AlbumsView commands and maintains Albums data in the
	/// AlbumsViewModel
	/// </summary>
	class AlbumsController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static AlbumsController()
		{
			TagMembershipChangedMessage.Register( TagMembershipChanged );
			SelectedLibraryChangedMessage.Register( SelectedLibraryChanged );
			TagDetailsChangedMessage.Register( TagDetailsChanged );
			TagDeletedMessage.Register( TagDeleted );
			AlbumPlayedStateChangedMessage.Register( AlbumChanged );
		}

		/// <summary>
		/// Get the Controller data
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Get the contents for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static void GetAlbumContents( Album theAlbum ) => theAlbum.GetSongs();

		/// <summary>
		/// Apply the specified filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public static void SetNewFilter( Tag newFilter )
		{
			// Update the model
			AlbumsViewModel.FilterSelector.CurrentFilter = newFilter;

			// No need to wait for this to be applied
			ApplyFilterAsync();
		}

		/// <summary>
		/// Sort the available data according to the current sort option
		/// </summary>
		public static async Task SortDataAsync()
		{
			// Do the sorting and indexing off the UI task
			await Task.Run( () => 
			{
				// Use the sort order stored in the model
				SortSelector.SortOrder sortOrder = AlbumsViewModel.BaseModel.SortSelector.CurrentSortOrder;

				// Clear the indexing collections (in case they are not used in the new sort order)
				AlbumsViewModel.FastScrollSections = null;
				AlbumsViewModel.FastScrollSectionLookup = null;

				// Now do the sorting and indexing according to the sort order
				switch ( sortOrder )
				{
					case SortSelector.SortOrder.alphaAscending:
					case SortSelector.SortOrder.alphaDescending:
					{
						if ( sortOrder == SortSelector.SortOrder.alphaAscending )
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
						}
						else
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
						}

						AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
						GenerateIndex( ( album, index ) => { return album.Name.RemoveThe().Substring( 0, 1 ).ToUpper(); } );
						break;
					}

					case SortSelector.SortOrder.idAscending:
					case SortSelector.SortOrder.idDescending:
					{
						// If these entries are filtered then order them by the tag id rather than the album id
						if ( AlbumsViewModel.FilterSelector.CurrentFilter == null )
						{
							if ( sortOrder == SortSelector.SortOrder.idAscending )
							{
								AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return a.Id.CompareTo( b.Id ); } );
							}
							else
							{
								AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return b.Id.CompareTo( a.Id ); } );
							}
						}
						else
						{
							// Form a lookup table from album identity to index in tagged albums.
							Dictionary<int, int> albumIdLookup = AlbumsViewModel.FilterSelector.CurrentFilter.TaggedAlbums
								.Select( ( ta, index ) => new { ta.AlbumId, index }).ToDictionary( pair => pair.AlbumId, pair => pair.index );

							// Order the albums by the album id list
							if ( sortOrder == SortSelector.SortOrder.idAscending )
							{
								AlbumsViewModel.FilteredAlbums = AlbumsViewModel.FilteredAlbums.OrderBy( album => albumIdLookup[ album.Id ] ).ToList();
							}
							else
							{
								AlbumsViewModel.FilteredAlbums = AlbumsViewModel.FilteredAlbums.OrderByDescending( album => albumIdLookup[ album.Id ] ).ToList();
							}
						}

						// No index required when sorted by Id
						AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
						break;
					}

					case SortSelector.SortOrder.yearAscending:
					case SortSelector.SortOrder.yearDescending:
					{
						if ( sortOrder == SortSelector.SortOrder.yearAscending )
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return a.Year.CompareTo( b.Year ); } );
						}
						else
						{
							AlbumsViewModel.FilteredAlbums.Sort( ( a, b ) => { return b.Year.CompareTo( a.Year ); } );
						}

						AlbumsViewModel.Albums = AlbumsViewModel.FilteredAlbums;
						GenerateIndex( ( album, index ) => { return album.Year.ToString(); } );
						break;
					}

					case SortSelector.SortOrder.genreAscending:
					case SortSelector.SortOrder.genreDescending:
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
						if ( sortOrder == SortSelector.SortOrder.genreDescending )
						{
							// Reverse it
							AlbumsViewModel.Albums.Reverse();
						}

						// Generate the fast lookup indexes. If in reverse order do the genre lookup in reverse order as well
						GenerateIndex( ( album, index ) => { return AlbumsViewModel.AlbumIndexToGenreLookup[
							sortOrder == SortSelector.SortOrder.genreAscending ? index : AlbumsViewModel.Albums.Count - 1 - index ]; } );

						break;
					}
				}
			} );

			// Publish the data
			DataReporter?.DataAvailable();
		}

		/// <summary>
		/// Called when the Album data has been read in from storage
		/// </summary>
		/// <param name="message"></param>
		private static async void StorageDataAvailable()
		{
			// Save the libray being used locally to detect changes
			AlbumsViewModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get all the albums associated with the library
			AlbumsViewModel.UnfilteredAlbums = Albums.AlbumCollection.Where( alb => alb.LibraryId == AlbumsViewModel.LibraryId ).ToList();

			// Apply the current filter
			await ApplyFilterAsync();
		}

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		private static async Task ApplyFilterAsync()
		{
			// Make all sort orders available
			AlbumsViewModel.BaseModel.SortSelector.MakeAvailable( new List<SortSelector.SortType> { SortSelector.SortType.alphabetic, SortSelector.SortType.identity,
					SortSelector.SortType.year, SortSelector.SortType.genre } );

			// Clear the Genre sorted albums list. It will only be set if a Genre sort is applied
			AlbumsViewModel.GenreSortedAlbums = null;

			// Check for no simple or group tag filters
			if ( AlbumsViewModel.FilterSelector.FilterApplied == false )
			{
				AlbumsViewModel.FilteredAlbums = AlbumsViewModel.UnfilteredAlbums.ToList();
			}
			else
			{
				await Task.Run( () =>
				{
					// Combine the simple and group tabs
					HashSet<int> albumIds = AlbumsViewModel.FilterSelector.CombineAlbumFilters();

					// Now get all the albums in the current library that are tagged
					AlbumsViewModel.FilteredAlbums = AlbumsViewModel.UnfilteredAlbums.Where( album => albumIds.Contains( album.Id ) == true ).ToList();

					// If the TagOrder flag is set then set the sort order to Id order.
					if ( AlbumsViewModel.FilterSelector.TagOrderFlag == true )
					{
						AlbumsViewModel.BaseModel.SortSelector.SetActiveSortOrder( SortSelector.SortType.identity );
					}
				} );
			}

			// Sort the displayed albums to the order specified in the SortSelector
			await SortDataAsync();
		}

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received.
		/// If the CurrentFilter has been changed or if the TagGroups contains this tag then the data must be refreshed
		/// If there is no filtering of if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="changedTags"></param>
		private static void TagMembershipChanged( List< string > changedTags )
		{
			if ( AlbumsViewModel.FilterSelector.FilterContainsTags( changedTags ) == true )
			{
				// Reapply the same filter. No need to wait for this.
				ApplyFilterAsync();
			}
		}

		/// <summary>
		/// Called when a SelectedLibraryChangedMessage has been received
		/// Clear the current data and the filter and then reload
		/// </summary>
		/// <param name="_"></param>
		private static void SelectedLibraryChanged( int _ )
		{
			// Clear the displayed data and filter
			AlbumsViewModel.ClearModel();

			// Reload the library specific album data
			StorageDataAvailable();
		}

		/// <summary>
		/// Called when a TagDetailsChangedMessage has been received
		/// If the tag is currently being used to filter the albums then update the filter and
		/// redisplay the albums
		/// </summary>
		/// <param name="message"></param>
		private static void TagDetailsChanged( Tag changedTag )
		{
			if ( AlbumsViewModel.FilterSelector.CurrentFilterName == changedTag.Name )
			{
				// Reapply the same filter. No need to wait for this
				ApplyFilterAsync();
			}
		}

		/// <summary>
		/// Called when a TagDeletedMessage has been received
		/// If the tag is currently being used to filter the albums then remove the filter and redisplay
		/// </summary>
		/// <param name="message"></param>
		private static void TagDeleted( Tag deletedTag )
		{
			if ( AlbumsViewModel.FilterSelector.CurrentFilterName == deletedTag.Name )
			{
				SetNewFilter( null );
			}
		}

		/// <summary>
		/// Called when a AlbumPlayedStateChangedMessage had been received.
		/// If this album is being displayed then inform the adapter of the data change
		/// </summary>
		/// <param name="changedAlbum"></param>
		private static void AlbumChanged( Album changedAlbum )
		{
			// Only process this if this album is in the library being displayed
			if ( changedAlbum.LibraryId == AlbumsViewModel.LibraryId )
			{
				// Is this album being displayed
				if ( AlbumsViewModel.Albums.Any( album => album.Id == changedAlbum.Id ) == true )
				{
					DataReporter?.DataAvailable();
				}
			}
		}

		/// <summary>
		/// Form a new Albums collection where albums with multiple genres are given multiple entries. The Albums will be in genre order
		/// </summary>
		private static void GenerateGenreAlbumList()
		{
			// This is the album list that we'll be generating
			AlbumsViewModel.GenreSortedAlbums = new List<Album>();

			// This genre list is generated alongside the main Album list
			AlbumsViewModel.AlbumIndexToGenreLookup = new List<string>();

			// We need a lookup table for all the Albums in the current filtered Album list
			Dictionary<int, Album> albumIds = AlbumsViewModel.FilteredAlbums.ToDictionary( alb => alb.Id );

			// The Albums need to be sorted in genre order. If there is no genre filter then use all the genre tags, otherwise just use
			// the tags in the filter
			TagGroup genreTags = AlbumsViewModel.FilterSelector.TagGroups.SingleOrDefault( ta => ta.Name == "Genre" ) ?? FilterManagementModel.GenreTags;

			// Get the Genre GroupTag and order the Tags by name. Copy the list so that we don't change it
			List <Tag> sortedTags = genreTags.Tags.ToList();
			sortedTags.Sort( ( a, b ) => { return a.Name.CompareTo( b.Name ); } );

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

		/// <summary>
		/// Generate the fast scroll indexes using the provided function to obtain the section name
		/// The sorted albums have already been moved/copied to the AlbumsViewModel.Albums list
		/// </summary>
		/// <param name="sectionNameProvider"></param>
		private static void GenerateIndex( Func<Album, int, string > sectionNameProvider )
		{
			// Initialise the index collections
			AlbumsViewModel.FastScrollSections = new List<Tuple<string, int>>();
			AlbumsViewModel.FastScrollSectionLookup = new int[ AlbumsViewModel.Albums.Count ];

			// Keep track of when a section has already been added to the FastScrollSections collection
			Dictionary<string, int> sectionLookup = new Dictionary<string, int>();

			int index = 0;
			foreach ( Album album in AlbumsViewModel.Albums )
			{
				// If this is the first occurrence of the section name then add it to the FastScrollSections collection together with the index 
				string sectionName = sectionNameProvider( album, index );
				int sectionIndex = sectionLookup.GetValueOrDefault( sectionName, -1 );
				if ( sectionIndex == -1 )
				{
					sectionIndex = sectionLookup.Count;
					sectionLookup[ sectionName ] = sectionIndex;
					AlbumsViewModel.FastScrollSections.Add( new Tuple<string, int>( sectionName, index ) );
				}

				// Provide a quick section lookup for this album
				AlbumsViewModel.FastScrollSectionLookup[ index++ ] = sectionIndex;
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static DataReporter.IReporter DataReporter
		{
			get => dataReporter.Reporter;
			set => dataReporter.Reporter = value;
		}

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}
