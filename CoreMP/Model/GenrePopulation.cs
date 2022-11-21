using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CoreMP
{
	public partial class GenrePopulation
	{
		/// <summary>
		/// The list of genres extracted from the delimited string
		/// </summary>
		[Ignore]
		public List<string> Genres
		{
			get
			{
				if ( genres == null )
				{
					genres = GenreString.Split( ';' ).ToList();
				}

				return genres;
			}
		}

		/// <summary>
		/// Add a collection of genre string to this population
		/// </summary>
		/// <param name="genresToAdd"></param>
		public void AddGenres( IEnumerable<string> genresToAdd )
		{
			Genres.AddRange( genresToAdd );

			// Reform the delimited string and save it to storage
			GenreString = string.Join( ';', Genres );

			// No need to wait for this
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		/// <summary>
		/// The list of genres extracted from the delimited string
		/// </summary>
		private List<string> genres = null;
	}
}
