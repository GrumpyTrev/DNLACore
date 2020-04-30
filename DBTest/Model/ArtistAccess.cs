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
		/// Get all the Artists associated with the library identity
		/// </summary>
		public static async Task<List<Artist>> GetArtistDetailsAsync( int libraryId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table< Artist >().Where( art => art.LibraryId == libraryId ).ToListAsync();

		/// <summary>
		/// Get the contents for the specified Artist
		/// The ArtistAlbum entries have already been obtained so just get the Songs for them
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist )
		{
			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				artistAlbum.Songs =
						await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbum.Id ) ).ToListAsync();
			}
		}

		/// <summary>
		/// Get the children entries for this Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistChildrenAsync( Artist theArtist ) => await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( theArtist );

		/// <summary>
		/// Insert a new ArtistAlbum in the database
		/// </summary>
		/// <param name="artistAlbum"></param>
		/// <returns></returns>
		public static async Task AddArtistAlbumAsync( ArtistAlbum artistAlbum ) => 
			await ConnectionDetailsModel.AsynchConnection.InsertAsync( artistAlbum );

		/// <summary>
		/// Get the children entries for this ArtistAlbum
		/// </summary>
		/// <param name="libraryToPopulate"></param>
		/// <returns></returns>
		public static async Task GetArtistAlbumChildrenAsync( ArtistAlbum artistAlbumToPopulate ) =>
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( artistAlbumToPopulate );

		/// <summary>
		/// Update the database with any changes to this ArtistAlbum
		/// </summary>
		/// <param name="artistAlbum"></param>
		/// <returns></returns>
		public static async Task UpdateArtistAlbumAsync( ArtistAlbum artistAlbum ) =>
			await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( artistAlbum );

		/// <summary>
		/// Insert a new Artist in the database
		/// </summary>
		/// <param name="artist"></param>
		/// <returns></returns>
		public static async Task AddArtistAsync( Artist artist ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( artist );

		/// <summary>
		/// Update the database with any changes to this Artist
		/// </summary>
		/// <param name="artistToUpdate"></param>
		/// <returns></returns>
		public static async Task UpdateArtistAsync( Artist artistToUpdate ) => 
			await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( artistToUpdate );

		/// <summary>
		/// Insert a new Song in the database
		/// </summary>
		/// <param name="song"></param>
		/// <returns></returns>
		public static async Task AddSongAsync( Song song ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( song );

		/// <summary>
		/// Get an ArtistAlbum entry givien its id
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static async Task<ArtistAlbum> GetArtistAlbumAsync( int artistAlbumId ) => 
			await ConnectionDetailsModel.AsynchConnection.GetAsync<ArtistAlbum>( artistAlbumId );

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
		/// Get the artist from its id
		/// </summary>
		/// <param name="artistId"></param>
		/// <returns></returns>
		public static async Task<Artist> GetArtistAsync( int artistId ) => await ConnectionDetailsModel.AsynchConnection.GetAsync<Artist>( artistId );

		/// <summary>
		/// Delete the specified list of songs from the database
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static async Task DeleteSongsAsync( List< Song > songsToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( songsToDelete );

		/// <summary>
		/// Get a list of all the songs associated the specified ArtistAlbum
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetSongsReferencingArtistAlbumAsync( int artistAlbumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbumId ) ).ToListAsync();

		/// <summary>
		/// Delete the specified ArtistAlbum entry
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static async Task DeleteArtistAlbumAsync( ArtistAlbum albumToDelete ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( albumToDelete );

		/// <summary>
		/// Get a list of all the ArtistAlbums associated with the specified Album
		/// </summary>
		/// <param name="albumId"></param>
		/// <returns></returns>
		public static async Task<List<ArtistAlbum>> GetArtistAlbumsReferencingAlbumAsync( int albumId ) =>
			 await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().Where( artAlbum => ( artAlbum.AlbumId == albumId ) ).ToListAsync();

		/// <summary>
		/// Get a list of all the ArtistAlbums associated with the specified Artist
		/// </summary>
		/// <param name="artistId"></param>
		/// <returns></returns>
		public static async Task<List<ArtistAlbum>> GetArtistAlbumsReferencingArtistAsync( int artistId ) =>
			 await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().Where( artAlbum => (artAlbum.ArtistId == artistId ) ).ToListAsync();

		/// <summary>
		/// Delete the specified Artist
		/// </summary>
		/// <param name="artistId"></param>
		/// <returns></returns>
		public static async Task DeleteArtistAsync( int artistId ) =>
			await ConnectionDetailsModel.AsynchConnection.DeleteAsync( await ConnectionDetailsModel.AsynchConnection.GetAsync<Artist>( artistId ) );
	}
}