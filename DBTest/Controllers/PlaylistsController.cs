using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains Playlist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	static class PlaylistsController
	{
		/// <summary>
		/// Public constructor providing the Database path and the interface instance used to report results
		/// </summary>
		static PlaylistsController()
		{
			Mediator.RegisterPermanent( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
		}

		/// <summary>
		/// Get the Playlist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetPlaylistsAsync( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( ( PlaylistsViewModel.Playlists == null ) || ( PlaylistsViewModel.LibraryId != libraryId ) )
			{
				PlaylistsViewModel.LibraryId = libraryId;
				PlaylistsViewModel.Playlists = await PlaylistAccess.GetPlaylistDetailsAsync( PlaylistsViewModel.LibraryId );

				// Extract just the names as well
				PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();
			}

			// Let the Views know that Playlists data is available
			Reporter?.PlaylistsDataAvailable();
		}

		/// <summary>
		/// Get the contents for the specified Playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void GetPlaylistContents( Playlist thePlaylist )
		{
			PlaylistAccess.GetPlaylistContents( thePlaylist );

			// Sort the PlaylistItems by Track
			thePlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// If the playlists have already been obtained then make sure that the specified playlist contents are refreshed
		/// and let the view know
		/// </summary>
		/// <param name="message"></param>
		private static void SongsAdded( object message )
		{
			if ( PlaylistsViewModel.Playlists != null )
			{
				PlaylistSongsAddedMessage songsAddedMessage = message as PlaylistSongsAddedMessage;

				// Get the playlist from the model (not the database) and refresh its contents.
				// If it can't be found then do nothing - report an error?
				Playlist addedToPlaylist = PlaylistsViewModel.Playlists.FirstOrDefault( d => ( d.Name == songsAddedMessage.PlaylistName ) );

				if ( addedToPlaylist != null )
				{
					GetPlaylistContents( addedToPlaylist );
					Reporter?.SongsAdded( songsAddedMessage.PlaylistName );
				}
			}
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { private get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void PlaylistsDataAvailable();
			void SongsAdded( string playlistName );
		}
	}
}