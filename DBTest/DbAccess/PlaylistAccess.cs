using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaylistAccess class is used to access and change Playlist data via the database
	/// </summary>
	class PlaylistAccess
	{
		/// <summary>
		/// Get all the playlists in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Playlist>> GetAllPlaylistsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().ToListAsync();

		/// <summary>
		/// Get all the playlist items in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<PlaylistItem>> GetPlaylistItemsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<PlaylistItem>().ToListAsync();

		/// <summary>
		/// Delete the specified playlist from the database
		/// </summary>
		/// <param name="thePlaylist"></param>
		/// <returns></returns>
		public static void DeletePlaylist( Playlist thePlaylist )
		{
			// Delete the PlaylistItem entries from the database.
			// No need to wait for this to finish
			DeletePlaylistItems( thePlaylist.PlaylistItems );

			// Now delete the playlist itself. No need to wait for this to finish
			ConnectionDetailsModel.AsynchConnection.DeleteAsync( thePlaylist );
		}

		/// <summary>
		/// Delete the specified PlaylistItem items
		/// </summary>
		/// <param name="items"></param>
		public static void DeletePlaylistItems( IEnumerable<PlaylistItem> items )
		{
			foreach ( PlaylistItem item in items )
			{
				ConnectionDetailsModel.AsynchConnection.DeleteAsync( item );
			}
		}

		/// <summary>
		/// Add the specified playlist to the database
		/// </summary>
		/// <param name="playlistName"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static void AddPlaylist( Playlist playlistToAdd ) => ConnectionDetailsModel.AsynchConnection.InsertAsync( playlistToAdd );

		/// <summary>
		/// Update a modified PlaylistItem
		/// </summary>
		/// <param name="itemToUpdate"></param>
		/// <returns></returns>
		public static async void UpdatePlaylistItemAsync( PlaylistItem itemToUpdate ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( itemToUpdate );

		/// <summary>
		/// Update a modified Playlist
		/// </summary>
		/// <param name="itemToUpdate"></param>
		/// <returns></returns>
		public static async void UpdatePlaylistAsync( Playlist itemToUpdate ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( itemToUpdate );

		/// <summary>
		/// Add a new PlaylistItem to the database. No need to wait for this to complete
		/// </summary>
		/// <param name="itemToAdd"></param>
		public static void AddPlaylistItem( PlaylistItem itemToAdd ) => ConnectionDetailsModel.AsynchConnection.InsertAsync( itemToAdd );
	}
};