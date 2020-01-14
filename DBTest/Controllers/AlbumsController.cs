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
		}

		/// <summary>
		/// Get the Album data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetAlbumsAsync( int libraryId )
		{
			// Check if the Album details for the library have already been obtained
			if ( AlbumsViewModel.LibraryId != libraryId )
			{
				// New data is required
				AlbumsViewModel.LibraryId = libraryId;
				AlbumsViewModel.Albums = await AlbumAccess.GetAlbumDetailsAsync( AlbumsViewModel.LibraryId, AlbumsViewModel.CurrentFilter );
				AlbumsViewModel.AlphaIndex.Clear();

				// Sort the list of albums by name unless a filter with the tag order set has been applied
				if ( ( AlbumsViewModel.CurrentFilter?.TagOrder ?? false ) == false )
				{
					// Do the sorting and indexing off the UI task
					await Task.Run( () => {

						// Use the sort order stored in the model
						AlbumSortSelector.AlbumSortOrder sortOrder = AlbumsViewModel.SortSelector.CurrentSortOrder;

						switch ( sortOrder )
						{
							case AlbumSortSelector.AlbumSortOrder.alphaDescending:                            
							case AlbumSortSelector.AlbumSortOrder.alphaAscending:
							{
								if ( sortOrder == AlbumSortSelector.AlbumSortOrder.alphaAscending )
								{
									AlbumsViewModel.Albums.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );
								}
								else
								{
									AlbumsViewModel.Albums.Sort( ( a, b ) => { return b.Name.RemoveThe().CompareTo( a.Name.RemoveThe() ); } );
								}

								// Work out the section indexes for the sorted data
								int index = 0;
								foreach ( Album album in AlbumsViewModel.Albums )
								{
									string key = album.Name.RemoveThe().Substring( 0, 1 ).ToUpper();
									if ( AlbumsViewModel.AlphaIndex.ContainsKey( key ) == false )
									{
										AlbumsViewModel.AlphaIndex[ key ] = index;
									}
									index++;
								}

								break;
							}

							case AlbumSortSelector.AlbumSortOrder.idAscending:
							{
								// Already in this order
								break;
							}

							case AlbumSortSelector.AlbumSortOrder.idDescending:
							{
								// Reverse the albums
								AlbumsViewModel.Albums.Reverse();
								break;
							}
						}
					} );
				}
				else
				{
					// When a filter is applied the entries are in Tag ID order. 
					// The most useful order for display purposes is the reverse of this.
					// Do the sorting and indexing off the UI task
					await Task.Run( () => { AlbumsViewModel.Albums.Reverse(); } );
				}

				// Get the list of current playlists
				await GetPlayListNames();

				// Get the Tags as well
				AlbumsViewModel.Tags = await FilterAccess.GetTagsAsync();
			}

			// Publish the data
			Reporter?.AlbumsDataAvailable();
		}

		/// <summary>
		/// Get the contents for the specified Album
		/// </summary>
		/// <param name="theAlbum"></param>
		public static async Task GetAlbumContentsAsync( Album theAlbum )
		{
			await AlbumAccess.GetAlbumContentsAsync( theAlbum );

			// Sort the songs by track number
			theAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static async void AddSongsToPlaylistAsync( List<Song> songsToAdd, string playlistName )
		{
			// Carry out the common processing to add songs to a playlist
			await PlaylistAccess.AddSongsToPlaylistAsync( songsToAdd, playlistName, AlbumsViewModel.LibraryId );

			// Publish this event
			new PlaylistSongsAddedMessage() { PlaylistName = playlistName }.Send();
		}

		/// <summary>
		/// Apply the new filter to the data being displayed
		/// </summary>
		/// <param name="newFilter"></param>
		public static void ApplyFilter( Tag newFilter )
		{
			// Clear the displayed data first as this may take a while
			AlbumsViewModel.ClearModel();

			// Publish the data
			Reporter?.AlbumsDataAvailable();

			// Update the model and reread the data
			AlbumsViewModel.CurrentFilter = newFilter;

			GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Refresh the data after a sort or other change requiring reaquiring and display of the data
		/// </summary>
		/// <param name="newFilter"></param>
		public static void RefreshData()
		{
			// Clear the displayed data
			AlbumsViewModel.ClearModel();

			// Get the data again
			GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
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
			if ( ( AlbumsViewModel.CurrentFilter != null ) &&
				( ( message as TagMembershipChangedMessage ).ChangedTags.Contains( AlbumsViewModel.CurrentFilter.Name ) == true ) )
			{
				ApplyFilter( AlbumsViewModel.CurrentFilter );
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
			GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
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
					ApplyFilter( tagMessage.ChangedTag );
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
				ApplyFilter( null );
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
			void AlbumsDataAvailable();
		}
	}
}