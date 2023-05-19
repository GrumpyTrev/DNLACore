using System.Collections.Generic;
using System.Linq;

namespace CoreMP
{
	/// <summary>
	/// The Population class holds the Genres and associated Albums for an Autoplay population
	/// </summary>
	public class Population
	{
		/// <summary>
		/// Create a new Population class and initialise it from the provided genres and albums
		/// </summary>
		/// <param name="seed"></param>
		public Population( GenrePopulation seed, IEnumerable<string> genres, IEnumerable<Album> albums )
		{
			SeedPopulation = seed;
			Genres = genres.ToList();
			Albums = albums.ToList();
		}

		/// <summary>
		/// Add the specified genres and albums to this popultion.
		/// Update the SeedPopulation with the new genres so that they can be persisted
		/// </summary>
		/// <param name="genres"></param>
		/// <param name="albums"></param>
		public void Add( IEnumerable<string> genres, IEnumerable<Album> albums )
		{
			SeedPopulation.AddGenres( genres );
			Genres.AddRange( genres );
			Albums.AddRange( albums );
		}

		/// <summary>
		/// The set of Genre string associated with this population
		/// </summary>
		public List<string> Genres { get; } = null;

		/// <summary>
		/// The set of Albums associated with this population
		/// </summary>
		public List<Album> Albums { get; } = null;

		/// <summary>
		/// The GenrePopulation used to initialise this Population and to which any changes to the Population are stored
		/// </summary>
		public GenrePopulation SeedPopulation { get; } = null;
	}
}
