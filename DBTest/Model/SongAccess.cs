using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The SongAccess class is used to access and change Song data via the database
	/// </summary>
	class SongAccess
	{
		/// <summary>
		/// Get a Song entry from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<Song> GetSongAsync( int songId ) => 
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => song.Id == songId ).FirstAsync();
	}
}