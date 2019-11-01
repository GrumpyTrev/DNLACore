using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace DBTest
{
	class LibraryScanner
	{
		public LibraryScanner( Library libraryToScan, SQLiteConnection db )
		{
			scanLibrary = libraryToScan;
			connection = db;
		}

		public async void ScanLibrary()
		{
			// Make sure the children links are read as well to the get Source entries
			scanLibrary = connection.GetWithChildren< Library >( scanLibrary.Id );

			// Iterate through the sources for this libaray
			foreach ( Source sourceToScan in scanLibrary.Sources )
			{
				sourceBeingScanned = connection.GetWithChildren<Source>( sourceToScan.Id );

				if ( sourceToScan.ScanType == "FTP" )
				{
					FTPScanner scanner = new FTPScanner();
					scanner.SongsScanned += Scanner_SongsScanned;

					await scanner.Scan( sourceToScan.ScanSource );
				}
			}
		}

		private void Scanner_SongsScanned( object sender, FTPScanner.SongsScannedArgs scannedSongs )
		{
			Dictionary<string, ScannedAlbum> albumGroups = new Dictionary<string, ScannedAlbum>();

			// Group the list of songs according to the associated album, and determine if all the songs are by the same artist
			foreach ( ScannedSong song in scannedSongs.ScannedSongs )
			{
				// Replace an empty artist or album name
				if ( song.Tags.Artist.Length == 0 )
				{
					song.Tags.Artist = "<Unknown>";
				}

				if ( song.Tags.Album.Length == 0 )
				{
					song.Tags.Album = "<Unknown>";
				}

				// Replace an empty track with 0
				if ( song.Tags.Track.Length == 0 )
				{
					song.Tags.Track = "0";
				}

				// Parse the track field to an integer track number
				// Use leading digits only in case the format is n/m
				try
				{
					song.Track = Int32.Parse( Regex.Match( song.Tags.Track, @"\d+" ).Value );
				}
				catch ( Exception formatException )
				{
				}

				song.Length = ( int )song.Tags.Length.TotalSeconds;

				// If there is an Album Artist tag then use it for the song name, otherwise use the Artist tag
				if ( song.Tags.AlbumArtist.Length > 0 )
				{
					song.ArtistName = song.Tags.AlbumArtist;
				}
				else
				{
					// The artist tag may consist of multiple parts divided by a '/'.
					string[] artists = song.Tags.Artist.Split( '/' );
					song.ArtistName = artists[ 0 ];
				}

				// Is there a group for this album
				if ( albumGroups.ContainsKey( song.Tags.Album ) == false )
				{
					albumGroups[ song.Tags.Album ] = new ScannedAlbum() { Name = song.Tags.Album };
				}

				ScannedAlbum album = albumGroups[ song.Tags.Album ];
				album.Songs.Add( song );

				// If this is not the first song in the group then check if the artist is the same
				if ( album.Songs.Count > 1 )
				{
					if ( album.Songs[ 0 ].ArtistName.ToUpper() != song.ArtistName.ToUpper() )
					{
						album.SingleArtist = false;
					}
				}
			}

			// Store the scanned song data
			foreach ( ScannedAlbum album in albumGroups.Values )
			{
				StoreAlbum( album );
			}
		}

		private void StoreAlbum( ScannedAlbum album )
		{
			Logger.Log( string.Format( "Album: {0} Single artist: {1}", album.Name, album.SingleArtist ) );

			// Get an existing or new Album entry for the songs
			Album songAlbum = GetAlbumToHoldSongs( album );

			// Keep track of Artist and ArtistAlbum entries in case they can be reused
			ArtistAlbum songArtistAlbum = null;
			Artist songArtist = null;

			// Add all the songs to the album
			foreach ( ScannedSong songScanned in album.Songs )
			{
				// If there is no existing Artist entry or the current one if for the wrong Artist name then get or create a new one
				if ( ( songArtist == null ) || ( songArtist.Name != songScanned.ArtistName ) )
				{
					// Find the Artist for this song
					songArtist = GetArtistToHoldSongs( songScanned.ArtistName );

					// Clear the ArtistAlbum that was being used. Need to get a new one for this Artist
					// Save any changes first though.
					if ( songArtistAlbum != null )
					{
						connection.UpdateWithChildren( songArtistAlbum );
						songArtistAlbum = null;
					}
				}

				// If there is an existing ArtistAlbum then use it as it will be for the correct Artist, i.e. not cleared above
				if ( songArtistAlbum == null )
				{
					// Find an exisiting or create a new ArtistAlbum entry
					songArtistAlbum = GetArtistAlbumToHoldSongs( songArtist, songAlbum );
				}

				// Add the song to the database, the album and the album artist
				Song songToAdd = new Song() { Title = songScanned.Tags.Title, Track = songScanned.Track, Path = songScanned.SourcePath,
					ModifiedTime = songScanned.Modified, Length = songScanned.Length };
				connection.Insert( songToAdd );

				MP3Tags tags = songScanned.Tags;
				Logger.Log( string.Format( "Artist: {0} Title: {1} Track: {2} Modified: {3} Length {4}", tags.Artist, tags.Title, tags.Track, 
					songScanned.Modified, songScanned.Length ) );

				// Add to the Album
				songAlbum.Songs.Add( songToAdd );

				// Add to the source
				sourceBeingScanned.Songs.Add( songToAdd );

				// Add to the ArtistAlbum
				songArtistAlbum.Songs.Add( songToAdd );
			}

			// Update the db with the song collections in the Album and Source
			connection.UpdateWithChildren( songAlbum );
			connection.UpdateWithChildren( sourceBeingScanned );
			connection.UpdateWithChildren( songArtistAlbum );
		}

		/// <summary>
		/// Either find an existing album or create a new album to hold the songs.
		/// If all songs are from the same artist then check is there is an existing album of the same name associated with that artist.
		/// If the songs are from different artists then check in the "Various Artists" artist.
		/// These artists may not exist at this stage
		/// </summary>
		/// <returns></returns>
		private Album GetAlbumToHoldSongs( ScannedAlbum album )
		{
			Album songAlbum = null;

			string artistName = ( album.SingleArtist == true ) ? album.Songs[ 0 ].ArtistName : artistName = "Various Artists";

			// Check if the artist already exists in the library
			Artist songArtist = connection.GetAllWithChildren<Artist>( p => ( p.Name.ToUpper() == artistName.ToUpper() ) && 
				( p.LibraryId == scanLibrary.Id ) ).FirstOrDefault();

			// If the artist exists then check for existing album. The artist will hold ArtistAlbum entries rather than Album entries, but the ArtistAlbum entries
			// have the same name as the albums
			if ( songArtist != null )
			{
				ArtistAlbum songArtistAlbum = songArtist.ArtistAlbums.SingleOrDefault( p => ( p.Name.ToUpper() == album.Songs[ 0 ].Tags.Album.ToUpper() ) );
				if ( songArtistAlbum != null )
				{
					// Get the existing album from the database
					songAlbum = connection.GetAllWithChildren<Album>( p => ( p.Id == songArtistAlbum.AlbumId ) ).First();
				}
			}

			Logger.Log( string.Format( "Album: {0} {1} for artist {2}", album.Name, ( songAlbum != null ) ? "found" : "not found", artistName ) );

			// If no existing album create a new one
			if ( songAlbum == null )
			{
				songAlbum = new Album() { Name = album.Name, Songs = new List<Song>() };
				connection.Insert( songAlbum );

				// Add to Library
				scanLibrary.Albums.Add( songAlbum );
				connection.UpdateWithChildren( scanLibrary );
			}

			return songAlbum;
		}

		/// <summary>
		/// Get an existing Artist or create a new one.
		/// </summary>
		/// <param name="artistName"></param>
		/// <returns></returns>
		private Artist GetArtistToHoldSongs( string artistName )
		{
			Artist songArtist = null;

			// Find the Artist for this song
			songArtist = connection.GetAllWithChildren<Artist>( p => ( p.Name.ToUpper() == artistName.ToUpper() ) && 
				( p.LibraryId == scanLibrary.Id ) ).FirstOrDefault();

			Logger.Log( string.Format( "Artist: {0} {1}", artistName, ( songArtist != null ) ? "found" : "not found creating in db" ) );

			if ( songArtist == null )
			{
				// Create a new Artist and add it to the database
				songArtist = new Artist() { Name = artistName, ArtistAlbums = new List<ArtistAlbum>() };
				connection.Insert( songArtist );

				// Add it to the library
				scanLibrary.Artists.Add( songArtist );
				connection.UpdateWithChildren( scanLibrary );
			}

			return songArtist;
		}


		/// <summary>
		/// Get an existing or new ArtistAlbum entry to hold the songs associated with a particular Artist
		/// </summary>
		/// <param name="songArtist"></param>
		/// <param name="songAlbum"></param>
		/// <returns></returns>
		private ArtistAlbum GetArtistAlbumToHoldSongs( Artist songArtist, Album songAlbum )
		{
			ArtistAlbum songArtistAlbum = null;

			// Find an exisiting or create a new ArtistAlbum entry
			songArtistAlbum = songArtist.ArtistAlbums.SingleOrDefault( p => ( p.Name.ToUpper() == songAlbum.Name.ToUpper() ) );

			Logger.Log( string.Format( "ArtistAlbum: {0} {1}", songAlbum.Name, ( songArtistAlbum != null ) ? "found" : "not found creating in db" ) );

			if ( songArtistAlbum == null )
			{
				songArtistAlbum = new ArtistAlbum() { Name = songAlbum.Name, Album = songAlbum, Songs = new List<Song>() };
				connection.Insert( songArtistAlbum );

				// Add it to the Artist
				songArtist.ArtistAlbums.Add( songArtistAlbum );
				connection.UpdateWithChildren( songArtist );
			}
			else
			{
				// Get the children of the existing ArtistAlbum
				connection.GetChildren<ArtistAlbum>( songArtistAlbum );
			}

			return songArtistAlbum;
		}

		private SQLiteConnection connection = null;

		private Library scanLibrary = null;

		private Source sourceBeingScanned = null;
	}
}