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
		/// The list of genre names obtained if necessary from the delimited list obtained from storage
		/// </summary>
		public List<string> GenreNames
		{
			get
			{
				if ( genres == null )
				{
					genres = Genres.Split( ';' ).ToList();
				}

				return genres;
			}
		}

		/// <summary>
		/// The list of genre names
		/// </summary>
		private List<string> genres = null;
	}
}