using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Artists class holds a collection of all the Artists entries read from storage.
	/// It allows access to Artists entries by Id and automatically persists changes back to storage
	/// </summary>	
	internal static class ArtistAlbums
	{
		/// <summary>
		/// Get the Artists collection from storage
		/// </summary>
		public static void CollectionLoaded() => IdLookup = ArtistAlbumCollection.ToDictionary( alb => alb.Id );

		/// <summary>
		/// Add a new ArtistAlbum to the storage and the local collections
		/// </summary>
		/// <param name="artistAlbumToAdd"></param>
		public static async Task AddArtistAlbumAsync( ArtistAlbum artistAlbumToAdd )
		{
			// Need to wait for the ArtistAlbum to be added as that will set its ID
			await ArtistAlbumCollection.AddAsync( artistAlbumToAdd );
			IdLookup[ artistAlbumToAdd.Id ] = artistAlbumToAdd;
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
			ArtistAlbumCollection.Remove( artistAlbumToDelete );
			IdLookup.Remove( artistAlbumToDelete.Id );
		}

		/// <summary>
		/// Delete all the specified ArtistAlbum from the storage and the collections
		/// </summary>
		/// <param name="artistAlbumsToDelete"></param>
		/// <returns></returns>
		public static void DeleteArtistAlbums( IEnumerable<ArtistAlbum> artistAlbumsToDelete )
		{
			foreach( ArtistAlbum artAlbum in artistAlbumsToDelete )
			{
				DeleteArtistAlbum( artAlbum );
			}
		}

		/// <summary>
		/// The set of ArtistAlbums currently held in storage
		/// </summary>
		public static ModelCollection<ArtistAlbum> ArtistAlbumCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by id
		/// </summary>
		private static Dictionary<int, ArtistAlbum> IdLookup { get; set; } = null;
	}
}
