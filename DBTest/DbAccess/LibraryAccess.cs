using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The LibraryAccess class is used to access and change Library data via the database
	/// </summary>
	static class LibraryAccess
	{
		/// <summary>
		/// Get all the libraries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Library>> GetLibrariesAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Library>().ToListAsync();

		/// <summary>
		/// Return the name of the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task<string> GetLibraryNameAsync( int libraryId ) =>
			( await ConnectionDetailsModel.AsynchConnection.GetAsync<Library>( libraryId ) ).Name;
	}
}