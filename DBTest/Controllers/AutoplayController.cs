using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AutoplayController class is the controller for the AutoplayManagement
	/// </summary>
	static class AutoplayController
	{
		/// <summary>
		/// Get the Autoplay data associated with the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetAutoplays( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( AutoplayModel.LibraryId != libraryId )
			{
				// New data is required
				AutoplayModel.LibraryId = libraryId;

				// All Artists are read as part of the storage data. So wait until that is available and then carry out the rest of the 
				// initialisation
				StorageController.RegisterInterestInDataAvailable( StorageDataAvailable );
			}
		}

		/// <summary>
		/// Start filling the NowPlaying list with the first set of songs
		/// Save the set of genres as genre population 0 and find all the albums associated with those genres
		/// If a song has been specified then add that to the NowPlaying list first
		/// </summary>
		/// <param name="selectedSong"></param>
		/// <param name="genres"></param>
		public static async void StartAutoplayAsync( Song selectedSong, IEnumerable<string> genres )
		{
			if ( genreTags == null )
			{
				genreTags = FilterManagementModel.TagGroups.Single( tg => tg.Name == "Genre" );
			}

			// Clear any existing Genre/Album populations
			AutoplayModel.ClearModel();

			// Get all the albums associated with the genres
			IEnumerable<Album> albums = GetAlbumsFromGenres( genres );

			AutoplayModel.Populations.Add( new AutoplayModel.Population() { Genres = genres.ToList(), Albums = albums.ToList() } );

			AutoplayModel.GenresAlreadyIncluded = genres.ToHashSet();
			AutoplayModel.AlbumsAlreadyIncluded = albums.Select( alb => alb.Id ).ToHashSet();

			// Start generating songs
			List<Song> songs = new List<Song>();
			if ( selectedSong != null )
			{
				songs.Add( selectedSong );
			}

			await GenerateSongsAsync( songs );

			BaseController.AddSongsToNowPlayingList( songs, true );
		}

		private static async Task GenerateSongsAsync( List<Song> songs )
		{
			// For each generation select a song from the available populations
			Random generator = new Random();
			while ( songs.Count() < GenerationSize )
			{
				// First select a population
				// For now randomly from available populations
				int populationNumber = generator.Next( 0, AutoplayModel.Populations.Count );
				AutoplayModel.Population population = AutoplayModel.Populations[ populationNumber ];

				// Now choose an Album from that population
				Album album = population.Albums[ generator.Next( 0, population.Albums.Count ) ];

				// Make sure that all the songs are available for the album
				await album.GetSongsAsync();

				// Now add a song
				songs.Add( album.Songs[ generator.Next( 0, album.Songs.Count ) ] );

				// Check if the album just choosen is associated with any Genres that we have not encountered yet
				IEnumerable<string> newGenres = album.Genre.Split( ';' ).Where( gen => AutoplayModel.GenresAlreadyIncluded.Add( gen ) == true );

				if ( newGenres.Count() > 0 )
				{
					// Get all the albums associated with the genres that have not already been seen
					IEnumerable<Album> newAlbums = GetAlbumsFromGenres( newGenres ).Where( alb => AutoplayModel.AlbumsAlreadyIncluded.Add( alb.Id ) == true );

					if ( newAlbums.Count() > 0 )
					{
						AutoplayModel.Population nextPopulation = null;

						// Add these entries to the next population to the one just used
						if ( populationNumber == ( AutoplayModel.Populations.Count - 1 ) )
						{
							nextPopulation = new AutoplayModel.Population();
							AutoplayModel.Populations.Add( nextPopulation );
						}
						else
						{
							nextPopulation = AutoplayModel.Populations[ populationNumber + 1 ];
						}

						nextPopulation.Genres.AddRange( newGenres );
						nextPopulation.Albums.AddRange( newAlbums );
					}
				}
			}
		}

		/// <summary>
		/// Find all the albums associated with a list of genre names
		/// </summary>
		/// <param name="genres"></param>
		/// <returns></returns>
		private static IEnumerable<Album> GetAlbumsFromGenres( IEnumerable<string> genres )
		{
			List<Album> albums = new List<Album>();

			// Get all the albums associated with the genres
			foreach ( string genre in genres )
			{
				albums.AddRange( genreTags.Tags.Single( tag => tag.Name == genre ).TaggedAlbums.Select( ta => ta.Album ) );
			}

			//Return a distinct list of albums
			return albums.Distinct();
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static void StorageDataAvailable( object message )
		{
			AutoplayModel.CurrentAutoplay = Autoplays.AutoplayCollection.Where( auto => auto.LibraryId == AutoplayModel.LibraryId ).FirstOrDefault();
		}

		/// <summary>
		/// The Genre tag group
		/// </summary>
		private static TagGroup genreTags = null;

		private const int GenerationSize = 50;
	}
}