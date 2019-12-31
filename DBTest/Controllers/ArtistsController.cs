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
				ArtistsViewModel.Artists = await ArtistAccess.GetArtistDetailsAsync( ArtistsViewModel.LibraryId, ArtistsViewModel.CurrentFilter );

				// Sort the Artists and create the indexes in a Task rather than the UI
				await Task.Run( () => {
					// Sort the list of artists by name. This is done even when the artists are filtered because an artist can contain multiple albums
					// Do a normal comparison, except remove a leading 'The ' before comparing
					ArtistsViewModel.Artists.Sort( ( a, b ) => { return a.Name.RemoveThe().CompareTo( b.Name.RemoveThe() ); } );

					// Work out the section indexes for the sorted data
					ArtistsViewModel.AlphaIndex.Clear();

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
				} );

				// Get the list of current playlists and extract the names to a list
				await GetPlayListNames();

				// Get the Tags as well
				ArtistsViewModel.Tags = await FilterAccess.GetTagsAsync();
			}

			// Publish the data
			Reporter?.ArtistsDataAvailable();
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// This amount of work could/should be done in a non-UI task, but not sure at the moment how to
		/// interact with the expanding UI.
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist )
		{
			await ArtistAccess.GetArtistContentsAsync( theArtist, ArtistsViewModel.CurrentFilter );

			// Sort the albums alphabetically
			theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			// Sort the songs by track number
			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}

			// Now all the ArtistAlbum and Song entries have been read form a single list from them
			theArtist.EnumerateContents();
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
		public static void ApplyFilter( Tag newFilter )
		{
			// Clear the displayed data first as this may take a while
			ArtistsViewModel.ClearModel();

			// Publish the data
			Reporter?.ArtistsDataAvailable();

			// Update the filter and reread the data
			ArtistsViewModel.CurrentFilter = newFilter;

			GetArtistsAsync( ConnectionDetailsModel.LibraryId );
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
				ApplyFilter( ArtistsViewModel.CurrentFilter );
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