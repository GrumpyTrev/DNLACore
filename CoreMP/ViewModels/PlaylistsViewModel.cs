using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The PlaylistsViewModel holds the SongPlaylist data obtained from the PlaylistsController
	/// </summary>
	public static class PlaylistsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Playlists.Clear();
			SongPlaylists.Clear();
			AlbumPlaylists.Clear();
			LibraryId = -1;
		}

		/// <summary>
		/// Allow changes to this model to be monitored
		/// </summary>
		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// <summary>
		/// The list of PlayLists for the current library
		/// </summary>
		public static List<Playlist> Playlists { get; set; } = new List<Playlist>();

		/// <summary>
		/// The SongPlaylist entries in the Playlists collection
		/// </summary>
		public static List<SongPlaylist> SongPlaylists { get; set; } = new List<SongPlaylist>();

		/// <summary>
		/// The AlbumPlaylist entries in the Playlists collection
		/// </summary>
		public static List<AlbumPlaylist> AlbumPlaylists { get; set; } = new List<AlbumPlaylist>();

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// Property introduced to trigger a notification when changes to a playlist have been made
		/// </summary>
		public static Playlist PlaylistUpdated
		{
			set => NotificationHandler.NotifyPropertyChanged( value );
		}
	}
}
