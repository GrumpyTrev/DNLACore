using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Libraries class holds a collection of all the Library entries read from storage.
	/// It allows access to Library entries by Id and automatically persists changes back to storage
	/// </summary>	
	internal static class Libraries
	{
		/// <summary>
		/// Get the Library collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( LibraryCollection == null )
			{
				// Get the current set of libraries
				LibraryCollection = await DbAccess.LoadAsync<Library>();

				LibraryNames = LibraryCollection.Select( lib => lib.Name ).ToList();
			}
		}

		/// <summary>
		/// Get the index of the specified library accessed by library id
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static int Index( int libraryId ) => LibraryCollection.FindIndex( lib => ( lib.Id == libraryId ) );

		/// <summary>
		/// Get the specified Library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Library GetLibraryById( int libraryId ) => LibraryCollection.SingleOrDefault( lib => ( lib.Id == libraryId ) );

		/// <summary>
		/// Add a new library to the collection and to persistent storage
		/// </summary>
		/// <param name="newLibrary"></param>
		public static async Task AddLibraryAsync( Library newLibrary )
		{
			LibraryCollection.Add( newLibrary );

			// Need to wait for the Library to be added to ensure that its ID is available
			await DbAccess.InsertAsync( newLibrary );

			// Reform the library names collection
			LibraryNames = LibraryCollection.Select( lib => lib.Name ).ToList();
		}

		/// <summary>
		/// Delete the specified library from the local collection and the database
		/// </summary>
		/// <param name="libraryToDelete"></param>
		public static void DeleteLibrary( Library libraryToDelete )
		{
			LibraryCollection.Remove( libraryToDelete );
			DbAccess.DeleteAsync( libraryToDelete );
		}

		/// <summary>
		/// The set of Library entries currently held in storage
		/// </summary>
		public static List<Library> LibraryCollection { get; private set; } = null;

		/// <summary>
		/// The names of all the libraries
		/// </summary>
		public static List<string> LibraryNames { get; private set; } = null;
	}
}
