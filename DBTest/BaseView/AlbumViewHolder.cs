using Android.Graphics;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// View holder for the group Album items
	/// </summary>
	internal class AlbumViewHolder : ExpandableListViewHolder
	{
		public void DisplayAlbum( Album album, bool actionMode, string genreText )
		{
			// Save the default colour if not already done so
			if ( albumNameColour == Color.Fuchsia )
			{
				albumNameColour = new Color( AlbumName.CurrentTextColor );
			}

			AlbumName.Text = album.Name;
			AlbumName.SetTextColor( ( album.Played == true ) ? Color.Black : albumNameColour );

			ArtistName.Text = ( album.ArtistName.Length > 0 ) ? album.ArtistName : "Unknown";

			Year.Text = ( album.Year > 0 ) ? album.Year.ToString() : " ";

			// Display the genres. Replace any spaces in the genres with non-breaking space characters. This prevents a long genres string with a 
			// space near the start being broken at the start, It just looks funny.
			Genre.Text = genreText.Replace( ' ', '\u00a0' );
		}

		public TextView AlbumName { get; set; }
		public TextView ArtistName { get; set; }
		public TextView Year { get; set; }
		public TextView Genre { get; set; }

		/// <summary>
		/// The Colour used to display the name of an album. Initialised to a colour we're never going to use
		/// </summary>
		private static Color albumNameColour = new Color( Color.Fuchsia );
	}
}
