using System.Collections.Generic;
using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;

namespace DBTest
{
	/// <summary>
	/// The LibraryAccess class is used to access and change Library data via the database
	/// </summary>
	static class LibraryAccess
	{
		/// <summary>
		/// Get all the libraries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Library>> GetLibrariesAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Library>().ToListAsync();

		/// <summary>
		/// Return the name of the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task<string> GetLibraryNameAsync( int libraryId ) =>
			( await ConnectionDetailsModel.AsynchConnection.GetAsync<Library>( libraryId ) ).Name;

		/// <summary>
		/// Clear the contents of the specified library
		/// </summary>
		/// <param name="libraryToClear"></param>
		/// <returns></returns>
		public static async Task ClearLibraryAsync( Library libraryToClear )
		{
			int libId = libraryToClear.Id;

			// Delete all the artists in the library and their associated ArtistAlbum entries
			List< Artist > artists = await ConnectionDetailsModel.AsynchConnection.Table<Artist>().Where( art => art.LibraryId == libId ).ToListAsync();
			await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( artists );
			foreach ( Artist artist in artists )
			{
				List< ArtistAlbum > aristAlbums = await ConnectionDetailsModel.AsynchConnection.Table<ArtistAlbum>().Where( aa => aa.ArtistId == artist.Id ).ToListAsync();
				await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( aristAlbums );
			}

			// Delete all the albums in the library and any tags associated with them
			List< Album > albums = await ConnectionDetailsModel.AsynchConnection.Table< Album >().Where( alb => alb.LibraryId == libId ).ToListAsync();
			await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( albums );
			foreach ( Album album in albums )
			{
				List<TaggedAlbum> taggedAlbums = await ConnectionDetailsModel.AsynchConnection.Table<TaggedAlbum>().Where( ta => ta.AlbumId == album.Id ).ToListAsync();
				await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( taggedAlbums );
			}

			// Clear all the playlist contents and then delete all the playlists except for the now playing playlist
			List<Playlist> playlists = await ConnectionDetailsModel.AsynchConnection.Table<Playlist>().Where( play => play.LibraryId == libId ).ToListAsync();
			foreach ( Playlist playlist in playlists )
			{
				List<PlaylistItem> items = await ConnectionDetailsModel.AsynchConnection.Table<PlaylistItem>().Where( pli => pli.PlaylistId == playlist.Id ).ToListAsync();
				await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( items );

				if ( playlist.Name != NowPlayingController.NowPlayingPlaylistName )
				{
					await ConnectionDetailsModel.AsynchConnection.DeleteAsync( playlist );
				}
			}

			// Delete all the songs in each of the sources associated with the library
			List<Source> sources = Sources.GetSourcesForLibrary( libId );
			foreach ( Source source in sources )
			{
				List< Song > songs = await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( s => s.SourceId == source.Id ).ToListAsync();
				await ConnectionDetailsModel.AsynchConnection.DeleteAllAsync( songs );
			}
		}
	}
}