using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Linq;
using SQLiteNetExtensions.Extensions;

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
		public static async Task<List<Artist>> GetArtistDetailsAsync( int libraryId, Tag currentFilter )
		{
			List<Artist> artists = null;
			if ( currentFilter == null )
			{
				artists = ( await ConnectionDetailsModel.AsynchConnection.GetWithChildrenAsync<Library>( libraryId ) ).Artists;
			}
			else
			{
				// Access artists that have albums that are tagged with the current tag
				// For all TagAlbums in current tag get the ArtistAlbum (from the AlbumId) and the Artists 

				// First of all form a list of all the album identities in the selected filter
				List<int> albumIds = currentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToList();

				// Now get all the artist identities of the albums that are tagged
				List< int > artistIds = ( await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().
					Where( aa => ( albumIds.Contains( aa.AlbumId ) == true ) ).ToListAsync() ).Select( aa => aa.ArtistId ).ToList();

				// Now get the Artists from the list of artist ids
				artists = await ConnectionDetailsModel.AsynchConnection.Table<Artist>().
					Where( art => ( art.LibraryId == libraryId ) && ( artistIds.Contains( art.Id ) == true ) ).ToListAsync();
			}

			return artists;
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// Get the collection of ArtistAlbums and then the songs from each of those
		/// </summary>
		/// <param name="theArtist"></param>
		public static async Task GetArtistContentsAsync( Artist theArtist, Tag currentFilter )
		{
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( theArtist );

			if ( currentFilter != null )
			{
				// Remove albums that are not tagged

				// First of all form a list of all the album identities in the selected filter. This could be cached somewhere
				List<int> albumIds = currentFilter.TaggedAlbums.Select( ta => ta.AlbumId ).ToList();

				theArtist.ArtistAlbums.RemoveAll( aa => albumIds.Contains( aa.AlbumId ) == false );
			}

			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( artistAlbum );
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
		/// Insert a new Album in the database
		/// </summary>
		/// <param name="album"></param>
		/// <returns></returns>
		public static async Task AddAlbumAsync( Album album ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( album );

		/// <summary>
		/// Update the database with any changes to this Album
		/// </summary>
		/// <param name="album"></param>
		/// <returns></returns>
		public static async Task UpdateAlbumAsync( Album album ) => await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( album );

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
			await ConnectionDetailsModel.AsynchConnection.GetAsync< ArtistAlbum >( artistAlbumId );
	}
}