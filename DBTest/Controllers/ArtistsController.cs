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
		public static async void GetArtistsAsync( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( ArtistsViewModel.LibraryId != libraryId )
			{
				// New data is required
				ArtistsViewModel.LibraryId = libraryId;
				ArtistsViewModel.UnfilteredArtists = await ArtistAccess.GetArtistDetailsAsync( ArtistsViewModel.LibraryId );
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists;

				// Sort the displayed artists - off the UI thread
				await SortDataAsync();

				// Get the list of current playlists and extract the names to a list
				await GetPlayListNames();

				// The data is now valid and can be reported
				ArtistsViewModel.DataValid = true;
				Reporter?.ArtistsDataAvailable();

				// Do the linking of ArtistAlbum entries off the UI thread
				await PartiallyPopulateArtistsAsync();
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
				await ArtistAccess.GetArtistContentsAsync( theArtist );

				// Mark the details have been read
				theArtist.DetailsRead = true;

				// Sort the albums alphabetically
				theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

				// Sort the songs by track number
				foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
				{
					artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}
			}

			// Now form the displayable content according to the current filter.
			// NB The Content is cleared whenever the current filter is changed
			if ( theArtist.Contents.Count == 0 )
			{
				// Now all the ArtistAlbum and Song entries have been read form a single list from them
				if ( ArtistsViewModel.CurrentFilter != null )
				{
					HashSet<int> albumIds = ArtistsViewModel.CurrentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToHashSet();
					theArtist.EnumerateContents( albumIds );
				}
				else
				{
					theArtist.EnumerateContents( null );
				}
			}
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static async void AddSongsToPlaylistAsync( List<Song> songsToAdd, string playlistName )
		{
			// Carry out the common processing to add songs to a playlist
			await PlaylistAccess.AddSongsToPlaylistAsync( songsToAdd, playlistName, ArtistsViewModel.LibraryId );

			// Publish this event
			new PlaylistSongsAddedMessage() { PlaylistName = playlistName }.Send();
		}

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public static async void ApplyFilterAsync( Tag newFilter )
		{
			// Update the model
			ArtistsViewModel.CurrentFilter = newFilter;

			// Assume the artists are going to be displayed in alphabetical order
			ArtistsViewModel.CurrentSortOrder = AlbumSortSelector.AlbumSortOrder.alphaAscending;

			// Clear the Contents entry for all the Artists so that it can be set according to the current filter
			ArtistsViewModel.UnfilteredArtists.ForEach( art => art.Contents.Clear() );

			// If there is no filter then display the unfiltered data 
			if ( ArtistsViewModel.CurrentFilter == null )
			{
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists;
			}
			else
			{
				// All of this is in the UI thread

				// Access artists that have albums that are tagged with the current tag
				// For all TagAlbums in current tag get the ArtistAlbum (from the AlbumId) and the Artists 

				// First of all form a list of all the album identities in the selected filter
				HashSet<int> albumIds = ArtistsViewModel.CurrentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToHashSet();

				// Now get all the artist identities of the albums that are tagged
				HashSet<int> artistIds = ArtistsViewModel.ArtistAlbums.FindAll( aa => albumIds.Contains( aa.AlbumId ) ).Select( aa => aa.ArtistId ).ToHashSet();

				// Now get the Artists from the list of artist ids
				ArtistsViewModel.Artists = ArtistsViewModel.UnfilteredArtists.Where( art => artistIds.Contains( art.Id ) == true ).ToList();

				// If the TagOrder flag is set then set the sort order to Id order.
				if ( ArtistsViewModel.CurrentFilter.TagOrder == true )
				{
					ArtistsViewModel.CurrentSortOrder = AlbumSortSelector.AlbumSortOrder.idDescending;
				}
			}

			// Sort the displayed albums to the order specified in the SortSelector
			await SortDataAsync();

			// Publish the data
			Reporter?.ArtistsDataAvailable();
		}

		/// <summary>
		/// Sort the available data
		/// </summary>
		public static async Task SortDataAsync( bool refreshData = false )
		{
			// Do the sorting and indexing off the UI task
			await Task.Run( () => 
			{
				switch ( ArtistsViewModel.CurrentSortOrder )
				{
					case AlbumSortSelector.AlbumSortOrder.alphaDescending:
					case AlbumSortSelector.AlbumSortOrder.alphaAscending:
					{
						if ( ArtistsViewModel.CurrentSortOrder == AlbumSortSelector.AlbumSortOrder.alphaAscending )
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
						}
						else
						{
							ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
						}

						// Work out the section indexes for the sorted data
						int index = 0;
						foreach ( Artist artist in ArtistsViewModel.Artists )
						{
							// Remember to ignore leading 'The ' here as well
							string key = artist.Name.RemoveThe().Substring( 0, 1 ).ToUpper();
							if ( ArtistsViewModel.AlphaIndex.ContainsKey( key ) == false )
							{
								ArtistsViewModel.AlphaIndex[ key ] = index;
							}
							index++;
						}

						break;
					}

					case AlbumSortSelector.AlbumSortOrder.idAscending:
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Id.CompareTo( b.Id ); } );
						break;
					}

					case AlbumSortSelector.AlbumSortOrder.idDescending:
					{
						ArtistsViewModel.Artists.Sort( ( a, b ) => { return b.Id.CompareTo( a.Id ); } );
						break;
					}
				}
			} );

			if ( refreshData == true )
			{
				// Publish the data
				Reporter?.ArtistsDataAvailable();
			}
		}

		/// <summary>
		/// Once the Artists have been read in their associated ArtistAlbums can be read as well and linked to them
		/// The ArtistAlbums are required for filtering so they may as well be linked in at the same time
		/// Get the Album associated with the ArtistAlbum as well so that only a single copy of the Albums is used (that in the AlbumsViewModel)
		/// </summary>
		private static async Task PartiallyPopulateArtistsAsync()
		{
			// Do the linking of ArtistAlbum entries off the UI thread
			await Task.Run( async () =>
			{
				// Get all the ArtistAlbums. NB This is all the ArtistAlbums in the whole database not just the current library.
				// The list that gets stored will be built as the Albums are resolved below
				List<ArtistAlbum> allArtistAlbums = await ArtistAccess.GetArtistAlbumsAsync();
				ArtistsViewModel.ArtistAlbums = new List<ArtistAlbum>();

				// Link the Albums from the AlbumModel to the ArtistAlbums and link the ArtistAlbums to their associated Artists. 
				// Need to access the Artists by their identities and the Albums by their identities
				Dictionary<int, Artist> artistDictionary = ArtistsViewModel.UnfilteredArtists.ToDictionary( artist => artist.Id );
				Dictionary<int, Album> albumDictionary = AlbumsViewModel.UnfilteredAlbums.ToDictionary( album => album.Id );

				foreach ( ArtistAlbum artAlbum in allArtistAlbums )
				{
					// Link in the single Album
					artAlbum.Album = albumDictionary.GetValueOrDefault( artAlbum.AlbumId );

					if ( artAlbum.Album != null )
					{
						// This ArtistAlbum is in the current library so save it
						ArtistsViewModel.ArtistAlbums.Add( artAlbum );

						// Add this ArtistAlbum to its Artist
						Artist associatedArtist = artistDictionary[ artAlbum.ArtistId ];
						if ( associatedArtist.ArtistAlbums == null )
						{
							associatedArtist.ArtistAlbums = new List<ArtistAlbum>();
						}

						associatedArtist.ArtistAlbums.Add( artAlbum );
					}
				}
			} );
		}

		/// <summary>
		/// Called when a PlaylistDeletedMessage or PlaylistAddedMessage message has been received
		/// Update the list of playlists held by the model
		/// </summary>
		/// <param name="message"></param>
		private static async void PlaylistAddedOrDeleted( object message ) => await GetPlayListNames();

		/// <summary>
		/// Called when a TagMembershipChangedMessage has been received
		/// If there is no filtering of if the tag being filtered on has not changed then no action is required.
		/// Otherwise the data must be refreshed
		/// </summary>
		/// <param name="message"></param>
		private static void TagMembershipChanged( object message )
		{
			if ( ( ArtistsViewModel.CurrentFilter != null ) &&
				( ( message as TagMembershipChangedMessage ).ChangedTags.Contains( ArtistsViewModel.CurrentFilter.Name ) == true ) )
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
			GetArtistsAsync( ConnectionDetailsModel.LibraryId );
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
		/// Get the names of all the user playlists
		/// </summary>
		private static async Task GetPlayListNames() => 
			ArtistsViewModel.PlaylistNames = ( await PlaylistAccess.GetPlaylistDetailsAsync( ArtistsViewModel.LibraryId ) ).Select( i => i.Name ).ToList();

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