using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	static class AutoplayAccess
	{
		/// <summary>
		/// Get all the Autoplay entries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Autoplay>> GetAutoplaysAsync() => await ConnectionDetailsModel.AsynchConnection.Table<Autoplay>().ToListAsync();

		/// <summary>
		/// Insert a new Autoplay in the database
		/// </summary>
		/// <param name="autoplay"></param>
		/// <returns></returns>
		public static async Task AddAutoplayAsync( Autoplay autoplay ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( autoplay );

		/// <summary>
		/// Update the database with any changes to this Autoplay
		/// </summary>
		/// <param name="autoplay"></param>
		/// <returns></returns>
		public static async void UpdateAutoplayAsync( Autoplay autoplay ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( autoplay );
	}
}