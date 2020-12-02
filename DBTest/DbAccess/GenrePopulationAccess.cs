using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	static class GenrePopulationAccess
	{
		/// <summary>
		/// Get all the GenrePopulation entries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<GenrePopulation>> GetGenrePopulationsAsync() => await ConnectionDetailsModel.AsynchConnection.Table<GenrePopulation>().ToListAsync();

		/// <summary>
		/// Insert a new GenrePopulation in the database
		/// </summary>
		/// <param name="population"></param>
		/// <returns></returns>
		public static async void AddGenrePopulationAsync( GenrePopulation population ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( population );

		/// <summary>
		/// Update the database with any changes to this GenrePopulation
		/// </summary>
		/// <param name="population"></param>
		/// <returns></returns>
		public static async void UpdateGenrePopulationAsync( GenrePopulation population ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( population );

		/// <summary>
		/// Delete the specified GenrePopulation
		/// </summary>
		/// <param name="population"></param>
		/// <returns></returns>
		public static async void DeleteGenrePopulationAsync( GenrePopulation population ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( population );
	}
}