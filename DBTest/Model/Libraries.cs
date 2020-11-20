using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Libraries class holds a collection of all the Library entries read from storage.
	/// It allows access to Library entries by Id and automatically persists changes back to storage
	/// </summary>	
	static class Libraries
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
				LibraryCollection = await LibraryAccess.GetLibrariesAsync();

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
		/// The set of Library entries currently held in storage
		/// </summary>
		public static List<Library> LibraryCollection { get; private set; } = null;

		/// <summary>
		/// The names of all the libraries
		/// </summary>
		public static List<string> LibraryNames { get; private set; } = null;
	}
}