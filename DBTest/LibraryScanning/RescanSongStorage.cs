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
		/// If it's a exisiting song with the same modified time then the song does not require any further scanning
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
				// Get the associated ArtistAlbum and Album entries
				ArtistAlbum artistAlbum = await ArtistAccess.GetArtistAlbumAsync( matchedSong.ArtistAlbumId );
				Album album = await AlbumAccess.GetAlbumAsync( artistAlbum.AlbumId );

				// If the artist or album name has changed then treat this as a new song. Otherwise update the existing song in the library
				if ( ( album.ArtistName.ToUpper() != song.ArtistName.ToUpper() ) || ( album.Name.ToUpper() != song.Tags.Album.ToUpper() ) )
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