using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Albums class holds a collection of all the Albums entries read from storage.
	/// It allows access to Albums entries by Id and automatically persists changes back to storage
	/// </summary>	
	internal class Albums
	{
		/// <summary>
		/// Called when the Albums have been read from storage
		/// </summary>
		/// <returns></returns>
		public static void CollectionLoaded() => IdLookup = AlbumCollection.ToDictionary( alb => alb.Id );

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
			await AlbumCollection.AddAsync( albumToAdd );
			IdLookup[ albumToAdd.Id ] = albumToAdd;
		}

		/// <summary>
		/// Delete the specified Album from the collections
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static void DeleteAlbum( Album albumToDelete )
		{
			_ = AlbumCollection.Remove( albumToDelete );
			_ = IdLookup.Remove( albumToDelete.Id );

			// Notify the rest of the system about this deletion
			NotificationHandler.NotifyPropertyChanged( albumToDelete );
		}

		/// <summary>
		/// Delete the specified Albums from the collections
		/// </summary>
		/// <param name="albumToDelete"></param>
		/// <returns></returns>
		public static void DeleteAlbums( IEnumerable<Album> albumsToDelete )
		{
			foreach ( Album albumToDelete in albumsToDelete )
			{
				DeleteAlbum( albumToDelete );
			}
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
		public static ModelCollection<Album> AlbumCollection { get; set; } = null;

		/// <summary>
		/// Lookup tables indexed by album id
		/// </summary>
		private static Dictionary<int, Album> IdLookup { get; set; } = null; 
	}
}
