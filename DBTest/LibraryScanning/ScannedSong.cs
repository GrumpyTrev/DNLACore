using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DBTest
{
	/// <summary>
	/// The ScannedSong class holds the MP3 tags and other relavent fields of a newly scanned song 
	/// </summary>
	class ScannedSong
	{
		public MP3Tags Tags { get; set; }
		public DateTime Modified { get; set; }
		public string SourcePath { get; set; }
		public string ArtistName { get; set; }
		public int Track { get; set; } = 0;
		public int Length { get; set; } = 0;
		public int Year { get; set; } = 0;
		public bool Matched { get; set; } = false;

		/// <summary>
		/// Replace zero length tag files with standard replacements, parse the track number and the length
		/// </summary>
		/// <param name="song"></param>
		public void NormaliseTags()
		{
			// Replace an empty artist or album name
			if ( Tags.Artist.Length == 0 )
			{
				Tags.Artist = "<Unknown>";
			}

			if ( Tags.Album.Length == 0 )
			{
				Tags.Album = "<Unknown>";
			}

			// Replace an empty track with 0
			if ( Tags.Track.Length == 0 )
			{
				Tags.Track = "0";
			}

			// Parse the track field to an integer track number
			// Use leading digits only in case the format is n/m
			try
			{
				Track = Int32.Parse( Regex.Match( Tags.Track, @"\d+" ).Value );
			}
			catch ( Exception )
			{
			}

			// Convert from a TimeSpan to seconds
			Length = ( int )Tags.Length.TotalSeconds;

			// If there is an Album Artist tag then use it for the song name, otherwise use the Artist tag
			if ( Tags.AlbumArtist.Length > 0 )
			{
				ArtistName = Tags.AlbumArtist;
			}
			else
			{
				// The artist tag may consist of multiple parts divided by a '/'.
				ArtistName = Tags.Artist.Split( '/' )[ 0 ];
			}

			// Replace an empty Year with 0
			if ( Tags.Year.Length == 0 )
			{
				Tags.Year = "0";
			}

			// Parse the year field to an integer year number
			try
			{
				Year = Int32.Parse( Tags.Year );
			}
			catch ( Exception )
			{
			}

			// The genre tag may consist of multiple parts divided by a ';'. We need to keep all of them but remove any whitespace around them
			List<string> genres = Tags.Genre.Split( ';' ).ToList();
			genres.ForEach( gen => gen = gen.Trim() );
			
			Tags.Genre = string.Join( ';', genres );
		}
	}
}