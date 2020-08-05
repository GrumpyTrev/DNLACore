using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The RescanSongStorage class derives from SongStorage in order to check for songs that don't require scanning and
	/// also to handle updated songs
	/// </summary>
	internal class RescanSongStorage : SongStorage
	{
		/// <summary>
		/// Public constructor
		/// </summary>
		/// <param name="libraryToScan"></param>
		/// <param name="sourceToScan"></param>
		public RescanSongStorage( Library libraryToScan, Source sourceToScan, Dictionary<string, Song> pathLookup ) : base ( libraryToScan, sourceToScan )
		{
			songLookup = pathLookup;
		}

		/// <summary>
		/// Called when the filepath and modified time for a song have been determined
		/// If it's a existing song with the same modified time then the song does not require any further scanning
		/// Otherwise let scanning continue
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="modifiedTime"></param>
		/// <returns></returns>
		public override bool DoesSongRequireScanning( string filepath, DateTime modifiedTime )
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

				// TESTING - mark all existing files as scanRequired = true and differ
				scanRequired = true;
				matchedSong.ScanAction = Song.ScanActionType.Differ;
			}

			return scanRequired;
		}

		/// <summary>
		/// Called to determine whether a song that has been scanned requires adding to the library, or just an exisiting entry updated
		/// </summary>
		/// <param name="song"></param>
		/// <returns></returns>
		public override async Task <bool> DoesSongRequireAdding( ScannedSong song )
		{
			bool needsAdding = true;

			// Lookup the path of this song in the dictionary
			if ( ( songLookup.TryGetValue( song.SourcePath, out Song matchedSong ) == true ) && ( matchedSong.ScanAction == Song.ScanActionType.Differ ) )
			{
				// Need to check whether the matched Artist or Album names have changed. If they have then treat this as a new song and mark
				// the matched song for deletion
				// Previously the artist name stored in the existing Album object was used to check for an artist naem change. Use the Artist record instead
				// as the name in the Albm record may be "Various Artists" for instance.
				ArtistAlbum matchedArtistAlbum = await ArtistAccess.GetArtistAlbumAsync( matchedSong.ArtistAlbumId );
				Artist matchedArtist = await ArtistAccess.GetArtistAsync( matchedArtistAlbum.ArtistId );

				// If the artist or album name has changed then treat this as a new song. Otherwise update the existing song in the library
				if ( ( matchedArtist.Name.ToUpper() != song.ArtistName.ToUpper() ) || ( matchedArtistAlbum.Name.ToUpper() != song.Tags.Album.ToUpper() ) )
				{
					// Mark the existing song for deletion
					matchedSong.ScanAction = Song.ScanActionType.NotMatched;
				}
				else
				{
					matchedSong.Length = song.Length;
					matchedSong.ModifiedTime = song.Modified;
					matchedSong.Title = song.Tags.Title;
					matchedSong.Track = song.Track;
					await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedSong );

					needsAdding = false;

					// Check if the year or genre fields on the album needs updating
					// Don't update the Album if it is a 'various artists' album as the these fields is not applicable
					Album matchedAlbum = await AlbumAccess.GetAlbumAsync( matchedArtistAlbum.AlbumId );

					if ( matchedAlbum.ArtistName != SongStorage.VariousArtistsString )
					{
						// Update the stored year if it is different to the artist year and the artist year is defined.
						// Log when a valid year is overwritten by different year
						if ( matchedAlbum.Year != song.Year )
						{
							if ( song.Year != 0 )
							{
								if ( matchedAlbum.Year != 0 )
								{
									Logger.Log( string.Format( "Album year is {0} song year is {1}", matchedAlbum.Year, song.Year ) );
								}

								matchedAlbum.Year = song.Year;
								await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedAlbum );
							}
						}

						if ( song.Tags.Genre.Length > 0 )
						{
							// If this album has a genre id then get the associated genre record
							Genre albumGenre = await FilterAccess.GetGenreByIdAsync( matchedAlbum.GenreId );

							if ( ( albumGenre == null ) || ( albumGenre.Name != song.Tags.Genre ) )
							{
								// If this is a new genre then add it to the genres list.
								albumGenre = await FilterAccess.GetGenreByNameAsync( song.Tags.Genre );
								if ( albumGenre == null )
								{
									albumGenre = new Genre() { Name = song.Tags.Genre };
									await FilterAccess.AddGenre( albumGenre );
								}

								matchedAlbum.GenreId = albumGenre.Id;
								await ConnectionDetailsModel.AsynchConnection.UpdateAsync( matchedAlbum );

							}
						}
					}
				}
			}

			return needsAdding;
		}

		/// <summary>
		/// Collection used to lookup esxisting songs
		/// </summary>
		private Dictionary<string, Song> songLookup = null;
	}
}