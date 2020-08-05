using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Genres class holds a collection of all the Genre entries read from storage.
	/// It allows access to Genre entries by Id and Name and automatically persists changes back to storage
	/// </summary>
	static class Genres
	{
		/// <summary>
		/// Return the name of the genre with the specified id. Return an empty string if no such id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static async Task<string> GetGenreName( int id )
		{
			string name = "";

			await InitialiseCollection();

			if ( IdLookup.TryGetValue( id, out Genre value ) == true )
			{
				name = value.Name;
			}

			return name;
		}

		/// <summary>
		/// Read the Genre entries from storage if not already read
		/// </summary>
		/// <returns></returns>
		private static async Task InitialiseCollection()
		{
			if ( GenreCollection == null )
			{
				// Get the current set of genres and form the lookup tables
				GenreCollection = await FilterAccess.GetGenresAsync();
				IdLookup = GenreCollection.ToDictionary( gen => gen.Id );
			}
		}

		/// <summary>
		/// The set of Genres currently held in storage
		/// </summary>
		private static List<Genre> GenreCollection { get; set; } = null;

		/// <summary>
		/// Dictionary to allow a Genre to be accessed by its Id
		/// </summary>
		private static Dictionary<int, Genre> IdLookup { get; set; } = null;
	}
}