using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;
using SQLiteNetExtensions.Extensions;

namespace DBTest
{
	/// <summary>
	/// The LibraryAccess class is used to access and change Library data via the database
	/// </summary>
	static class LibraryAccess
	{
		/// <summary>
		/// Get all the sources associated with the library
		/// </summary>
		public static async Task<List<Source>> GetSourcesAsync( int libraryId, bool withChildren = false )
		{
			List<Source> sources = null;

			// Get all sources for the specified library
			if ( withChildren == false )
			{
				sources = await ConnectionDetailsModel.AsynchConnection.Table<Source>().Where( d => ( d.LibraryId == libraryId ) ).ToListAsync();
			}
			else
			{
				sources = await ConnectionDetailsModel.AsynchConnection.GetAllWithChildrenAsync<Source>( d => ( d.LibraryId == libraryId ) );
			}

			return sources;
		}

		/// <summary>
		/// Get all the libraries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Library>> GetLibrariesAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Library>().ToListAsync();

		/// <summary>
		/// Get the children entries for this library
		/// </summary>
		/// <param name="libraryToPopulate"></param>
		/// <returns></returns>
		public static async Task GetLibraryChildrenAsync( Library libraryToPopulate ) => 
			await ConnectionDetailsModel.AsynchConnection.GetChildrenAsync( libraryToPopulate );

		/// <summary>
		/// Update the database with any changes to this library
		/// </summary>
		/// <param name="libraryToUpdate"></param>
		/// <returns></returns>
		public static async Task UpdateLibraryAsync( Library libraryToUpdate ) =>
			await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( libraryToUpdate );

		/// <summary>
		/// Update the database with any changes to this source
		/// </summary>
		/// <param name="sourceToUpdate"></param>
		/// <returns></returns>
		public static async Task UpdateSourceAsync( Source sourceToUpdate ) =>
			await ConnectionDetailsModel.AsynchConnection.UpdateWithChildrenAsync( sourceToUpdate );
	}
}