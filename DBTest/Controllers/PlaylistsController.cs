using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsController is the Controller for the PlaylistsView. It responds to PlaylistsView commands and maintains Playlist data in the
	/// PlaylistsViewModel
	/// /// </summary>
	static class PlaylistsController
	{
		/// <summary>
		/// Get the Playlist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetPlaylistsAsync( int libraryId )
		{
			// Check if the Playlists details for the library have already been obtained
			if ( ( PlaylistsViewModel.Playlists == null ) || ( LibraryId != libraryId ) )
			{
				LibraryId = libraryId;
				PlaylistsViewModel.Playlists = await PlaylistAccess.GetPlaylistDetailsAsync( DatabasePath, LibraryId );

				// Extract just the names as well
				PlaylistsViewModel.PlaylistNames = PlaylistsViewModel.Playlists.Select( i => i.Name ).ToList();
			}

			// Let the Views know that Playlists data is available
			new PlaylistsDataAvailableMessage().Send();
		}

		/// <summary>
		/// Add a list of Songs to the Now Playing list
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToNowPlayingList( List<Song> songsToAdd, bool clearFirst )
		{
			if ( clearFirst == true )
			{
				PlaylistAccess.ClearNowPlayingList( DatabasePath, LibraryId );

				// Publish this event
				new NowPlayingClearedMessage().Send();
			}

			// Carry out the common processing to add songs to a playlist
			PlaylistAccess.AddSongsToNowPlayingList( songsToAdd, DatabasePath, LibraryId );

			// Raise the NowPlayingSongsAddedMessage
			new NowPlayingSongsAddedMessage() { Songs = songsToAdd }.Send();
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, string playlistName )
		{
			// Find the Playlist object associated with the playlist name in the PlaylistsViewModel
			Playlist selectedPlaylist = PlaylistsViewModel.Playlists.Where( d => d.Name == playlistName ).SingleOrDefault();

			if ( selectedPlaylist != null )
			{
				// Carry out the common processing to add songs to a playlist
				PlaylistAccess.AddSongsToPlaylist( songsToAdd, selectedPlaylist, DatabasePath );

				// Make sure that all of the playlist details are loaded
				GetPlaylistContents( selectedPlaylist );

				// Publish this event
				new PlaylistSongsAddedMessage() { Playlist = selectedPlaylist, Songs = songsToAdd }.Send();
			}
		}

		/// <summary>
		/// Get the contents for the specified Playlist
		/// </summary>
		/// <param name="thePlaylist"></param>
		public static void GetPlaylistContents( Playlist thePlaylist )
		{
			PlaylistAccess.GetPlaylistContents( thePlaylist, DatabasePath );

			// Sort the PlaylistItems by Track
			thePlaylist.PlaylistItems.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
		}

		/// <summary>
		/// The database file path
		/// </summary>
		public static string DatabasePath { private get; set; }

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		private static int LibraryId { get; set; } = -1;
	}
}