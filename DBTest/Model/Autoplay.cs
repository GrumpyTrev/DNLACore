﻿using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The Autoplay class contains auto-playlist configuration details
	/// </summary>
	partial class Autoplay
	{
		/// <summary>
		/// Intialise the Population list from the GenrePopulation records
		/// </summary>
		public void InitialisePopulations()
		{
			// Get all the GenrePopulation records associated with this Autoplay and order by the population index
			IEnumerable<GenrePopulation> genrePopulations = GenrePopulations.GenrePopulationCollection.Where( gp => gp.AutoplayId == Id ).OrderBy( gp => gp.Index );

			// Create a Population for each GenrePopulation and add it to the list
			foreach ( GenrePopulation population in genrePopulations )
			{
				// If this is the original set of genres then save it separately
				if ( population.Index == -1 )
				{
					SeedGenres = population;
				}
				else
				{
					// Get all the new genres and albums
					List<string> populationGenres = population.Genres.Where( gen => GenresAlreadyIncluded.Add( gen ) == true ).ToList();
					IEnumerable<Album> populationAlbums = GetAlbumsFromGenres( populationGenres );

					Populations.Add( new Population( population, populationGenres, populationAlbums ) );
				}
			}
		}

		/// <summary>
		/// Clear the Population and SeedGenres entries
		/// </summary>
		public void Clear()
		{
			foreach ( Population population in Populations )
			{
				GenrePopulations.RemovePopulation( population.SeedPopulation );
			}

			if ( SeedGenres != null )
			{
				GenrePopulations.RemovePopulation( SeedGenres );
				SeedGenres = null;
			}

			Populations.Clear();
			GenresAlreadyIncluded.Clear();
			AlbumsAlreadyIncluded.Clear();
		}

		public void AddToPopulation( int populationIndex, IEnumerable<string> genres )
		{
			// Make sure that there are some genres and albums that have not been seen before
			// Use a List here as having the GenresAlreadyIncluded modifier in the .where seems to cause problems with Linq lazy evaluation
			List<string> newGenres = genres.Where( gen => GenresAlreadyIncluded.Add( gen ) == true ).ToList();

			if ( newGenres.Count > 0 )
			{
				List<Album> newAlbums = GetAlbumsFromGenres( newGenres );

				if ( newAlbums.Count > 0 )
				{
					// Add these entries to the next population to the one just used
					if ( populationIndex == ( Populations.Count - 1 ) )
					{
						// A new population is required
						Populations.Add( new Population( GenrePopulations.CreatePopulation( Id, Populations.Count, newGenres ), newGenres, newAlbums ) );
					}
					else
					{
						// Add the genres and albums to the existing Population
						Populations[ populationIndex + 1 ].Add( newGenres, newAlbums );
					}
				}
			}
		}

		/// <summary>
		/// Generate a new GenrePopulation to contain the seed genres and add it to the GenrePopulation collection
		/// </summary>
		/// <param name="genres"></param>
		public void SaveSeedGenres( IEnumerable<string> genres ) => SeedGenres = GenrePopulations.CreatePopulation( Id, -1, genres );

		/// <summary>
		/// Update this Autoplay's options with those in the new Autoplay
		/// </summary>
		/// <param name="newAutoplay"></param>
		public void UpdateOptions( Autoplay newAutoplay )
		{
			bool optionsChanged = ( newAutoplay.Spread != Spread ) || ( newAutoplay.Target != Target ) || ( newAutoplay.Weight != Weight );
			Spread = newAutoplay.Spread;
			Target = newAutoplay.Target;
			Weight = newAutoplay.Weight;

			if ( optionsChanged == true )
			{
				// Update the record. No need to wait.
				AutoplayAccess.UpdateAutoplayAsync( this );
			}
		}

		/// <summary>
		/// Find all the albums associated with a list of genre names
		/// </summary>
		/// <param name="genres"></param>
		/// <returns></returns>
		private List<Album> GetAlbumsFromGenres( IEnumerable<string> genres )
		{
			List<Album> albums = new List<Album>();

			// Get all the albums associated with the genres
			foreach ( string genre in genres )
			{
				albums.AddRange( FilterManagementModel.GenreTags.Tags.Single( tag => tag.Name == genre ).TaggedAlbums
					.Where( alb => AlbumsAlreadyIncluded.Add( alb.AlbumId ) == true ).Select( ta => ta.Album ) );
			}

			return albums;
		}

		/// <summary>
		/// The Populations associated with this Autoplay record
		/// </summary>
		[Ignore]
		public List<Population> Populations { get; set; } = new List<Population>();

		/// <summary>
		/// The original set of genres
		/// </summary>
		[Ignore]
		public GenrePopulation SeedGenres { get; set; } = null;

		/// <summary>
		/// All of the Genres associated with this Autoplay in all populations
		/// </summary>
		[Ignore]
		public HashSet<string> GenresAlreadyIncluded { get; set; } = new HashSet<string>();

		/// <summary>
		/// All of the albums associated with this Autoplay in all populations
		/// </summary>
		[Ignore]
		public HashSet<int> AlbumsAlreadyIncluded { get; set; } = new HashSet<int>();
	}
}