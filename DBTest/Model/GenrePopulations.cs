﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The GenrePopulations class holds a collection of GenrePopulations items read from storage
	/// </summary>
	static class GenrePopulations
	{
		/// <summary>
		/// Get the Albums collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( GenrePopulationCollection == null )
			{
				// Get the current set of GenrePopulation
				GenrePopulationCollection = await GenrePopulationAccess.GetGenrePopulationsAsync();
			}
		}

		/// <summary>
		/// Create a GenrePopulation and add it to the collection.
		/// </summary>
		/// <param name="population"></param>
		public static GenrePopulation CreatePopulation( int autoplayId, int index, IEnumerable<string> genres )
		{
			GenrePopulation newPopulation = new GenrePopulation() { AutoplayId = autoplayId, Index = index, GenreString = string.Join( ";", genres ) };

			GenrePopulationCollection.Add( newPopulation );

			// No need to wait for this
			GenrePopulationAccess.AddGenrePopulationAsync( newPopulation );

			return newPopulation;
		}

		/// <summary>
		/// Remove the specified GenrePopulation from the collection and from storage.
		/// </summary>
		/// <param name="population"></param>
		public static void RemovePopulation( GenrePopulation population )
		{
			GenrePopulationCollection.Remove( population );

			// No need to wait for this
			GenrePopulationAccess.DeleteGenrePopulationAsync( population );
		}

		/// <summary>
		/// The set of GenrePopulations currently held in storage
		/// </summary>
		public static List<GenrePopulation> GenrePopulationCollection { get; set; } = null;
	}
}