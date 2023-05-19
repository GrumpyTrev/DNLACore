using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The LibraryScanController carries out the asynchronous actions involved in scanning a library
	/// </summary>
	internal class LibraryScanController
	{
		/// <summary>
		/// Asynchronous method called to carry out a library scan
		/// </summary>
		/// <param name="libraryToScan"></param>
		public async void ScanLibraryAsynch( Library libraryToScan, Action scanFinished, Func<bool> scanCancelledCheck )
		{
			// Ignore this if there is already a scan in progress
			if ( scanInProgress == false )
			{
				// Prevent this from being executed twice
				scanInProgress = true;

				// Keep track of any existing songs that have not been matched in the scan
				List<Song> unmatchedSongs = new List<Song>();

				// Keep track of all the albums that have been scanned
				List<ScannedAlbum> newAlbums = new List<ScannedAlbum>();

				// Keep track of the songs for which the album name has changed
				List<Song> songsWithChangedAlbumNames = new List<Song>();

				// Form a lookup table for Artists in this library
				artistsInLibrary = Artists.ArtistCollection.Where( art => art.LibraryId == libraryToScan.Id ).ToDictionary( art => art.Name.ToUpper() );

				await Task.Run( async () =>
				{
					foreach ( Source source in libraryToScan.Sources )
					{
						// Get the songs as well as we're going to need them below
						source.GetSongs();

						// Add the songs from this source to a dictionary. For UPnP sources don't use the path stored with the song as that may have
						// changed. Instead use a combination of the Artist Name, Song Name, Album Name and track number
						Dictionary<string, Song> pathLookup = null;
						if ( source.AccessMethod != Source.AccessType.UPnP )
						{
							pathLookup = new Dictionary<string, Song>( source.Songs.ToDictionary( song => song.Path ) );
						}
						else
						{
							pathLookup = new Dictionary<string, Song>();

							foreach ( Song songToAdd in source.Songs )
							{
								string key = $"{songToAdd.Album.ArtistName}:{songToAdd.Title}:{songToAdd.Album.Name}:{songToAdd.Track}";

								if ( pathLookup.ContainsKey( key ) == false )
								{
									pathLookup[ key ] = songToAdd;
								}
								else
								{
									Logger.Log( string.Format( $"Duplicate song key {key}" ) );
								}
							}
						}

						// Reset the scan action for all Songs
						source.Songs.ForEach( song => song.ScanAction = Song.ScanActionType.NotMatched );

						// Use a SongStorage instance to check for song changes
						SongStorage scanStorage = new SongStorage( source, pathLookup );

						// Check the source scanning method
						if ( source.AccessMethod == Source.AccessType.FTP )
						{
							// Scan using the generic FTPScanner but with our callbacks
							await new FTPScanner( scanStorage, scanCancelledCheck ).Scan( source.ScanSource );
						}
						else if ( source.AccessMethod == Source.AccessType.Local )
						{
							// Scan using the generic InternalScanner but with our callbacks
							await new InternalScanner( scanStorage, scanCancelledCheck ).Scan( source.ScanSource );
						}
						else if ( source.AccessMethod == Source.AccessType.UPnP )
						{
							// Scan using the generic UPnPScanner but with our callbacks
							await new UPnPScanner( scanStorage, scanCancelledCheck ).Scan( source.ScanSource );
						}

						// Add any unmatched songs to a list that'll be processed when all sources have been scanned
						unmatchedSongs.AddRange( pathLookup.Values.Where( song => song.ScanAction == Song.ScanActionType.NotMatched ) );

						// Add the new scanned albums to the model
						newAlbums.AddRange( scanStorage.NewAlbums );

						// Add the songs with changed albums names to the model
						songsWithChangedAlbumNames.AddRange( scanStorage.SongsWithChangedAlbumNames );
					}

					// If the scan process has not been cancelled then store the scanned albums and delete any unmatched songs
					if ( scanCancelledCheck.Invoke() == false )
					{
						// Delete songs for which the album name has changed
						DeleteSongs( songsWithChangedAlbumNames );

						foreach ( ScannedAlbum album in newAlbums )
						{
							await StoreAlbumAsync( album, libraryToScan.Id );
						}

						// Delete unmatched songs
						DeleteSongs( unmatchedSongs );
					}
				} );

				// If there have been any changes to the library, and it is the library currently being displayed then force a refresh
				if ( ( ( songsWithChangedAlbumNames.Count > 0 ) || ( newAlbums.Count > 0 ) || ( unmatchedSongs.Count > 0 ) ) && 
					( libraryToScan.Id == ConnectionDetailsModel.LibraryId ) )
				{
					new SelectedLibraryChangedMessage() { SelectedLibrary = libraryToScan.Id }.Send();
				}

				scanInProgress = false;

				// Report the completion back through the delegate
				scanFinished.Invoke();
			}
		}

		/// <summary>
		/// Delete the list of songs from the library
		/// </summary>
		/// <param name="songsToDelete"></param>
		private void DeleteSongs( List<Song> songsToDelete )
		{
			// Keep track of any albums that are deleted so that other controllers can be notified
			List<int> deletedAlbumIds = new List<int>();

			// Delete all the Songs.
			Songs.DeleteSongs( songsToDelete );

			// Delete all the PlaylistItems associated with the songs.
			Playlists.DeletePlaylistItems( songsToDelete.Select( song => song.Id ).ToHashSet() );

			// Form a distinct list of all the ArtistAlbum items referenced by the deleted songs
			IEnumerable<int> artistAlbumIds = songsToDelete.Select( song => song.ArtistAlbumId ).Distinct();

			// Check if any of these ArtistAlbum items are now empty and need deleting
			foreach ( int id in artistAlbumIds )
			{
				int deletedAlbumId = CheckForAlbumDeletion( id );

				if ( deletedAlbumId != -1 )
				{
					deletedAlbumIds.Add( deletedAlbumId );
				}
			}

			if ( deletedAlbumIds.Count > 0 )
			{
				new AlbumsDeletedMessage() { DeletedAlbumIds = deletedAlbumIds }.Send();
			}
		}

		/// <summary>
		/// Called to process a group of songs belonging to the same album name ( but not necessarily the same artist )
		/// </summary>
		/// <param name="album"></param>
		private async Task StoreAlbumAsync( ScannedAlbum album, int libraryId )
		{
			Logger.Log( string.Format( "Album: {0} Single artist: {1}", album.Name, album.SingleArtist ) );

			// Get an existing or new Album entry for the songs
			Album songAlbum = await GetAlbumToHoldSongsAsync( album, libraryId );

			// Keep track of Artist and ArtistAlbum entries in case they can be reused for different artists
			ArtistAlbum songArtistAlbum = null;
			Artist songArtist = null;

			// Add all the songs in the album
			foreach ( ScannedSong songScanned in album.Songs )
			{
				// If there is no existing Artist entry or the current one if for the wrong Artist name then get or create a new one
				if ( ( songArtist == null ) || ( songArtist.Name != songScanned.ArtistName ) )
				{
					// As this is a new Artist the ArtistAlbum needs to be re-initialised.
					if ( songArtistAlbum != null )
					{
						songArtistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
					}

					songArtistAlbum = null;

					// Find the Artist for this song
					songArtist = await GetArtistToHoldSongsAsync( songScanned.ArtistName, libraryId );
				}

				// If there is an existing ArtistAlbum then use it as it will be for the correct Artist, i.e. not cleared above
				if ( songArtistAlbum == null )
				{
					// Find an existing or create a new ArtistAlbum entry
					songArtistAlbum = await GetArtistAlbumToHoldSongsAsync( songArtist, songAlbum );
				}

				// Add the song to the database, the album and the album artist
				Song songToAdd = new Song()
				{
					Title = songScanned.Tags.Title,
					Track = songScanned.Track,
					Path = songScanned.SourcePath,
					ModifiedTime = songScanned.Modified,
					Length = songScanned.Length,
					AlbumId = songAlbum.Id,
					ArtistAlbumId = songArtistAlbum.Id,
					SourceId = album.ScanSource.Id
				};

				// No need to wait for this
				await Songs.AddSongAsync( songToAdd );

				Logger.Log( string.Format(
					"Song added with Artist: {0} Title: {1} Track: {2} Modified: {3} Length {4} Year {5} Album Id: {6} ArtistAlbum Id: {7}",
					songScanned.Tags.Artist, songScanned.Tags.Title, songScanned.Tags.Track, songScanned.Modified, songScanned.Length, songScanned.Year,
					songToAdd.AlbumId, songToAdd.ArtistAlbumId ) );

				// Add to the Album
				songAlbum.Songs.Add( songToAdd );

				// Keep track whether or not to update the album 
				bool updateAlbum = false;

				// Store the artist name with the album
				if ( ( songAlbum.ArtistName == null ) || ( songAlbum.ArtistName.Length == 0 ) )
				{
					songAlbum.ArtistName = songArtist.Name;
					updateAlbum = true;
				}
				else
				{
					// The artist has already been stored - check if it is the same artist
					if ( songAlbum.ArtistName != songArtist.Name )
					{
						songAlbum.ArtistName = SongStorage.VariousArtistsString;
						updateAlbum = true;
					}
				}

				// Update the album year if not already set and this song has a year set
				if ( ( songAlbum.Year != songScanned.Year ) && ( songAlbum.Year == 0 ) )
				{
					songAlbum.Year = songScanned.Year;
					updateAlbum = true;
				}

				// Update the album genre.
				// Only try updating if a genre is defined for the song
				// If the album does not have a genre then get one for the song and store it in the album
				if ( ( songScanned.Tags.Genre.Length > 0 ) && ( songAlbum.Genre.Length == 0 ) )
				{
					songAlbum.Genre = songScanned.Tags.Genre;
					updateAlbum = true;
				}

				if ( updateAlbum == true )
				{
					await DbAccess.UpdateAsync( songAlbum );
				}

				// Add to the source
				album.ScanSource.Songs.Add( songToAdd );

				// Add to the ArtistAlbum
				songArtistAlbum.Songs.Add( songToAdd );
			}

			if ( songArtistAlbum != null )
			{
				songArtistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}
		}

		/// <summary>
		/// Either find an existing album or create a new album to hold the songs.
		/// If all songs are from the same artist then check is there is an existing album of the same name associated with that artist.
		/// If the songs are from different artists then check in the "Various Artists" artist.
		/// These artists may not exist at this stage
		/// </summary>
		/// <returns></returns>
		private async Task<Album> GetAlbumToHoldSongsAsync( ScannedAlbum album, int libraryId )
		{
			Album songAlbum = null;

			string artistName = ( album.SingleArtist == true ) ? album.Songs[ 0 ].ArtistName : SongStorage.VariousArtistsString;

			// Check if the artist already exists in the library. 
			Artist songArtist = artistsInLibrary.GetValueOrDefault( artistName.ToUpper() );

			// If the artist exists then check for existing album. The artist will hold ArtistAlbum entries rather than Album entries, but the ArtistAlbum entries
			// have the same name as the albums. Cannot just use the album name as that may not be unique.
			if ( songArtist != null )
			{
				ArtistAlbum songArtistAlbum = songArtist.ArtistAlbums.SingleOrDefault( p => ( p.Name.ToUpper() == album.Songs[ 0 ].Tags.Album.ToUpper() ) );
				if ( songArtistAlbum != null )
				{
					Logger.Log( string.Format( "Artist {0} and ArtistAlbum {1} both found", artistName, songArtistAlbum.Name ) );

					songAlbum = songArtistAlbum.Album;
				}
				else
				{
					Logger.Log( string.Format( "Artist {0} found ArtistAlbum {1} not found", artistName, album.Songs[ 0 ].Tags.Album ) );
				}
			}
			else
			{
				Logger.Log( string.Format( "Artist {0} for album {1} not found", artistName, album.Name ) );
			}

			// If no existing album create a new one
			if ( songAlbum == null )
			{
				songAlbum = new Album() { Name = album.Name, LibraryId = libraryId };
				await Albums.AddAlbumAsync( songAlbum );

				Logger.Log( string.Format( "Create album {0} Id: {1}", songAlbum.Name, songAlbum.Id ) );
			}

			return songAlbum;
		}

		/// <summary>
		/// Get an existing Artist or create a new one.
		/// </summary>
		/// <param name="artistName"></param>
		/// <returns></returns>
		private async Task<Artist> GetArtistToHoldSongsAsync( string artistName, int libraryId )
		{
			// Find the Artist for this song. The 'scanLibrary' can be used for this search as it is already fully populated with the 
			// artists
			Artist songArtist = artistsInLibrary.GetValueOrDefault( artistName.ToUpper() );

			if ( songArtist == null )
			{
				// Create a new Artist and add it to the database
				songArtist = new Artist() { Name = artistName, ArtistAlbums = new List<ArtistAlbum>(), LibraryId = libraryId };
				await Artists.AddArtistAsync( songArtist );

				Logger.Log( string.Format( "Artist: {0} not found. Created with Id: {1}", artistName, songArtist.Id ) );

				// Add it to the collection for this library only
				artistsInLibrary[ songArtist.Name.ToUpper() ] = songArtist;
			}
			else
			{
				Logger.Log( string.Format( "Artist: {0} found with Id: {1}", songArtist.Name, songArtist.Id ) );
			}

			return songArtist;
		}

		/// <summary>
		/// Get an existing or new ArtistAlbum entry to hold the songs associated with a particular Artist
		/// </summary>
		/// <param name="songArtist"></param>
		/// <param name="songAlbum"></param>
		/// <returns></returns>
		private async Task<ArtistAlbum> GetArtistAlbumToHoldSongsAsync( Artist songArtist, Album songAlbum )
		{
			ArtistAlbum songArtistAlbum = null;

			// Find an existing or create a new ArtistAlbum entry
			songArtistAlbum = songArtist.ArtistAlbums.SingleOrDefault( p => ( p.Name.ToUpper() == songAlbum.Name.ToUpper() ) );

			Logger.Log( string.Format( "ArtistAlbum: {0} {1}", songAlbum.Name, ( songArtistAlbum != null ) ? "found" : "not found creating in db" ) );

			if ( songArtistAlbum == null )
			{
				// Create a new ArtistAlbum and add it to the database and the Artist
				songArtistAlbum = new ArtistAlbum()
				{
					Name = songAlbum.Name,
					Album = songAlbum,
					Songs = new List<Song>(),
					ArtistId = songArtist.Id,
					Artist = songArtist,
					AlbumId = songAlbum.Id
				};
				await ArtistAlbums.AddArtistAlbumAsync( songArtistAlbum );

				Logger.Log( string.Format( "ArtistAlbum: {0} created with Id: {1}", songArtistAlbum.Name, songArtistAlbum.Id ) );

				songArtist.ArtistAlbums.Add( songArtistAlbum );
			}
			else
			{
				Logger.Log( string.Format( "ArtistAlbum: {0} found with Id: {1}", songArtistAlbum.Name, songArtistAlbum.Id ) );

				// Get the children of the existing ArtistAlbum
				if ( songArtistAlbum.Songs == null )
				{
					songArtistAlbum.Songs = Songs.GetArtistAlbumSongs( songArtistAlbum.Id );
				}
			}

			return songArtistAlbum;
		}

		/// <summary>
		/// Check if the specified ArtistAlbum should be deleted and any associated Album or Artist
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		private int CheckForAlbumDeletion( int artistAlbumId )
		{
			// Keep track in the Tuple of any albums or artists deleted
			int deletedAlbumId = -1;

			// Refresh the contents of the ArtistAlbum
			ArtistAlbum artistAlbum = ArtistAlbums.GetArtistAlbumById( artistAlbumId );
			artistAlbum.Songs = Songs.GetArtistAlbumSongs( artistAlbum.Id );

			// Check if this ArtistAlbum is being referenced by any songs
			if ( artistAlbum.Songs.Count == 0 )
			{
				// Delete the ArtistAlbum as it is no longer being referenced
				ArtistAlbums.DeleteArtistAlbum( artistAlbum );

				// Remove this ArtistAlbum from the Artist
				Artist artist = Artists.GetArtistById( artistAlbum.ArtistId );
				artist.ArtistAlbums.Remove( artistAlbum );

				// Does the associated Artist have any other Albums
				if ( artist.ArtistAlbums.Count == 0 )
				{
					// Delete the Artist
					Artists.DeleteArtist( artist );
				}

				// Does any other ArtistAlbum reference the Album
				if ( ArtistAlbums.ArtistAlbumCollection.Any( art => art.AlbumId == artistAlbum.AlbumId ) == false )
				{
					// Not referenced by any ArtistAlbum. so delete it
					Albums.DeleteAlbum( artistAlbum.Album );
					deletedAlbumId = artistAlbum.AlbumId;
				}
			}

			return deletedAlbumId;
		}

		/// <summary>
		/// Flag indicating whether or not the this controller is busy scanning a library
		/// </summary>
		private bool scanInProgress = false;

		/// <summary>
		/// The Artists in the Library being scanned
		/// </summary>
		private Dictionary<string, Artist> artistsInLibrary = null;
	}
}
