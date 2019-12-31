using System.Linq;
using System.Threading.Tasks;

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
		public static async Task SetSelectedSongAsync( int songIndex )
		{
			Playback playbackRecord = await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync();
			playbackRecord.SongIndex = songIndex;
			await ConnectionDetailsModel.AsynchConnection.UpdateAsync( playbackRecord );
		}

		/// <summary>
		/// Get the selected song
		/// </summary>
		/// <returns></returns>
		public static async Task<int> GetSelectedSongAsync() => ( await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync() ).SongIndex;

		/// <summary>
		/// Get the current (last used) playback device name
		/// </summary>
		/// <returns></returns>
		public static async Task< string > GetPlaybackDeviceAsync() => ( await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync() ).PlaybackDeviceName;

		/// <summary>
		/// Save the device name in the database
		/// </summary>
		public static async Task SetPlaybackDeviceAsync( string deviceName )
		{
			Playback playbackRecord = await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync();
			playbackRecord.PlaybackDeviceName = deviceName;
			await ConnectionDetailsModel.AsynchConnection.UpdateAsync( playbackRecord );
		}

		/// <summary>
		/// Update the selected library
		/// </summary>
		/// <param name="selectedLibrary"></param>
		public static async Task SetSelectedLibraryAsync( Library selectedLibrary )
		{
			Playback playbackRecord = await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync();
			playbackRecord.LibraryId = selectedLibrary.Id;
			await ConnectionDetailsModel.AsynchConnection.UpdateAsync( playbackRecord );
		}
	}
}