using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaylistsViewModel holds the SongPlaylist data obtained from the PlaylistsController
	/// </summary>
	internal static class PlaylistsViewModel
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
			BaseModel.Clear();
		}

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
		/// The common model features are contained in the BaseViewModel
		/// </summary>
		public static BaseViewModel BaseModel { get; } = new BaseViewModel();
	}
}
