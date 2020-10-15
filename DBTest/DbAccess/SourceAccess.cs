using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The SourceAccess class is used to access and change Source data via the database
	/// </summary>
	static class SourceAccess
	{
		/// <summary>
		/// Get all the Sources in the data base
		/// </summary>
		public static async Task<List<Source>> GetAllSourcesAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Source>().ToListAsync();

		/// <summary>
		/// Update the specified source
		/// </summary>
		/// <param name="sourceToUpdate"></param>
		public static async void UpdateSourceAsync( Source sourceToUpdate ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( sourceToUpdate );
	}
}