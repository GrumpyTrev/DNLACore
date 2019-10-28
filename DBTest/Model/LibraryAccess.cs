using SQLite;
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
		/// Get all the sources associated with the library
		/// Should this be here or in the SourceAccess class?
		/// </summary>
		public static async Task<List<Source>> GetSourcesAsync( int libraryId )
		{
			// Get all the playlist except the Now Playing list
			AsyncTableQuery<Source> query = ConnectionDetailsModel.AsynchConnection.Table<Source>().Where( d => ( d.LibraryId == libraryId ) );

			return await query.ToListAsync();
		}
	}
}