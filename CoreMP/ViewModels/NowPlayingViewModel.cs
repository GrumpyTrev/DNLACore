namespace CoreMP
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	public static class NowPlayingViewModel
	{
		/// <summary>
		/// Allow changes to this model to be monitored
		/// </summary>
		public static ModelAvailable Available { get; } = new ModelAvailable();

		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static SongPlaylist NowPlayingPlaylist { get; set; } = null;

		/// <summary>
		/// Property introduced to trigger a notification when changes to the playlist have been made (and completed)
		/// </summary>
		public static bool PlaylistUpdated
		{
			set => NotificationHandler.NotifyPropertyChanged( null );
		}

		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public static int CurrentSongIndex
		{
			get => Playlists.CurrentSongIndex;
			set
			{
				Playlists.CurrentSongIndex = value;

				CurrentSong = ( ( Playlists.CurrentSongIndex == -1 ) || ( NowPlayingPlaylist == null ) ) ? null :
					( ( SongPlaylistItem )NowPlayingPlaylist.PlaylistItems[ Playlists.CurrentSongIndex ] ).Song;

				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// The current song being played
		/// </summary>
		public static Song CurrentSong { get; set; } = null;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;
	}
}
