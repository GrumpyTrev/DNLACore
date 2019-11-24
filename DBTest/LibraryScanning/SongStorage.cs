﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The SongStorage class is responsible for storing scanned songs and associated albums in the database
	/// </summary>
	class SongStorage : ISongStorage
	{
		public SongStorage( Library libraryToScan, Source sourceToScan )
		{
			scanLibrary = libraryToScan;
			sourceBeingScanned = sourceToScan;
		}

		/// <summary>
		/// Called when a group of songs from the same folder have been scanned
		/// Group the songs into albums by album name and then process each of these albums
		/// </summary>
		/// <param name="songs"></param>
		public virtual async void SongsScanned( List<ScannedSong> songs )
		{
			Dictionary<string, ScannedAlbum> albumGroups = new Dictionary<string, ScannedAlbum>();

			// Group the list of songs according to the associated album, and determine if all the songs are by the same artist
			foreach ( ScannedSong song in songs )
			{
				// Replace empty tags etc.
				NormaliseTags( song );

				// Is there a group for this album
				if ( albumGroups.TryGetValue( song.Tags.Album, out ScannedAlbum album ) == false )
				{
					album = new ScannedAlbum() { Name = song.Tags.Album };
					albumGroups[ song.Tags.Album ] = album;
				}

				// Add the song to the album
				album.Songs.Add( song );

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
				await StoreAlbumAsync( album );
			}
		}

		public virtual bool DoesSongRequireScanning( string filepath, DateTime modifiedTime )
		{
			return true;
		}

		/// <summary>
		/// Replace zero length tag files with standard replacements, parse the track number and the length
		/// </summary>
		/// <param name="song"></param>
		private void NormaliseTags( ScannedSong song )
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
			catch ( Exception )
			{
			}

			// Convert from a TimeSpan to seconds
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
		}

		/// <summary>
		/// Called to process a group of songs belonging to the same album name ( but not necessarily the same artist )
		/// </summary>
		/// <param name="album"></param>
		private async Task StoreAlbumAsync( ScannedAlbum album )
		{
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
					// As this is a new Artist the old ArtistAlbum if there was one needs to be stored away.
					if ( songArtistAlbum != null )
					{
						await ArtistAccess.UpdateArtistAlbumAsync( songArtistAlbum );
						songArtistAlbum = null;
					}

					// Find the Artist for this song
					songArtist = await GetArtistToHoldSongsAsync( songScanned.ArtistName );
				}

				// If there is an existing ArtistAlbum then use it as it will be for the correct Artist, i.e. not cleared above
				if ( songArtistAlbum == null )
				{
					// Find an exisiting or create a new ArtistAlbum entry
					songArtistAlbum = await GetArtistAlbumToHoldSongsAsync( songArtist, songAlbum );
				}

				// Add the song to the database, the album and the album artist
				Song songToAdd = new Song() {
					Title = songScanned.Tags.Title, Track = songScanned.Track, Path = songScanned.SourcePath,
					ModifiedTime = songScanned.Modified, Length = songScanned.Length
				};
				await ArtistAccess.AddSongAsync( songToAdd );

				Logger.Log( string.Format( "Artist: {0} Title: {1} Track: {2} Modified: {3} Length {4}", songScanned.Tags.Artist, songScanned.Tags.Title,
					songScanned.Tags.Track, songScanned.Modified, songScanned.Length ) );

				// Add to the Album
				songAlbum.Songs.Add( songToAdd );

				// Add to the source
				sourceBeingScanned.Songs.Add( songToAdd );

				// Add to the ArtistAlbum
				songArtistAlbum.Songs.Add( songToAdd );
			}

			// Update the db with the song collections in the Album, Source and ArtistAlbum
			await ArtistAccess.UpdateAlbumAsync( songAlbum );
			await LibraryAccess.UpdateSourceAsync( sourceBeingScanned );
			await ArtistAccess.UpdateArtistAlbumAsync( songArtistAlbum );
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

			string artistName = ( album.SingleArtist == true ) ? album.Songs[ 0 ].ArtistName : "Various Artists";

			// Check if the artist already exists in the library. The 'scanLibrary' can be used for this search as it is already fully populated with the 
			// artists
			Artist songArtist = scanLibrary.Artists.SingleOrDefault( p => ( p.Name.ToUpper() == artistName.ToUpper() ) );

			// If the artist exists then check for existing album. The artist will hold ArtistAlbum entries rather than Album entries, but the ArtistAlbum entries
			// have the same name as the albums. Cannot just use the album name as that may not be unique.
			if ( songArtist != null )
			{
				// Make sure that the Artist's children are available
				if ( songArtist.ArtistAlbums == null )
				{
					await ArtistAccess.GetArtistChildrenAsync( songArtist );
				}

				ArtistAlbum songArtistAlbum = songArtist.ArtistAlbums.SingleOrDefault( p => ( p.Name.ToUpper() == album.Songs[ 0 ].Tags.Album.ToUpper() ) );
				if ( songArtistAlbum != null )
				{
					// The Album is a child of this ArtistAlbum so..
					if ( songArtistAlbum.Album == null )
					{
						await ArtistAccess.GetArtistAlbumChildrenAsync( songArtistAlbum );
					}

					songAlbum = songArtistAlbum.Album;
				}
			}

			Logger.Log( string.Format( "Album: {0} {1} for artist {2}", album.Name, ( songAlbum != null ) ? "found" : "not found", artistName ) );

			// If no existing album create a new one
			if ( songAlbum == null )
			{
				songAlbum = new Album() { Name = album.Name, Songs = new List<Song>() };
				await ArtistAccess.AddAlbumAsync( songAlbum );

				// Add to Library
				scanLibrary.Albums.Add( songAlbum );
				await LibraryAccess.UpdateLibraryAsync( scanLibrary );
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
			Artist songArtist = null;

			// Find the Artist for this song. The 'scanLibrary' can be used for this search as it is already fully populated with the 
			// artists
			songArtist = scanLibrary.Artists.SingleOrDefault( p => ( p.Name.ToUpper() == artistName.ToUpper() ) );

			Logger.Log( string.Format( "Artist: {0} {1}", artistName, ( songArtist != null ) ? "found" : "not found creating in db" ) );

			if ( songArtist == null )
			{
				// Create a new Artist and add it to the database
				songArtist = new Artist() { Name = artistName, ArtistAlbums = new List<ArtistAlbum>() };
				await ArtistAccess.AddArtistAsync( songArtist );

				// Add it to the library
				scanLibrary.Artists.Add( songArtist );
				await LibraryAccess.UpdateLibraryAsync( scanLibrary );
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
				songArtistAlbum = new ArtistAlbum() { Name = songAlbum.Name, Album = songAlbum, Songs = new List<Song>() };
				await ArtistAccess.AddArtistAlbumAsync( songArtistAlbum );

				songArtist.ArtistAlbums.Add( songArtistAlbum );

				// Update this relationship in the database
				await ArtistAccess.UpdateArtistAsync( songArtist );
			}
			else
			{
				// Get the children of the existing ArtistAlbum
				await ArtistAccess.GetArtistAlbumChildrenAsync( songArtistAlbum );
			}

			return songArtistAlbum;
		}

		/// <summary>
		/// The Source to insert Songs into
		/// </summary>
		private Source sourceBeingScanned = null;

		/// <summary>
		/// The Library to insert Artists and Albums into
		/// </summary>
		private Library scanLibrary = null;
	}

	/// <summary>
	/// Interface that all SongStorage type classes need to implement
	/// </summary>
	internal interface ISongStorage
	{
		/// <summary>
		/// Method called when a group of songs from the same folder have been scanned in
		/// </summary>
		/// <param name="songs"></param>
		void SongsScanned( List<ScannedSong> songs );

		/// <summary>
		/// Method called when checking whether or not a song requires scanning (downloading)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="modifiedTime"></param>
		/// <returns></returns>
		bool DoesSongRequireScanning( string filepath, DateTime modifiedTime );
	}
}