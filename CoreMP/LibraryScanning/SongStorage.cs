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
		public SongStorage( Source sourceToScan, Dictionary<string, Song> pathLookup )
		{
			sourceBeingScanned = sourceToScan;
			songLookup = pathLookup;
		}

		/// <summary>
		/// Called when a group of songs from the same folder have been scanned
		/// Group the songs into albums by album name
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
					album = new ScannedAlbum() { Name = song.Tags.Album, ScanSource = sourceBeingScanned };
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

			// Save the scanned albums for a second pass when they will be stored in the library
			foreach ( ScannedAlbum album in albumGroups.Values )
			{
				if ( album.Songs.Count > 0 )
				{
					NewAlbums.Add( album );
				}
			}
		}

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
		/// Allow the collection to be accessed by path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Song GetSongFromPath( string path ) => songLookup.GetValueOrDefault( path );

		/// <summary>
		/// The ScannedAlbum instances that have been scanned and need processing through the library
		/// </summary>
		public List<ScannedAlbum> NewAlbums { get; } = new List<ScannedAlbum>();

		/// <summary>
		/// Songs that have had their album names changed and must be removed from the library
		/// </summary>
		public List<Song> SongsWithChangedAlbumNames { get; } = new List<Song>();

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
				// Need to check whether the matched Artist or Album names have changed. If they have then treat this as a new song and mark
				// the matched song for deletion
				ArtistAlbum matchedArtistAlbum = ArtistAlbums.GetArtistAlbumById( matchedSong.ArtistAlbumId );
				Artist matchedArtist = Artists.GetArtistById( matchedArtistAlbum.ArtistId );

				// If the artist or album name has changed then treat this as a new song. Otherwise update the existing song in the library
				if ( ( matchedArtist.Name.ToUpper() != song.ArtistName.ToUpper() ) || ( matchedArtistAlbum.Name.ToUpper() != song.Tags.Album.ToUpper() ) )
				{
					// The existing song needs to be deleted.
					// If the song is not deleted then there will be more than one associated with the same storage location.
					// This is much less complicated than trying to move the existing song to a new Artist or Album
					SongsWithChangedAlbumNames.Add( matchedSong );
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
		/// The name given to the artist in albums when they contain songs from multiple artists
		/// </summary>
		public const string VariousArtistsString = "Various Artists";

		/// <summary>
		/// The Source to insert Songs into
		/// </summary>
		private readonly Source sourceBeingScanned = null;

		/// <summary>
		/// Collection used to lookup esxisting songs
		/// </summary>
		private readonly Dictionary<string, Song> songLookup = null;
	}
}
