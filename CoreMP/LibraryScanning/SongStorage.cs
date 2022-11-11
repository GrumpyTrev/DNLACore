using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The SongStorage class is responsible for storing scanned songs and associated albums in the database
	/// </summary>
	internal class SongStorage
	{
		/// <summary>
		/// Constructor specifying the library and source
		/// </summary>
		/// <param name="libraryToScan"></param>
		/// <param name="sourceToScan"></param>
		public SongStorage( int libraryToScan, Source sourceToScan, Dictionary<string, Song> pathLookup )
		{
			scanLibrary = libraryToScan;
			sourceBeingScanned = sourceToScan;
			songLookup = pathLookup;
			LibraryModified = false;

			// Form a lookup table for Artists in this library
			artistsInLibrary = Artists.ArtistCollection.Where( art => art.LibraryId == scanLibrary ).ToDictionary( art => art.Name.ToUpper() );
		}

		/// <summary>
		/// Called when a group of songs from the same folder have been scanned
		/// Group the songs into albums by album name and then process each of these albums
		/// </summary>
		/// <param name="songs"></param>
		public async Task SongsScanned( List<ScannedSong> songs )
		{
			Dictionary<string, ScannedAlbum> albumGroups = new Dictionary<string, ScannedAlbum>();

			// Group the list of songs according to the associated album, and determine if all the songs are by the same artist
			foreach ( ScannedSong song in songs )
			{
				// Replace empty tags etc.
				song.NormaliseTags();

				// Is there a group for this album
				if ( albumGroups.TryGetValue( song.Tags.Album, out ScannedAlbum album ) == false )
				{
					album = new ScannedAlbum() { Name = song.Tags.Album };
					albumGroups[ album.Name ] = album;
				}

				if ( await DoesSongRequireAdding( song ) == true )
				{
					// Add the song to the album
					album.Songs.Add( song );
				}

				// If this is not the first song in the group then check if the artist is the same
				if ( album.Songs.Count > 1 )
				{
					if ( ( album.SingleArtist == true ) && ( album.Songs[ 0 ].ArtistName.ToUpper() != song.ArtistName.ToUpper() ) )
					{
						album.SingleArtist = false;
					}
				}
			}

			// Store the scanned song data
			foreach ( ScannedAlbum album in albumGroups.Values )
			{
				if ( album.Songs.Count > 0 )
				{
					await StoreAlbumAsync( album );
				}
			}
		}

		/// <summary>
		/// Was the librray modified during the scan
		/// </summary>
		public bool LibraryModified { get; set; }

		/// <summary>
		/// Called when the filepath and modified time for a song have been determined
		/// If it's a existing song with the same modified time then the song does not require any further scanning
		/// Otherwise let scanning continue
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="modifiedTime"></param>
		/// <returns></returns>
		public bool DoesSongRequireScanning( string filepath, DateTime modifiedTime )
		{
			bool scanRequired = true;

			// Lookup the path of this song in the dictionary
			if ( songLookup.TryGetValue( filepath, out Song matchedSong ) == true )
			{
				// Time from the FTP server always seem to be an hour out and is supposed to be in UTC
				// Compare both times as is and then take 1 hour off the stored time and then add 1 hour on
				if ( ( matchedSong.ModifiedTime == modifiedTime ) || ( matchedSong.ModifiedTime.AddHours( -1 ) == modifiedTime ) ||
					( matchedSong.ModifiedTime.AddHours( 1 ) == modifiedTime ) )
				{
					scanRequired = false;
					matchedSong.ScanAction = Song.ScanActionType.Matched;
				}
				else
				{
					// Song was found but has changed in some way
					matchedSong.ScanAction = Song.ScanActionType.Differ;
				}
			}

			return scanRequired;
		}

		/// <summary>
		/// Called to determine whether a song that has been scanned requires adding to the library, or just an existing entry updated
		/// </summary>
		/// <param name="song"></param>
		/// <returns></returns>
		private async Task<bool> DoesSongRequireAdding( ScannedSong song )
		{
			bool needsAdding = true;

			// Lookup the path of this song in the dictionary
			if ( ( songLookup.TryGetValue( song.SourcePath, out Song matchedSong ) == true ) && ( matchedSong.ScanAction == Song.ScanActionType.Differ ) )
			{
				// The library is about to be changed in some way so set the modified flag
				LibraryModified = true;

				// Need to check whether the matched Artist or Album names have changed. If they have then treat this as a new song and mark
				// the matched song for deletion
				ArtistAlbum matchedArtistAlbum = ArtistAlbums.GetArtistAlbumById( matchedSong.ArtistAlbumId );
				Artist matchedArtist = Artists.GetArtistById( matchedArtistAlbum.ArtistId );

				// If the artist or album name has changed then treat this as a new song. Otherwise update the existing song in the library
				if ( ( matchedArtist.Name.ToUpper() != song.ArtistName.ToUpper() ) || ( matchedArtistAlbum.Name.ToUpper() != song.Tags.Album.ToUpper() ) )
				{
					// The existing song needs to be deleted now.
					// If the song is not deleted then there will be more than one associated with the same storage location.
					// This is much less complicated than trying to move the existing song to a new Artist or Album
					// If an Artist has been deleted due to this song deletion then remove it from the dictionary being used here
					Artist deletedArtist = LibraryScanController.DeleteSong( matchedSong );
					if ( deletedArtist != null )
					{
						artistsInLibrary.Remove( deletedArtist.Name.ToUpper() );
					}

					matchedSong.ScanAction = Song.ScanActionType.Matched;
				}
				else
				{
					// No need to add a new song, update the existing one
					needsAdding = false;

					matchedSong.Length = song.Length;
					matchedSong.ModifiedTime = song.Modified;
					matchedSong.Title = song.Tags.Title;
					matchedSong.Track = song.Track;
					await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedSong );

					// Check if the year or genre fields on the album needs updating
					// Don't update the Album if it is a 'various artists' album as the these fields is not applicable
					Album matchedAlbum = Albums.GetAlbumById( matchedArtistAlbum.AlbumId );

					if ( matchedAlbum.ArtistName != SongStorage.VariousArtistsString )
					{
						// Update the stored year if it is different to the artist year and the artist year is defined.
						// Log when a valid year is overwritten by different year
						if ( matchedAlbum.Year != song.Year )
						{
							if ( song.Year != 0 )
							{
								matchedAlbum.Year = song.Year;
								await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedAlbum );
							}
						}

						if ( song.Tags.Genre.Length > 0 )
						{
							// If this album does not already have a genre, or the genre has been changed then set it now
							if ( ( matchedAlbum.Genre == null ) || ( matchedAlbum.Genre != song.Tags.Genre ) )
							{
								matchedAlbum.Genre = song.Tags.Genre;
								await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedAlbum );
							}
						}
					}
				}
			}

			return needsAdding;
		}

		/// <summary>
		/// Called to process a group of songs belonging to the same album name ( but not necessarily the same artist )
		/// </summary>
		/// <param name="album"></param>
		private async Task StoreAlbumAsync( ScannedAlbum album )
		{
			// Set the modified flag
			LibraryModified = true;

			Logger.Log( string.Format( "Album: {0} Single artist: {1}", album.Name, album.SingleArtist ) );

			// Get an existing or new Album entry for the songs
			Album songAlbum = await GetAlbumToHoldSongsAsync( album );

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
					songArtist = await GetArtistToHoldSongsAsync( songScanned.ArtistName );
				}

				// If there is an existing ArtistAlbum then use it as it will be for the correct Artist, i.e. not cleared above
				if ( songArtistAlbum == null )
				{
					// Find an existing or create a new ArtistAlbum entry
					songArtistAlbum = await GetArtistAlbumToHoldSongsAsync( songArtist, songAlbum );
				}

				// Add the song to the database, the album and the album artist
				Song songToAdd = new Song() {
					Title = songScanned.Tags.Title, Track = songScanned.Track, Path = songScanned.SourcePath,
					ModifiedTime = songScanned.Modified, Length = songScanned.Length, AlbumId = songAlbum.Id, ArtistAlbumId = songArtistAlbum.Id,
					SourceId = sourceBeingScanned.Id
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
						songAlbum.ArtistName = VariousArtistsString;
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
				sourceBeingScanned.Songs.Add( songToAdd );

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
		private async Task<Album> GetAlbumToHoldSongsAsync( ScannedAlbum album )
		{
			Album songAlbum = null;

			string artistName = ( album.SingleArtist == true ) ? album.Songs[ 0 ].ArtistName : VariousArtistsString;

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

					// The rest of the code expects the Album to have its songs populated, so check here
					songAlbum.GetSongs();
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
				songAlbum = new Album() { Name = album.Name, Songs = new List<Song>(), LibraryId = scanLibrary };
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
		private async Task<Artist> GetArtistToHoldSongsAsync( string artistName )
		{
			// Find the Artist for this song. The 'scanLibrary' can be used for this search as it is already fully populated with the 
			// artists
			Artist songArtist = artistsInLibrary.GetValueOrDefault( artistName.ToUpper() );

			if ( songArtist == null )
			{
				// Create a new Artist and add it to the database
				songArtist = new Artist() { Name = artistName, ArtistAlbums = new List<ArtistAlbum>(), LibraryId = scanLibrary };
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
				songArtistAlbum = new ArtistAlbum() { Name = songAlbum.Name, Album = songAlbum, Songs = new List<Song>(), ArtistId = songArtist.Id,
					 Artist = songArtist, AlbumId = songAlbum.Id };
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
		/// The name given to the artist in albums when they contain songs from multiple artists
		/// </summary>
		public const string VariousArtistsString = "Various Artists";

		/// <summary>
		/// The Source to insert Songs into
		/// </summary>
		private readonly Source sourceBeingScanned = null;

		/// <summary>
		/// The Library to insert Artists and Albums into
		/// </summary>
		private readonly int scanLibrary = 0;

		/// <summary>
		/// The Artists in the Library being scanned
		/// </summary>
		private readonly Dictionary< string, Artist > artistsInLibrary = null;

		/// <summary>
		/// Collection used to lookup esxisting songs
		/// </summary>
		private readonly Dictionary<string, Song> songLookup = null;
	}
}
