using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Albums class holds a collection of all the Albums entries read from storage.
	/// It allows access to Albums entries by Id and automatically persists changes back to storage
	/// </summary>	
	static class Albums
	{
		/// <summary>
		/// Get the Albums collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( AlbumCollection == null )
			{
				// Get the current set of albums and form the lookup tables
				AlbumCollection = await AlbumAccess.GetAllAlbumsAsync();
				IdLookup = AlbumCollection.ToDictionary( alb => alb.Id );
			}
		}

		/// <summary>
		/// Return the Album with the specified Id or null if not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Album GetAlbumById( int id ) => IdLookup.GetValueOrDefault( id );

		/// <summary>
		/// Add a new album to the storage and the local collections
		/// </summary>
		/// <param name="albumToAdd"></param>
		public static async Task AddAlbumAsync( Album albumToAdd )
		{
			AlbumCollection.Add( albumToAdd );
			IdLookup[ albumToAdd.Id ] = albumToAdd;
			await AlbumAccess.AddAlbumAsync( albumToAdd );
		}

		/// <summary>
		/// Delete the specified Album from the storage and the collections
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static async Task DeleteAlbumAsync( Album albumToDelete )
		{
			await AlbumAccess.DeleteAlbumAsync( albumToDelete );
			AlbumCollection.Remove( albumToDelete );
			IdLookup.Remove( albumToDelete.Id );
		}

		/// <summary>
		/// The set of Albums currently held in storage
		/// </summary>
		public static List<Album> AlbumCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by album id
		/// </summary>
		private static Dictionary<int, Album> IdLookup { get; set; } = null; 
	}
}