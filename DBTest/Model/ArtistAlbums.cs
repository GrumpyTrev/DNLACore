using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Artists class holds a collection of all the Artists entries read from storage.
	/// It allows access to Artists entries by Id and automatically persists changes back to storage
	/// </summary>	
	static class ArtistAlbums
	{
		/// <summary>
		/// Get the Artists collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( ArtistAlbumCollection == null )
			{
				// Get the current set of albums and form the lookup tables
				ArtistAlbumCollection = await ArtistAlbumAccess.GetArtistAlbumsAsync();
				IdLookup = ArtistAlbumCollection.ToDictionary( alb => alb.Id );
			}
		}

		/// <summary>
		/// Add a new ArtistAlbum to the storage and the local collections
		/// </summary>
		/// <param name="artistAlbumToAdd"></param>
		public static async Task AddArtistAlbumAsync( ArtistAlbum artistAlbumToAdd )
		{
			ArtistAlbumCollection.Add( artistAlbumToAdd );
			IdLookup[ artistAlbumToAdd.Id ] = artistAlbumToAdd;

			// Need to wait for the ArtistAlbum to be added as that will set its ID
			await ArtistAlbumAccess.AddArtistAlbumAsync( artistAlbumToAdd );
		}

		/// <summary>
		/// Return the artist with the specified Id or null if not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static ArtistAlbum GetArtistAlbumById( int id ) => IdLookup.GetValueOrDefault( id );

		/// <summary>
		/// Delete the specified ArtistAlbum from the storage and the collections
		/// </summary>
		/// <param name="artistAlbumToDelete"></param>
		/// <returns></returns>
		public static void DeleteArtistAlbum( ArtistAlbum artistAlbumToDelete )
		{
			// No need to wait for the ArtistAlbum to be deleted from storage
			ArtistAlbumAccess.DeleteArtistAlbumAsync( artistAlbumToDelete );
			ArtistAlbumCollection.Remove( artistAlbumToDelete );
			IdLookup.Remove( artistAlbumToDelete.Id );
		}

		/// <summary>
		/// The set of ArtistAlbums currently held in storage
		/// </summary>
		public static List<ArtistAlbum> ArtistAlbumCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by id
		/// </summary>
		private static Dictionary<int, ArtistAlbum> IdLookup { get; set; } = null;
	}
}