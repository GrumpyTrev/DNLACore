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
		/// Get the Playback record from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<Playback> GetPlaybackAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Playback>().FirstAsync();

		/// <summary>
		/// Save the Playback record in the database
		/// </summary>
		public static async Task UpdatePlaybackAsync( Playback playbackRecord ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( playbackRecord );
	}
}