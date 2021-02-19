using System.Threading.Tasks;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The DbAccess class is used for DB operations that can be performed generically
	/// </summary>
	static class DbAccess
	{
		/// <summary>
		/// Get all the members of a table
		/// </summary>
		public static async Task<List<T>> LoadAsync<T>() where T : new() => await ConnectionDetailsModel.AsynchConnection.Table<T>().ToListAsync();

		/// <summary>
		/// Delete the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async void DeleteAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( item );

		/// <summary>
		/// Insert the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async Task InsertAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( item );

		/// <summary>
		/// Update the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async void UpdateAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( item );

		/// <summary>
		/// Delete the specified list of items
		/// </summary>
		/// <param name="items"></param>
		public static void DeleteItemsAsync<T>( IEnumerable<T> items )
		{
			foreach ( T item in items )
			{
				DeleteAsync( item );
			}
		}

		//
		// All the followimg non-generic access methods are used to access Songs.
		// Songs are different to all the other DB based objects in that they are not all read into memory, so specific queries are required.
		//

		/// <summary>
		/// Get the songs for the specified Album identity
		/// </summary>
		/// <param name="albumId"></param>
		public static async Task<List<Song>> GetAlbumSongsAsync( int albumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.AlbumId == albumId ) ).ToListAsync();

		/// <summary>
		/// Get a Song entry from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<Song> GetSongAsync( int songId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.Id == songId ) ).FirstAsync();

		/// <summary>
		/// Get the songs associated with a specific ArtistAlbum
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetArtistAlbumSongsAsync( int artistAlbumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbumId ) ).ToListAsync();

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
	}
}