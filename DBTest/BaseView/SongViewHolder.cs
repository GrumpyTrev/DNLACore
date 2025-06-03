using System;
using System.Resources;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// View holder for the group Song items
	/// </summary>
	internal class SongViewHolder : ExpandableListViewHolder
	{
		public void DisplaySong( SongPlaylistItem playlistItem )
		{
			Title.Text = playlistItem.Song.Title;
			Duration.Text = TimeSpan.FromSeconds( playlistItem.Song.Length ).ToString( @"mm\:ss" );
			Artist.Text = string.Format( "{0} : {1}", playlistItem.Artist.Name, playlistItem.Song.Album.Name );
		}

		public void Highlight()
		{
			Title.TextSize = 20;
//			Title.TextSize = Title.Context.Resources.GetDimensionPixelSize( Resource.Dimension.text_size_heading );
		}

		public void UnHighlight()
		{
//			Title.TextSize = Title.Context.Resources.GetDimensionPixelSize( Resource.Dimension.text_size_normal );
			Title.TextSize = 15;
		}

		public TextView Artist { get; set; }
		public TextView Title { get; set; }
		public TextView Duration { get; set; }
	}
}
