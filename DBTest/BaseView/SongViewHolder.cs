using System;
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

		public TextView Artist { get; set; }
		public TextView Title { get; set; }
		public TextView Duration { get; set; }
	}
}
