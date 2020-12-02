using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The AutoplayController class is the controller for the AutoplayManagement
	/// </summary>
	class AutoplayController : BaseController
	{
		/// <summary>
		/// Create the one and only instance of the controller
		/// </summary>
		static AutoplayController() => instance = new AutoplayController();

		/// <summary>
		/// Get the Controller data
		/// </summary>
		public static void GetControllerData() => instance.GetData();

		/// <summary>
		/// Start filling the NowPlaying list with the first set of songs
		/// Save the set of genres as genre population 0 and find all the albums associated with those genres
		/// If a song has been specified then add that to the NowPlaying list first
		/// </summary>
		/// <param name="selectedSong"></param>
		/// <param name="genres"></param>
		public static async void StartAutoplayAsync( Song selectedSong, IEnumerable<string> genres )
		{
			// Clear any existing Genre/Album populations from the Autoplay record
			AutoplayModel.CurrentAutoplay.Clear();

			// Save the starting set of Genres with the Autoplay record and add the first population
			AutoplayModel.CurrentAutoplay.SaveSeedGenres( genres );
			AutoplayModel.CurrentAutoplay.AddToPopulation( -1, genres );

			// Start generating songs
			List<Song> songs = new List<Song>();
			if ( selectedSong != null )
			{
				songs.Add( selectedSong );
			}

			await GenerateSongsAsync( songs );

			BaseController.AddSongsToNowPlayingList( songs, true );
		}

		/// <summary>
		/// Called when the Autoplay data has been read in from storage
		/// Get the Autoplay record for the current library
		/// </summary>
		/// <param name="message"></param>
		protected override async void StorageDataAvailable( object _ = null )
		{
			// Link the Autoplay records with their Populations
			await Autoplays.LinkPopulationsAsync();

			// Save the libray being used locally to detect changes
			AutoplayModel.LibraryId = ConnectionDetailsModel.LibraryId;

			// Get the library's Autoplay
			AutoplayModel.CurrentAutoplay = await Autoplays.GetAutoplayAsync( AutoplayModel.LibraryId );

			base.StorageDataAvailable();
		}

		/// <summary>
		/// Generate a set of songs according to the current Autoplay configuration
		/// </summary>
		/// <param name="songs"></param>
		/// <returns></returns>
		private static async Task GenerateSongsAsync( List<Song> songs )
		{
			// For each generation select a song from the available populations
			Random generator = new Random();
			while ( songs.Count() < GenerationSize )
			{
				// First select a population
				// For now randomly from available populations
				int populationNumber = generator.Next( 0, AutoplayModel.CurrentAutoplay.Populations.Count );
				Population population = AutoplayModel.CurrentAutoplay.Populations[ populationNumber ];

				// Now choose an Album from that population
				Album album = population.Albums[ generator.Next( 0, population.Albums.Count ) ];

				// Make sure that all the songs are available for the album
				await album.GetSongsAsync();

				// Now add a song
				songs.Add( album.Songs[ generator.Next( 0, album.Songs.Count ) ] );

				// Add any new genres associated with the choosen album to the next Autoplay population
				AutoplayModel.CurrentAutoplay.AddToPopulation( populationNumber, album.Genre.Split( ';' ) );
			}
		}

		private const int GenerationSize = 50;

		/// <summary>
		/// The one and only AutoplayController instance
		/// </summary>
		private static readonly AutoplayController instance = null;
	}
}