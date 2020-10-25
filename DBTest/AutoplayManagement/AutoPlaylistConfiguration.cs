using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AutoPlaylistConfiguration class holds the configuration information required to generate a auotmatic playlist 
	/// </summary>
	class AutoPlaylistConfiguration
	{
		public AutoPlaylistConfiguration( )
		{
		}

		/// <summary>
		/// Initialise with items selected by the user
		/// </summary>
		/// <param name="selectedItems"></param>
		public async Task LoadConfigurationFromSelection( GroupedSelection selectedItems )
		{
			// If an Artist has been selected then the starting point for generation will be the albums associated with the Artist.
			// If an Album has been selected then that album will be the starting point.
			// If a Song has been selected then that song will be the starting point.
			selectedArtist = selectedItems.Artists.SingleOrDefault();
			if ( selectedArtist != null )
			{
				// Make sure that all the songs are available
				await selectedArtist.GetSongsAsync();

				// Form a list of all the songs and genres associated with this Artist
				foreach ( ArtistAlbum artistAlbum in selectedArtist.ArtistAlbums )
				{
					startingPopulation.AddRange( artistAlbum.Songs );
					selectedGenres.Add( artistAlbum.Album.Genre );
				}

				// Make genre list unique
				selectedGenres = selectedGenres.Distinct().ToList();
			}
			else
			{
				selectedAlbum = selectedItems.ArtistAlbums.SingleOrDefault();
				if ( selectedAlbum != null )
				{
					if ( selectedAlbum.Songs == null )
					{
						selectedAlbum.Songs = await SongAccess.GetArtistAlbumSongsAsync( selectedAlbum.Id );
					}

					startingPopulation.AddRange( selectedAlbum.Songs );
					selectedGenres.Add( selectedAlbum.Album.Genre );
				}
				else
				{
					selectedSong = selectedItems.Songs.SingleOrDefault();
					if ( selectedSong != null )
					{
						startingPopulation.Add( selectedSong );
						selectedGenres.Add( ArtistAlbums.GetArtistAlbumById( selectedSong.ArtistAlbumId ).Album.Genre );
					}
				}
			}
		}

		public async Task< List< Song > > FirstGeneration( int generationSize )
		{
			List<Song> generation = new List<Song>
			{
				// Take the first song from the original selection
				startingPopulation[ new Random().Next( 0, startingPopulation.Count ) ]
			};

			// For now only look at the selected Genres
			List<Song> population = new List<Song>();

			// Get all the albums that have one of the selected genres
			List<Album> albumPopulation = Albums.AlbumCollection.Where( album => selectedGenres.Contains( album.Genre ) ).ToList();

			// Populate all their song collections
			foreach ( Album album in albumPopulation )
			{
				await album.GetSongsAsync();
			}

			// Add all the songs to the population
			albumPopulation.ForEach( album => population.AddRange( album.Songs ) );

			Random generator = new Random();
			for ( int generationCount = 1; generationCount < generationSize; ++generationCount )
			{
				generation.Add( population[ generator.Next( 0, population.Count ) ] );
			}

			return generation;
		}

		private Artist selectedArtist = null;

		private ArtistAlbum selectedAlbum = null;

		private Song selectedSong = null;

		private List<Song> startingPopulation = new List<Song>();
		private List<string> selectedGenres = new List<string>();
	}
}