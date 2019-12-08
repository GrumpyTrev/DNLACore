using System.Collections.Generic;
using System.Linq;

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
			if ( ( AlbumsViewModel.Albums == null ) || ( AlbumsViewModel.LibraryId != libraryId ) )
			{
				// New data is required
				AlbumsViewModel.LibraryId = libraryId;
				AlbumsViewModel.Albums = await AlbumAccess.GetAlbumDetailsAsync( AlbumsViewModel.LibraryId, AlbumsViewModel.CurrentFilter );

				// Sort the list of artists by name
				AlbumsViewModel.Albums.Sort( ( a, b ) => {
					// Do a normal comparison, except remove a leading 'The ' before comparing
					string albumA = ( a.Name.ToUpper().StartsWith( "THE " ) == true ) ? a.Name.Substring( 4 ) : a.Name;
					string albumB = ( b.Name.ToUpper().StartsWith( "THE " ) == true ) ? b.Name.Substring( 4 ) : b.Name;

					return albumA.CompareTo( albumB );
				} );

				// Work out the section indexes for the sorted data
				AlbumsViewModel.AlphaIndex = new Dictionary<string, int>();
				int index = 0;
				foreach ( Album album in AlbumsViewModel.Albums )
				{
					string key = album.Name[ 0 ].ToString();
					if ( AlbumsViewModel.AlphaIndex.ContainsKey( key ) == false )
					{
						AlbumsViewModel.AlphaIndex[ key ] = index;
					}
					index++;
				}

				// Get the list of current playlists
				List< Playlist > playlists = await PlaylistAccess.GetPlaylistDetailsAsync( AlbumsViewModel.LibraryId );

				// Extract just the names as well
				AlbumsViewModel.PlaylistNames = playlists.Select( i => i.Name ).ToList();

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
		public static void GetAlbumContents( Album theAlbum )
		{
			AlbumAccess.GetAlbumContents( theAlbum );

			// Sort the songs by track number
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
			PlaylistAccess.AddSongsToPlaylist( songsToAdd, playlistName, AlbumsViewModel.LibraryId );

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
			AlbumsViewModel.Albums = null;
			AlbumsViewModel.AlphaIndex = null;

			// Publish the data
			Reporter?.AlbumsDataAvailable();

			// Update the model and reread the data
			AlbumsViewModel.CurrentFilter = newFilter;

			GetAlbumsAsync( AlbumsViewModel.LibraryId );
		}

		/// <summary>
		/// Called when a PlaylistDeletedMessage or PlaylistAddedMessage message has been received
		/// Update the list of playlists held by the model
		/// </summary>
		/// <param name="message"></param>
		private static async void PlaylistAddedOrDeleted( object message )
		{
			// Get the list of current playlists
			List<Playlist> playlists = await PlaylistAccess.GetPlaylistDetailsAsync( AlbumsViewModel.LibraryId );

			AlbumsViewModel.PlaylistNames = playlists.Select( i => i.Name ).ToList();
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
			// Set the new library
			AlbumsViewModel.LibraryId = ( message as SelectedLibraryChangedMessage ).SelectedLibrary.Id;

			// Clear the displayed data and filter
			AlbumsViewModel.Albums = null;
			AlbumsViewModel.AlphaIndex = null;
			AlbumsViewModel.CurrentFilter = null;

			// Publish the data
			Reporter?.AlbumsDataAvailable();

			// Reread the data
			GetAlbumsAsync( AlbumsViewModel.LibraryId );
		}

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