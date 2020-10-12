﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AlbumsController is the Controller for the AlbumsView. It responds to AlbumsView commands and maintains Albums data in the
	/// AlbumsViewModel
	/// </summary>
	static class AlbumsController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static AlbumsController()
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
		/// Get the Album data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetAlbums( int libraryId )
		{
			// Check if the Album details for the library have already been obtained
			if ( AlbumsViewModel.LibraryId != libraryId )
			{
				// New data is required. At this point the albums are not filtered
				AlbumsViewModel.LibraryId = libraryId;

				// All Albums are read as part of the Albums collection. So wait until that is available and then carry out the rest of the 
				// initialisation
				StorageController.RegisterInterestInDataAvailable( AlbumDataAvailable );
			}
			else
			{
				// Publish the data
				if ( AlbumsViewModel.DataValid == true )
				{
					Reporter?.AlbumsDataAvailable();
				}
			}
		}

		/// <summary>
		/// Get the contents for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static async Task GetAlbumContentsAsync( Album theAlbum )
		{
			await AlbumAccess.GetAlbumSongsAsync( theAlbum );

			// Sort the songs by track number - UI thread but not many entries
			theAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, string playlistName )
		{
			// Carry out the common processing to add songs to a playlist
			Playlists.GetPlaylist( playlistName, AlbumsViewModel.LibraryId ).AddSongs( songsToAdd );

			// Publish this event
			new PlaylistSongsAddedMessage() { PlaylistName = playlistName }.Send();
		}

		/// <summary>
		/// Wrapper around ApplyFilterAsync to match delegate signature
		/// </summary>
		/// <param name="newFilter"></param>
		/// <returns></returns>
		public static async Task ApplyFilterDelegateAsync( Tag newFilter ) => await ApplyFilterAsync( newFilter );

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public static async Task ApplyFilterAsync( Tag newFilter, bool report = true )
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
					List< TaggedAlbum > albumFilter = BaseController.CombineAlbumFilters( AlbumsViewModel.CurrentFilter, AlbumsViewModel.TagGroups );

					// First of all form a set of all the album identities in the selected filter
					HashSet<int> albumIds = albumFilter.Select( ta => ta.AlbumId ).ToHashSet();

					// Now get all the albums that are tagged and in the current library
					AlbumsViewModel.Albums = AlbumsViewModel.UnfilteredAlbums.FindAll( album => albumIds.Contains( album.Id ) == true );

					// If the TagOrder flag is set then set the sort order to Id order.
					if ( ( AlbumsViewModel.CurrentFilter?.TagOrder??false ) == true )
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
				Reporter?.AlbumsDataAvailable();
			}
		}

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
				Reporter?.AlbumsDataAvailable();
			}
		}

		/// <summary>
		/// Called when the Album data has been read in from storage
		/// </summary>
		/// <param name="message"></param>
		private static async void AlbumDataAvailable( object message )
		{
			AlbumsViewModel.UnfilteredAlbums = Albums.AlbumCollection.Where( alb => alb.LibraryId == AlbumsViewModel.LibraryId ).ToList();

			// Populate the genre name from the id in the album
			await PopulateAlbumGenresAsync();

			// Revert to no filter and sort the data
			await ApplyFilterAsync( null, false );

			// Get the list of current playlists
			GetPlayListNames();

			AlbumsViewModel.DataValid = true;

			Reporter?.AlbumsDataAvailable();
		}

		/// <summary>
		/// Once the Albums have been read their genre id fields can be used to set their genre name values
		/// </summary>
		private static async Task PopulateAlbumGenresAsync()
		{
			// Do the linking of Album entries off the UI thread
			await Task.Run( () =>
			{
				foreach ( Album album in AlbumsViewModel.UnfilteredAlbums )
				{
					album.Genre = Genres.GetGenreName( album.GenreId );
				}
			} );
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

			// Publish the data
			Reporter?.AlbumsDataAvailable();

			// Reread the data
			GetAlbums( ConnectionDetailsModel.LibraryId );
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
					Reporter?.AlbumsDataAvailable();
				}
			}
		}

		/// <summary>
		/// Get the names of all the user playlists
		/// </summary>
		private static void GetPlayListNames() =>
			AlbumsViewModel.PlaylistNames = Playlists.GetPlaylistsForLibrary( AlbumsViewModel.LibraryId ).Select( list => list.Name).ToList();

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void AlbumsDataAvailable();
		}
	}
}