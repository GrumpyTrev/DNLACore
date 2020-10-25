using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	static class AutoplayAccess
	{
		/// <summary>
		/// Get all the libraries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Autoplay>> GetAutoplaysAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Autoplay>().ToListAsync();
	}
}