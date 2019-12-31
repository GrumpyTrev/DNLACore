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
		/// Called when a group of songs from the same folder have been scanned
		/// Special processing is required here for existing songs that have been modified.
		/// For now filter these out of the list and let the base class handle the new songs
		/// </summary>
		/// <param name="songs"></param>
		public override Task SongsScanned( List<ScannedSong> songs )
		{
			songs.RemoveAll( song => songLookup.ContainsKey( song.SourcePath ) == true );
			return base.SongsScanned( songs );
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
				// Compare both times as is and then take 1 hour off the stored time
				if ( ( matchedSong.ModifiedTime == modifiedTime ) || ( matchedSong.ModifiedTime.AddHours( -1 ) == modifiedTime ) )
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
		/// Collection used to lookup esxisting songs
		/// </summary>
		private Dictionary<string, Song> songLookup = null;
	}
}