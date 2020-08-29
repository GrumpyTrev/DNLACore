using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The ArtistAccess class is used to access and change Artist data via the database
	/// </summary>
	static class ArtistAccess
	{
		/// <summary>
		/// Get all the Artists in the data base
		/// </summary>
		public static async Task<List<Artist>> GetAllArtistsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Artist>().ToListAsync();

		/// <summary>
		/// Get the contents for the specified Artist
		/// The ArtistAlbum entries have already been obtained so just get the Songs for them
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistSongsAsync( Artist theArtist )
		{
			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				artistAlbum.Songs =
						await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbum.Id ) ).ToListAsync();
			}
		}

		/// <summary>
		/// Insert a new ArtistAlbum in the database
		/// </summary>
		/// <param name="artistAlbum"></param>
		/// <returns></returns>
		public static async Task AddArtistAlbumAsync( ArtistAlbum artistAlbum ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( artistAlbum );

		/// <summary>
		/// Get the children entries for this ArtistAlbum
		/// </summary>
		/// <param name="libraryToPopulate"></param>
		/// <returns></returns>
		public static async Task GetArtistAlbumSongsAsync( ArtistAlbum artistAlbumToPopulate ) => artistAlbumToPopulate.Songs = 
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbumToPopulate.Id ) ).ToListAsync();

		/// <summary>
		/// Insert a new Artist in the database
		/// </summary>
		/// <param name="artist"></param>
		/// <returns></returns>
		public static async Task AddArtistAsync( Artist artist ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( artist );

		/// <summary>
		/// Insert a new Song in the database
		/// </summary>
		/// <param name="song"></param>
		/// <returns></returns>
		public static async Task AddSongAsync( Song song ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( song );

		/// <summary>
		/// Get all of the ArtistAlbum entries in the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<ArtistAlbum>> GetArtistAlbumsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().ToListAsync();

		/// <summary>
		/// Get all the songs in the specified source with the specified title 
		/// </summary>
		/// <param name="songName"></param>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetMatchingSongAsync( string songName, int sourceId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.Title == songName ) && ( song.SourceId == sourceId ) ).ToListAsync();

		/// <summary>
		/// Delete the specified list of songs from the database
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static async Task DeleteSongsAsync( List< Song > songsToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( songsToDelete );

		/// <summary>
		/// Delete the specified ArtistAlbum entry
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static async Task DeleteArtistAlbumAsync( ArtistAlbum albumToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( albumToDelete );

		/// <summary>
		/// Delete the specified Artist
		/// </summary>
		/// <param name="artistId"></param>
		/// <returns></returns>
		public static async Task DeleteArtistAsync( Artist artist ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( artist );
	}
}