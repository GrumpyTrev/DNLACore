using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	static class ArtistsController
	{
		/// <summary>
		/// Get the Artist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetArtistsAsync( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( ( ArtistsViewModel.Artists == null ) || ( LibraryId != libraryId ) )
			{
				// New data is required
				LibraryId = libraryId;
				ArtistsViewModel.Artists = await ArtistAccess.GetArtistDetailsAsync( DatabasePath, LibraryId );

				// Sort the list of artists by name
				ArtistsViewModel.Artists.Sort( ( a, b ) => {
					// Do a normal comparison, except remove a leading 'The ' before comparing
					string artistA = ( a.Name.ToUpper().StartsWith( "THE " ) == true ) ? a.Name.Substring( 4 ) : a.Name;
					string artistB = ( b.Name.ToUpper().StartsWith( "THE " ) == true ) ? b.Name.Substring( 4 ) : b.Name;

					return artistA.CompareTo( artistB );
				} );

				// Work out the section indexes for the sorted data
				ArtistsViewModel.AlphaIndex = new Dictionary<string, int>();
				int index = 0;
				foreach ( Artist artist in ArtistsViewModel.Artists )
				{
					string key = artist.Name[ 0 ].ToString();
					if ( ArtistsViewModel.AlphaIndex.ContainsKey( key ) == false )
					{
						ArtistsViewModel.AlphaIndex[ key ] = index;
					}
					index++;
				}
			}

			// Publish the data
			new ArtistsDataAvailableMessage().Send();
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static void GetArtistContents( Artist theArtist )
		{
			ArtistAccess.GetArtistContents( theArtist, DatabasePath );

			// Sort the albums alphabetically
			theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			// Sort the songs by track number
			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}

			// Now all the ArtistAlbum and Song entries have been read form a single list from them
			theArtist.EnumerateContents();
		}

		/// <summary>
		/// The database file path
		/// </summary>
		public static string DatabasePath { private get; set; }

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		private static int LibraryId { get; set; } = -1;
	}
}