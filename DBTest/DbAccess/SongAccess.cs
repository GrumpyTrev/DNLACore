using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
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

		/// <summary>
		/// Get the songs associated with a specific ArtistAlbum
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetArtistAlbumSongsAsync( int artistAlbumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbumId ) ).ToListAsync();

		/// <summary>
		/// Insert a new Song in the database
		/// </summary>
		/// <param name="song"></param>
		/// <returns></returns>
		public static async Task AddSongAsync( Song song ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( song );

		/// <summary>
		/// Get all the songs in the specified source with the specified title 
		/// </summary>
		/// <param name="songName"></param>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetMatchingSongAsync( string songName, int sourceId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.Title == songName ) && ( song.SourceId == sourceId ) ).ToListAsync();

		/// <summary>
		/// Get all the songs associated with the specified source
		/// </summary>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetSongsForSourceAsync( int sourceId ) => 
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => song.SourceId == sourceId ).ToListAsync();

		/// <summary>
		/// Delete the specified list of songs from the database
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static async Task DeleteSongsAsync( List<Song> songsToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( songsToDelete );

		/// <summary>
		/// Delete the specified song from the database
		/// </summary>
		/// <param name="songToDelete"></param>
		public static async Task DeleteSongAsync( Song songToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( songToDelete );
	}
}