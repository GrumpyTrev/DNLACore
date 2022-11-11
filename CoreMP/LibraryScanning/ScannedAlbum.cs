using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The ScannedAlbum class contains one or more songs from the same album obtained from a single folder
	/// </summary>
	internal class ScannedAlbum
	{
		/// <summary>
		/// The name of the album
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// THe songs associated with this album
		/// </summary>
		public List<ScannedSong> Songs { get; set; } = new List<ScannedSong>();

		/// <summary>
		/// Are all the songs from the same artist
		/// </summary>
		public bool SingleArtist { get; set; } = true;
	}
}
