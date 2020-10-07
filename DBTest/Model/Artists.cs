using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Artists class holds a collection of all the Artists entries read from storage.
	/// It allows access to Artists entries by Id and automatically persists changes back to storage
	/// </summary>	
	static class Artists
	{
		/// <summary>
		/// Get the Artists collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( ArtistCollection == null )
			{
				// Get the current set of albums and form the lookup tables
				ArtistCollection = await ArtistAccess.GetAllArtistsAsync();
				IdLookup = ArtistCollection.ToDictionary( art => art.Id );
			}
		}

		/// <summary>
		/// Return the artist with the specified Id or null if not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Artist GetArtistById( int id ) => IdLookup.GetValueOrDefault( id );

		/// <summary>
		/// Add a new artist to the storage and the local collections
		/// </summary>
		/// <param name="artistToAdd"></param>
		public static async Task AddArtistAsync( Artist artistToAdd )
		{
			ArtistCollection.Add( artistToAdd );
			IdLookup[ artistToAdd.Id ] = artistToAdd;

			// Need to wait for the Artist to be added so that its ID is availabel
			await ArtistAccess.AddArtistAsync( artistToAdd );
		}

		/// <summary>
		/// Delete the specified Artist from the storage and the collections
		/// </summary>
		/// <param name="artistAlbumToDelete"></param>
		/// <returns></returns>
		public static void DeleteArtist( Artist artistToDelete )
		{
			// No need to wait for the Artist to be removed from storage
			ArtistAccess.DeleteArtistAsync( artistToDelete );
			ArtistCollection.Remove( artistToDelete );
			IdLookup.Remove( artistToDelete.Id );
		}

		/// <summary>
		/// The set of Artists currently held in storage
		/// </summary>
		public static List<Artist> ArtistCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by id
		/// </summary>
		private static Dictionary<int, Artist> IdLookup { get; set; } = null; 
	}
}