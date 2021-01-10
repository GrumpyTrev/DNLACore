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
		public static void RegisterInterestInDataAvailable( Action<Object> callback )
		{
			// If the data is available then call the callback
			if ( DataAvailable == true )
			{
				callback( null );
			}
			else
			{
				// Data is not currently available. Register the callback with the StorageDataAvailableMessage
				Mediator.RegisterPermanent( callback, typeof( StorageDataAvailableMessage ) );

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
			await PlaybackDetails.GetDataAsync();
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
			await Task.Run( async () =>
			{
				// Keep track of which ArtistAlbum entries are pointing to missing Artists
				List<ArtistAlbum> missingArtistRefs = new List<ArtistAlbum>();

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

						// Need to fix up some missing artists.
						if ( artAlbum.Artist == null )
						{
							Logger.Log( string.Format( "Cannot find Artist for ArtistAlbum id {0} name {1} using artist id {2}", artAlbum.Id, artAlbum.Name, artAlbum.ArtistId ) );
							missingArtistRefs.Add( artAlbum );
						}
						else
						{
							// Add this ArtistAlbum to its Artist
							artAlbum.Artist.ArtistAlbums.Add( artAlbum );
						}
					}
					else
					{
						Logger.Log( string.Format( "Cannot find album for ArtistAlbum id {0} name {1} using album id {2}", artAlbum.Id, artAlbum.Name, artAlbum.AlbumId ) );
					}
				}

				// Should be able to remove this after a few more sucessful library scans
				if ( missingArtistRefs.Count > 0 )
				{
					foreach ( ArtistAlbum artAlbum in missingArtistRefs )
					{
						// Find the Artist by name and library rather than id
						Artist missingArtist = Artists.ArtistCollection.Where( art => ( art.Name == artAlbum.Album.ArtistName ) && 
							( art.LibraryId == artAlbum.Album.LibraryId ) ).SingleOrDefault();

						if ( missingArtist == null )
						{
							// No such artist. Create one. Wait for this because we need to access the Id of the artist
							missingArtist = new Artist() { LibraryId = artAlbum.Album.LibraryId, Name = artAlbum.Album.ArtistName };
							await Artists.AddArtistAsync( missingArtist );
						}

						// Add this ArtistAlbum to the Artist
						missingArtist.ArtistAlbums.Add( artAlbum );
						artAlbum.ArtistId = missingArtist.Id;
						ArtistAlbumAccess.UpdateArtistAlbumAsync( artAlbum );
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