using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The StorageController class is responsible for coordinating the reading from storage collections of data that are not library specific
	/// and are not therefore re-read whenever the library changes. Other controllers can make use of library specific subsets of this data
	/// </summary>
	static class StorageController
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

			DataAvailable = true;
			new StorageDataAvailableMessage().Send();
		}

		/// <summary>
		/// Once the Artists have been read in their associated ArtistAlbums can be read as well and linked to them
		/// The ArtistAlbums are required for filtering so they may as well be linked in at the same time
		/// Get the Album associated with the ArtistAlbum as well so that only a single copy of the Albums is used
		/// </summary>
		private static async Task PopulateArtistsAsync()
		{
			// Do the linking of ArtistAlbum entries off the UI thread
			await Task.Run( () =>
			{
				// Link the Albums from the AlbumModel to the ArtistAlbums and link the ArtistAlbums to their associated Artists. 
				foreach ( ArtistAlbum artAlbum in ArtistAlbums.ArtistAlbumCollection )
				{
					// If this ArtistAlbum is associated with an Album (it should be) then link it to the Artist
					Album associatedAlbum = Albums.GetAlbumById( artAlbum.AlbumId );
					if ( associatedAlbum != null )
					{
						// Store the Album in the ArtistAlbum
						artAlbum.Album = associatedAlbum;

						// Save a reference to the Artist in the ArtistAlbum
						artAlbum.Artist = Artists.GetArtistById( artAlbum.ArtistId );

						// Add this ArtistAlbum to its Artist
						artAlbum.Artist.ArtistAlbums.Add( artAlbum );
					}
				}
			} );
		}

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