using System.Linq;

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
			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().Single();
			playbackRecord.SongIndex = songIndex;
			ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
		}

		/// <summary>
		/// Get the selected song
		/// </summary>
		/// <returns></returns>
		public static int GetSelectedSong() => ConnectionDetailsModel.SynchConnection.Table<Playback>().Single().SongIndex;

		/// <summary>
		/// Get the current (last used) playback device name
		/// </summary>
		/// <returns></returns>
		public static string GetPlaybackDevice() => ConnectionDetailsModel.SynchConnection.Table<Playback>().Single().PlaybackDeviceName;

		/// <summary>
		/// Save the device name in the database
		/// </summary>
		public static void SetPlaybackDevice( string deviceName )
		{
			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().Single();
			playbackRecord.PlaybackDeviceName = deviceName;
			ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
		}

		/// <summary>
		/// Update the selected library
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public static void SetSelectedLibrary( Library selectedLibrary )
		{
			Playback playbackRecord = ConnectionDetailsModel.SynchConnection.Table<Playback>().Single();
			playbackRecord.LibraryId = selectedLibrary.Id;
			ConnectionDetailsModel.SynchConnection.Update( playbackRecord );
		}
	}
}