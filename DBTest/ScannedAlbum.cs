using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The ScannedAlbum class contains one or more songs from the same album obtained from a single folder
	/// </summary>
	class ScannedAlbum
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