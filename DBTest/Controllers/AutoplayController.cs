﻿using System;
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
		static AutoplayController()
		{
			Mediator.RegisterPermanent( SongSelectedAsync, typeof( SongSelectedMessage ) );
			instance = new AutoplayController();
		}

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
		public static async void StartAutoplayAsync( IEnumerable<Song> selectedSongs, IEnumerable<string> genres, bool playNow )
		{
			// Clear any existing Genre/Album populations from the Autoplay record
			AutoplayModel.CurrentAutoplay.Clear();
			populationNumber = -1;

			// Save the starting set of Genres with the Autoplay record and add the first population
			AutoplayModel.CurrentAutoplay.SaveSeedGenres( genres );

			// Determine the first population from the genres according to the Spread setting
			switch ( AutoplayModel.CurrentAutoplay.Spread )
			{
				// For Slow and NoSpread the first population is the starting set of genres
				case Autoplay.SpreadType.NoSpread:
				case Autoplay.SpreadType.Slow:
					AutoplayModel.CurrentAutoplay.AddToPopulation( -1, genres );
					break;

				// For a fast spread determine all the reachable Genres and use that as the first population
				case Autoplay.SpreadType.Fast:
					AutoplayModel.CurrentAutoplay.AddAllReachableGenres( genres );
					break;
			}

			// Start generating songs
			List<Song> songs = new List<Song>( selectedSongs );
			await GenerateSongsAsync( songs );

			// Add these songs to the NowPlaying list either replacing or just adding them to the list
			BaseController.AddSongsToNowPlayingList( songs, playNow );

			// Set Autoplay active 
			PlaybackModeController.AutoOn = true;
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
			Random generator = new Random();

			// For each generation select a song from the available populations
			while ( songs.Count() < GenerationSize )
			{
				// First select a population. This can either be randomly selected from all the available populations, or just the next or previous populations
				if ( AutoplayModel.CurrentAutoplay.Target == Autoplay.TargetType.AllPopulations )
				{
					// Take the WeightType into account when choosing the next population
					switch ( AutoplayModel.CurrentAutoplay.Weight )
					{
						case Autoplay.WeightType.None:
						{
							// No weighting so choose randomly
							populationNumber = generator.Next( 0, AutoplayModel.CurrentAutoplay.Populations.Count );
							break;
						}
						case Autoplay.WeightType.Centre:
						case Autoplay.WeightType.Edge:
						{
							// Skew the selection towards the either the lowest or highest numbered populations
							// Increase the selection range by allocating more numbers to the lowest populations
							// If there are 'n' populations from 0 - (n-1) then the range will be n + (n - 1 ) + ( n - 2 ) + ... + 1 i.e. n(n + 1) / 2
							// Once a selection has been made it must be mapped to a population.
							// Do this by repeatedly removing n, n - 1, n - 2, etc from the selection until the correct population is reached
							int skewedSelection = 
								generator.Next( 0, ( AutoplayModel.CurrentAutoplay.Populations.Count * ( AutoplayModel.CurrentAutoplay.Populations.Count + 1 ) ) / 2 );

							// Start mapping the selection to a population either from the start or the end
							populationNumber = ( AutoplayModel.CurrentAutoplay.Weight == Autoplay.WeightType.Centre ) ? 0 
								: AutoplayModel.CurrentAutoplay.Populations.Count - 1;

							// Map the selection by removing a reducing slot size from the selection count until 0 is reached 
							int slotSize = AutoplayModel.CurrentAutoplay.Populations.Count;

							while ( ( skewedSelection -= slotSize-- ) >= 0 )
							{
								populationNumber += ( AutoplayModel.CurrentAutoplay.Weight == Autoplay.WeightType.Centre ) ? 1 : -1;
							}

							break;
						}
					}

				}
				else
				{
					// If this is the first time through this loop then use the first population
					if ( populationNumber == -1 )
					{
						populationNumber = 0;
					}
					else
					{
						// Determine the range of populations to choose from
						int firstPopulation = ( populationNumber == 0 ) ? 0 : populationNumber - 1;
						int lastPopulation = ( populationNumber < ( AutoplayModel.CurrentAutoplay.Populations.Count - 1 ) ) ? populationNumber + 1 : populationNumber;
						populationNumber = generator.Next( firstPopulation, lastPopulation + 1 );
					}
				}

				// Select the population
				Population selectedPopulation = AutoplayModel.CurrentAutoplay.Populations[ populationNumber ];

				// Now choose an Album from that population
				Album album = selectedPopulation.Albums[ generator.Next( 0, selectedPopulation.Albums.Count ) ];

				// Make sure that all the songs are available for the album
				await album.GetSongsAsync();

				// Now add a song
				songs.Add( album.Songs[ generator.Next( 0, album.Songs.Count ) ] );

				// If a SlowSpread is specified then add any new genres associated with the choosen album to the next Autoplay population.
				if ( AutoplayModel.CurrentAutoplay.Spread == Autoplay.SpreadType.Slow )
				{
					AutoplayModel.CurrentAutoplay.AddToPopulation( populationNumber, album.Genre.Split( ';' ) );
				}
			}
		}

		/// <summary>
		/// Called when the SongSelectedMessage is received
		/// We need to work out here if another set of entries should be added to the playlist. 
		/// If the song index is within "RefillLevel" of the end of the Now Playing list then add another "GenerationSize" set of songs.
		/// All the songs prior to the song index are removed except for the last "LeaveSongs"
		/// Only proceed with any of this processing if autoplay is active
		/// </summary>
		/// <param name="message"></param>
		private static async void SongSelectedAsync( object _message )
		{
			if ( PlaybackModeModel.AutoOn == true )
			{
				int currentSongIndex = Playback.SongIndex;
				Playlist nowPlaying = Playlists.GetNowPlayingPlaylist( AutoplayModel.LibraryId );

				Logger.Log( $"AutoplayController.SongSelectedAsync setting currentSongIndex to {currentSongIndex}" );

				if ( ( nowPlaying.PlaylistItems.Count - currentSongIndex ) < RefillLevel )
				{
					// Generate some songs and add to the Now Playing list
					List<Song> songs = new List<Song>();
					await GenerateSongsAsync( songs );

					// Add these songs to the NowPlaying list
					BaseController.AddSongsToNowPlayingList( songs, false );

					// Remove 'played' songs
					int songsToRemove = Math.Max(0, currentSongIndex - LeaveSongs );
					if ( songsToRemove > 0 )
					{
						Logger.Log( $"AutoplayController.SongSelectedAsync removing {songsToRemove} songs" );
						NowPlayingController.DeleteNowPlayingItems( nowPlaying.PlaylistItems.GetRange( 0, songsToRemove ) );
					}
				}
			}
		}
		
		/// <summary>
		/// The number of songs generated in one go
		/// </summary>
		private const int GenerationSize = 50;

		/// <summary>
		/// The number of songs left to play at which point the next set is generated
		/// </summary>
		private const int RefillLevel = 10;

		/// <summary>
		/// The number of songs to leave above the current song when deleting songs
		/// </summary>
		private const int LeaveSongs = 10;

		// The last population index used to select from
		private static int populationNumber = -1;

		/// <summary>
		/// The one and only AutoplayController instance
		/// </summary>
		private static readonly AutoplayController instance = null;
	}
}