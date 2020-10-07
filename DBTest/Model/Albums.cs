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

			// Need to wait for the Album to be added to ensrure that its ID is available
			await AlbumAccess.AddAlbumAsync( albumToAdd );
		}

		/// <summary>
		/// Delete the specified Album from the storage and the collections
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static void DeleteAlbum( Album albumToDelete )
		{
			// No need to wait for the delete
			AlbumAccess.DeleteAlbumAsync( albumToDelete );
			AlbumCollection.Remove( albumToDelete );
			IdLookup.Remove( albumToDelete.Id );
		}

		/// <summary>
		/// Set or clear the played falg in the specified Album
		/// </summary>
		/// <param name="albumToUpdate"></param>
		/// <param name="newState"></param>
		public static void SetPlayedFlag( Album albumToUpdate, bool newState )
		{
			albumToUpdate.Played = newState;

			// No need to wait for the storage to complete
			AlbumAccess.UpdateAlbumAsync( albumToUpdate );
		}

		/// <summary>
		/// Get an album from with the specified name, artist name and library
		/// </summary>
		/// <param name="albumName"></param>
		/// <param name="artistName"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Album GetAlbumInLibrary( string albumName, string artistName, int libraryId ) => 
			AlbumCollection.Where( album => ( album.LibraryId == libraryId ) && ( album.Name == albumName ) && ( album.ArtistName == artistName ) ).FirstOrDefault();

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