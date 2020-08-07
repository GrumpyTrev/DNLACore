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
		public static async Task<string> GetGenreNameAsync( int id )
		{
			string name = "";

			await InitialiseCollectionAsync();

			if ( IdLookup.TryGetValue( id, out Genre value ) == true )
			{
				name = value.Name;
			}

			return name;
		}

		/// <summary>
		/// Return the Genre with the specified Id or null if not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static async Task<Genre> GetGenreByIdAsync( int id )
		{
			Genre genreFound = null;

			await InitialiseCollectionAsync();

			if ( IdLookup.TryGetValue( id, out Genre value ) == true )
			{
				genreFound = value;
			}

			return genreFound;
		}

		/// <summary>
		/// Return the Genre with the specified name. If no such Genre optionally create one
		/// </summary>
		/// <param name="name"></param>
		/// <param name="createIfNotFound"></param>
		/// <returns></returns>
		public static async Task<Genre> GetGenreByNameAsync( string name, bool createIfNotFound = false )
		{
			Genre genreFound = null;

			await InitialiseCollectionAsync();

			if ( NameLookup.TryGetValue( name, out Genre value ) == true )
			{
				genreFound = value;
			}
			else
			{
				if ( createIfNotFound == true )
				{
					genreFound = new Genre() { Name = name };
					await FilterAccess.AddGenre( genreFound );
					GenreCollection.Add( genreFound );
					IdLookup[ genreFound.Id ] = genreFound;
					NameLookup[ genreFound.Name ] = genreFound;
				}
			}

			return genreFound;
		}

		/// <summary>
		/// Read the Genre entries from storage if not already read
		/// </summary>
		/// <returns></returns>
		private static async Task InitialiseCollectionAsync()
		{
			if ( GenreCollection == null )
			{
				// Get the current set of genres and form the lookup tables
				GenreCollection = await FilterAccess.GetGenresAsync();
				IdLookup = GenreCollection.ToDictionary( gen => gen.Id );
				NameLookup = GenreCollection.ToDictionary( gen => gen.Name );
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

		/// <summary>
		/// Dictionary to allow a Genre to be accessed by its name
		/// </summary>
		private static Dictionary<string, Genre> NameLookup { get; set; } = null;
	}
}