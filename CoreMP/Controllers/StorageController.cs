using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The StorageController class is responsible for coordinating the reading from storage collections of data that are not library specific
	/// and are not therefore re-read whenever the library changes. Other controllers can make use of library specific subsets of this data
	/// </summary>
	internal static class StorageController
	{
		/// <summary>
		/// Called to register interest in the availability of the managed storage collections
		/// </summary>
		/// <param name="callback"></param>
		public static void RegisterInterestInDataAvailable( Action callback )
		{
			// If the data is available then call the callback
			if ( DataAvailable == true )
			{
				callback();
			}
			else
			{
				// Data is not currently available. Register the callback with the StorageDataAvailableMessage
				StorageDataAvailableMessage.Register( callback );

				// If the data is not being read then start the read process
				if ( DataBeingRead == false )
				{
					DataBeingRead = true;
					ReadManagedCollections();
				}
			}
		}

		/// <summary>
		/// Read all the managed collections and then tell any registered listeners
		/// </summary>
		private static async void ReadManagedCollections()
		{
			await Songs.GetDataAsync();
			await Albums.GetDataAsync();
			await Sources.GetDataAsync();
			await Artists.GetDataAsync();
			await ArtistAlbums.GetDataAsync();
            await Libraries.GetDataAsync();
			await Playback.GetDataAsync();
			await Playlists.GetDataAsync();
			await Autoplays.GetDataAsync();
			await Tags.GetDataAsync();
			await TaggedAlbums.GetDataAsync();
			await GenrePopulations.GetDataAsync();

			// Carry out some one-off data linking
			await PopulateArtistsAsync();
			await FilterManagementController.FormGenreTagsAsync();

			// Carry out model integrity checking
			await CheckSongArtistAlbumIds();
			await CheckEmptyArtists();
			await CheckAlbumsWithNoArtists();

			DataAvailable = true;
			new StorageDataAvailableMessage().Send();
        }

		/// <summary>
		/// Once the Artists have been read in their associated ArtistAlbums can be read as well and linked to them
		/// The ArtistAlbums are required for filtering so they may as well be linked in at the same time
		/// Get the Album associated with the ArtistAlbum as well so that only a single copy of the Albums is used
		/// Do the linking of ArtistAlbum entries off the UI thread
		/// </summary>
		private static async Task PopulateArtistsAsync() => await Task.Run( () =>
		{
			// Keep tabs on any ArtistAlbum entries that have no Album or Artists associated with them.
			List<ArtistAlbum> orphanArtistAlbums = new List<ArtistAlbum>();

			// Link the Albums from the AlbumModel to the ArtistAlbums and link the ArtistAlbums to their associated Artists. 
			foreach ( ArtistAlbum artAlbum in ArtistAlbums.ArtistAlbumCollection )
			{
				// If this ArtistAlbum is associated with an Album (it should be) then link it to the Artist
				// Store the Album in the ArtistAlbum
				artAlbum.Album = Albums.GetAlbumById( artAlbum.AlbumId );

				if ( artAlbum.Album != null )
				{  
					// Save a reference to the Artist in the ArtistAlbum
					artAlbum.Artist = Artists.GetArtistById( artAlbum.ArtistId );

					if ( artAlbum.Artist != null )
					{
						// Add this ArtistAlbum to its Artist
						artAlbum.Artist.ArtistAlbums.Add( artAlbum );
					}
					else
					{
						Logger.Log( string.Format( "Cannot find Artist id: {0} for ArtistAlbum {1} id: {2}", artAlbum.ArtistId, artAlbum.Name, artAlbum.Id ) );

						orphanArtistAlbums.Add( artAlbum );
					}
				}
				else
				{
					Logger.Log( string.Format( "Cannot find Album id: {0} for ArtistAlbum {1} id: {2}", artAlbum.AlbumId, artAlbum.Name, artAlbum.Id ) );

					orphanArtistAlbums.Add( artAlbum );
				}
			}

			ArtistAlbums.DeleteArtistAlbums( orphanArtistAlbums );
		} );

		/// <summary>
		/// Check that all the Songs in the model have valid ArtistAlbum instances associated with
		/// them
		/// </summary>
		/// <returns></returns>
		private static async Task CheckSongArtistAlbumIds() => await Task.Run( () =>
		{
			List<Song> orphanSongs = new
			List<Song>();

			foreach ( Song songToCheck in Songs.SongCollection )
			{
				ArtistAlbum checkAlbum = ArtistAlbums.GetArtistAlbumById( songToCheck.ArtistAlbumId );
				if ( checkAlbum == null )
				{
					Logger.Log( string.Format( "Cannot find ArtistAlbum id: {0} for Song {1} id: {2}", songToCheck.ArtistAlbumId, songToCheck.Title, songToCheck.Id ) );

					orphanSongs.Add( songToCheck );
				}
			}

			Songs.DeleteSongs( orphanSongs );
		} );

		/// <summary>
		/// Check that all the Artists in the model have ArtistAlbums linked to them
		/// </summary>
		/// <returns></returns>
		private static async Task CheckEmptyArtists() => await Task.Run( () =>
		{
			List<Artist> orphanArtists = new List<Artist>();

			foreach ( Artist artistToCheck in Artists.ArtistCollection )
			{
				if ( artistToCheck.ArtistAlbums.Count == 0 )
				{
					Logger.Log( string.Format( "No ArtistAlbums for Artist {0} id: {1}", artistToCheck.Name, artistToCheck.Id ) );

					orphanArtists.Add( artistToCheck );
				}
			}

			Artists.DeleteArtists( orphanArtists );
		} );

		private static async Task CheckAlbumsWithNoArtists() => await Task.Run( () =>
		{
			List<Album> orphanAlbums = new List<Album>();

			foreach ( Album albumToCheck in Albums.AlbumCollection )
			{
				if ( albumToCheck.ArtistName == null )
				{
					Logger.Log( string.Format( "No Artist Name for Album {0} id: {1}", albumToCheck.Name, albumToCheck.Id ) );

					orphanAlbums.Add( albumToCheck );
				}
			}

			Albums.DeleteAlbums( orphanAlbums );

		} );

		/// <summary>
		/// Is the managed storage available
		/// </summary>
		private static bool DataAvailable { get; set; } = false;

		/// <summary>
		/// If the managed storage currently being read
		/// </summary>
		private static bool DataBeingRead { get; set; } = false;
	}
}
