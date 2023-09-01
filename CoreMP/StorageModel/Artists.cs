using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Artists class holds a collection of all the Artists entries read from storage.
	/// It allows access to Artists entries by Id and automatically persists changes back to storage
	/// </summary>	
	internal static class Artists
	{
		/// <summary>
		/// Get the Artists collection from storage
		/// </summary>
		/// <returns></returns>
		public static void CollectionLoaded() => IdLookup = ArtistCollection.ToDictionary( art => art.Id );

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
			// Need to wait for the Artist to be added so that its Id is available
			await ArtistCollection.AddAsync( artistToAdd );
			IdLookup[ artistToAdd.Id ] = artistToAdd;
		}

		/// <summary>
		/// Delete the specified Artist from the storage and the collections
		/// </summary>
		/// <param name="artistAlbumToDelete"></param>
		/// <returns></returns>
		public static void DeleteArtist( Artist artistToDelete )
		{
			ArtistCollection.Remove( artistToDelete );
			IdLookup.Remove( artistToDelete.Id );
		}

		/// <summary>
		/// Delete the specified Artists from the storage and the collections
		/// </summary>
		/// <param name="artistsToDelete"></param>
		public static void DeleteArtists( IEnumerable< Artist> artistsToDelete )
		{
			foreach ( Artist artistToDelete in artistsToDelete )
			{
				ArtistCollection.Remove( artistToDelete );
				IdLookup.Remove( artistToDelete.Id );
			}
		}

		/// <summary>
		/// The set of Artists currently held in storage
		/// </summary>
		public static ModelCollection<Artist> ArtistCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by id
		/// </summary>
		private static Dictionary<int, Artist> IdLookup { get; set; } = null; 
	}
}
