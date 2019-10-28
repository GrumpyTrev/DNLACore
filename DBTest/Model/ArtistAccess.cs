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
		public static async Task < List< Artist > > GetArtistDetailsAsync( int libraryId )
		{
			Library songLibrary = await ConnectionDetailsModel.AsynchConnection.GetAsync<Library>( libraryId );
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync<Library>( songLibrary );

			// Get all of the artist details from the database
			for ( int artistIndex = 0; artistIndex < songLibrary.Artists.Count; ++artistIndex )
			{
				songLibrary.Artists[ artistIndex ] = await ConnectionDetailsModel.AsynchConnection.GetAsync<Artist>( songLibrary.Artists[ artistIndex ].Id );
			}

			return songLibrary.Artists;
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static void GetArtistContents( Artist theArtist )
		{
			ConnectionDetailsModel.SynchConnection.GetChildren<Artist>( theArtist );

			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				ConnectionDetailsModel.SynchConnection.GetChildren<ArtistAlbum>( artistAlbum );
			}
		}
	}
}