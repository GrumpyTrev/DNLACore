using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using SQLiteNetExtensions.Extensions;
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
		public static async Task < List< Artist > > GetArtistDetailsAsync( string databasePath, int libraryId )
		{
			SQLiteAsyncConnection dbAsynch = new SQLiteAsyncConnection( databasePath );

			Library songLibrary = await dbAsynch.GetAsync<Library>( libraryId );
			await dbAsynch.GetChildrenAsync<Library>( songLibrary );

			// Get all of the artist details from the database
			for ( int artistIndex = 0; artistIndex < songLibrary.Artists.Count; ++artistIndex )
			{
				songLibrary.Artists[ artistIndex ] = await dbAsynch.GetAsync<Artist>( songLibrary.Artists[ artistIndex ].Id );
			}

			return songLibrary.Artists;
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static void GetArtistContents( Artist theArtist, string databasePath )
		{
			using ( SQLiteConnection db = new SQLiteConnection( databasePath ) )
			{
				db.GetChildren<Artist>( theArtist );

				foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
				{
					db.GetChildren<ArtistAlbum>( artistAlbum );
				}
			}
		}
	}
}