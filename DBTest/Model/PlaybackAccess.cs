using SQLite;

namespace DBTest
{
	/// <summary>
	/// The PlaybackAccess class is used to access and change Playback data via the database
	/// </summary>
	class PlaybackAccess
	{
		/// <summary>
		/// Set the selected song in the database
		/// </summary>
		public static void SetSelectedSong( int songIndex )
		{
			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();
			playbackRecord.SongIndex = songIndex;
			ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
		}

		/// <summary>
		/// Get the selected song
		/// </summary>
		/// <param name="databasePath"></param>
		/// <returns></returns>
		public static int GetSelectedSong()
		{
			int selectedSong = -1;

			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();
			selectedSong = playbackRecord.SongIndex;

			return selectedSong;
		}

		/// <summary>
		/// Get the current (last used) playback device name
		/// </summary>
		/// <returns></returns>
		public static string GetPlaybackDevice()
		{
			string playbackDevice = "";

			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();

			if ( playbackRecord != null )
			{
				playbackDevice = playbackRecord.PlaybackDeviceName;
			}

			return playbackDevice;
		}

		/// <summary>
		/// Save the device name in the database
		/// </summary>
		public static void SetPlaybackDevice( string deviceName )
		{
			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().FirstOrDefault();
			if ( playbackRecord != null )
			{
				playbackRecord.PlaybackDeviceName = deviceName;
				ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
			}
		}


	}
}