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
		public static Album GetAlbumById( int id )
		{
			Album albumFound = null;

			if ( IdLookup.TryGetValue( id, out Album value ) == true )
			{
				albumFound = value;
			}

			return albumFound;
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