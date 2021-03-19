﻿using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The PlaybackManagerModel holds the song data and playback modes obtained from the PlaybackManagementController
	/// </summary>
	static class PlaybackManagerModel
	{
		/// <summary>
		/// The Now Playing playlist
		/// </summary>
		public static Playlist NowPlayingPlaylist { get; set; } = null;

		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public static int CurrentSongIndex
		{
			get => currentSongIndex;
			set
			{
				currentSongIndex = value;

				CurrentSong = ( ( currentSongIndex == -1 ) || ( NowPlayingPlaylist == null ) ) ? null :
					( ( SongPlaylistItem )NowPlayingPlaylist.PlaylistItems[ currentSongIndex ] ).Song;
			}
		}

		/// <summary>
		/// The current song being played
		/// </summary>
		public static Song CurrentSong { get; private set; } = null;

		/// <summary>
		/// The sources associated with the library
		/// </summary>
		public static List<Source> Sources { get; set; } = null;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// The details of the selected available playback device
		/// </summary>
		public static PlaybackDevice AvailableDevice { get; set; } = null;

		/// <summary>
		/// Indicates whether or not the data held by the class is valid
		/// </summary>
		public static bool DataValid { get; set; } = false;

		/// <summary>
		/// Private song index accessed via the CurrentSongIndex property
		/// </summary>
		private static int currentSongIndex = -1;
	}
}