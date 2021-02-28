using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// View holder for the group Album items
	/// </summary>
	class AlbumViewHolder : ExpandableListViewHolder
	{
		public void DisplayAlbum( Album album, bool actionMode, bool showGenre, string genreText )
		{
			// Save the default colour if not already done so
			if ( albumNameColour == Color.Fuchsia )
			{
				albumNameColour = new Color( AlbumName.CurrentTextColor );
			}

			AlbumName.Text = album.Name;
			AlbumName.SetTextColor( ( album.Played == true ) ? Color.Black : albumNameColour );

			ArtistName.Text = ( album.ArtistName.Length > 0 ) ? album.ArtistName : "Unknown";

			// Set the year text
			string yearText = ( album.Year > 0 ) ? album.Year.ToString() : " ";

			// If genres are being displayed then show the genre layout and set the genre name
			// Get the genre layout view so we can show or hide it
			if ( ( showGenre == true ) && ( album.Genre.Length > 0 ) )
			{
				// When genres are displayed the genre and year are displayed on their own line. So hide the year field that sit on the album anme line
				GenreLayout.Visibility = ViewStates.Visible;
				Year.Visibility = ViewStates.Gone;

				// Display the genres. Replace any spaces in the genres with non-breaking space characters. This prevents a long genres string with a 
				// space near the start being broken at the start, It just looks funny.
				Genre.Text = genreText.Replace( ' ', '\u00a0' );

				// Set the year
				GenreYear.Text = yearText;
			}
			else
			{
				// Hide the seperate genre line and make sure the year field is shown on the album name line
				GenreLayout.Visibility = ViewStates.Gone;
				Year.Visibility = ViewStates.Visible;
				Year.Text = yearText;
			}
		}

		public TextView AlbumName { get; set; }
		public TextView ArtistName { get; set; }
		public TextView Year { get; set; }
		public TextView GenreYear { get; set; }
		public TextView Genre { get; set; }
		public RelativeLayout GenreLayout { get; set; }

		/// <summary>
		/// The Colour used to display the name of an album. Initialised to a colour we're never going to use
		/// </summary>
		private static Color albumNameColour = new Color( Color.Fuchsia );
	}
}