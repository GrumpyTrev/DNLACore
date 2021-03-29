using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The SongsDisplayAdapter is used to display AlbumPlaylistItem songs in a list view
	/// </summary>
	class SongsDisplayAdapter : BaseAdapter, AdapterView.IOnItemClickListener
	{
		/// <summary>
		/// Save the data and st up the click listener
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parent"></param>
		/// <param name="songs"></param>
		/// <param name="lastPlayedId"></param>
		/// <param name="clickAction"></param>
		public SongsDisplayAdapter( Context context, ListView parent, List<Song> songsToDisplay, int lastPlayedId, Action clickAction )
		{
			inflator = LayoutInflater.FromContext( context );
			songs = songsToDisplay;
			lastPlayedSongId = lastPlayedId;
			parent.OnItemClickListener = this;
			onClickAction = clickAction;
		}

		/// <summary>
		/// The following are required by BaseAdapter
		/// </summary>
		/// <returns></returns>
		public override Java.Lang.Object GetItem( int position ) => position;
		public override long GetItemId( int position ) => position;

		/// <summary>
		/// Only one view type so always return 0
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public override int GetItemViewType( int position ) => 0;

		/// <summary>
		/// Only one view type
		/// </summary>
		public override int ViewTypeCount => 1;

		/// <summary>
		/// Called to display a particular song
		/// </summary>
		/// <param name="position"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.popup_song_layout, null );
				convertView.Tag = new SongViewHolder()
				{
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration )
				};
			}

			// Display the song
			( ( SongViewHolder )convertView.Tag ).DisplaySong( songs[ position ] );

			// If this song is currently being played then show it with a different background
			convertView.SetBackgroundColor( songs[ position ].Id == lastPlayedSongId ? Color.AliceBlue : Color.Transparent );

			return convertView;
		}

		/// <summary>
		/// Called when a list view item has been clicked. Invoke the on click action
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="view"></param>
		/// <param name="position"></param>
		/// <param name="id"></param>
		public void OnItemClick( AdapterView parent, View view, int position, long id ) => onClickAction?.Invoke();

		/// <summary>
		/// The number of items in the list
		/// </summary>
		public override int Count => songs.Count;

		/// <summary>
		/// View holder for the child Song items
		/// </summary>
		private class SongViewHolder : Java.Lang.Object
		{
			public void DisplaySong( Song song )
			{
				Title.Text = song.Title;
				Duration.Text = TimeSpan.FromSeconds( song.Length ).ToString( @"mm\:ss" );
			}

			public TextView Title { get; set; }
			public TextView Duration { get; set; }
		}

		/// <summary>
		/// The inflator for the view
		/// </summary>
		private readonly LayoutInflater inflator = null;

		/// <summary>
		/// The songs to display
		/// </summary>
		private readonly List<Song> songs = null;

		/// <summary>
		/// The last played song for this album
		/// </summary>
		private readonly int lastPlayedSongId = -1;

		/// <summary>
		/// The action to be performed when an item is clicked
		/// </summary>
		private readonly Action onClickAction = null;
	}
}