namespace DBTest
{
	/// <summary>
	/// The NowPlayingViewModel holds the song data obtained from the NowPlayingController
	/// </summary>
	internal static class NowPlayingViewModel
	{
		/// The Now Playing playlist that has been obtained from the database
		/// </summary>
		public static SongPlaylist NowPlayingPlaylist { get; set; } = null;

		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public static int CurrentSongIndex
		{
			get => Playlists.CurrentSongIndex;
			set
			{
				// Normalise the value being set
				if ( value >= NowPlayingPlaylist.PlaylistItems.Count )
				{
					Playlists.CurrentSongIndex = -1;
				}
				else
				{
					Playlists.CurrentSongIndex = value;
				}

				CurrentSong = ( ( Playlists.CurrentSongIndex == -1 ) || ( NowPlayingPlaylist == null ) ) ? null :
					( ( SongPlaylistItem )NowPlayingPlaylist.PlaylistItems[ Playlists.CurrentSongIndex ] ).Song;
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

		/// <summary>
		/// The common model features are contained in the BaseViewModel
		/// </summary>
		public static BaseViewModel BaseModel { get; } = new BaseViewModel();
	}
}
