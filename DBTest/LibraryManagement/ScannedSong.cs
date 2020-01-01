using System;

namespace DBTest
{
	class ScannedSong
	{
		public MP3Tags Tags { get; set; }
		public DateTime Modified { get; set; }
		public string SourcePath { get; set; }
		public string ArtistName { get; set; }
		public int Track { get; set; } = 0;
		public int Length { get; set; } = 0;
		public bool Matched { get; set; } = false;
	}
}