using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The AlbumsAdapter class displays album and song data in an ExpandableListView
	/// </summary>
	/// <remarks>
	/// AlbumsAdapter constructor
	/// </remarks>
	/// <param name="context"></param>
	/// <param name="parentView"></param>
	/// <param name="provider"></param>
	internal class AlbumsAdapter( Context context, ExpandableListView parentView,
		ExpandableListAdapter<Album>.IGroupContentsProvider<Album> provider, IAdapterEventHandler actionHandler ) :
		ExpandableListAdapter<Album>( context, parentView, provider, AlbumsAdapterModel.BaseModel, actionHandler )
	{
		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => Groups[ groupPosition ].Songs?.Count ?? 0;

		/// <summary>
		/// There is only one Child Type - the songs
		/// </summary>
		public override int ChildTypeCount => 1;

		/// <summary>
		/// As there is only one Child Type always return 0
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override int GetChildType( int groupPosition, int childPosition ) => 0;

		/// <summary>
		/// There is one group type, the album
		/// </summary>
		public override int GroupTypeCount => 1;

		/// <summary>
		/// As there is only one group Type always return 0
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetGroupType( int groupPosition ) => 0;

		/// <summary>
		/// Get the starting position for a section
		/// </summary>
		/// <param name="sectionIndex"></param>
		/// <returns></returns>
		public override int GetPositionForSection( int sectionIndex ) => fastScrollSections[ sectionIndex ].Item2;

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public override Java.Lang.Object[] GetSections() => javaSections;

		/// <summary>
		/// Provide a view containing song details at the specified position
		/// Attempt to reuse the supplied view if it previously contained the same type of detail.
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="isLastChild"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent )
		{
			// If the supplied view is null then create one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_song_layout, null );
				convertView.Tag = new SongViewHolder()
				{
					Track = convertView.FindViewById<TextView>( Resource.Id.track ),
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration )
				};
			}

			// Display the Track number, Title and Duration
			( ( SongViewHolder )convertView.Tag ).DisplaySong( Groups[ groupPosition ].Songs[ childPosition ] );

			return convertView;
		}

		/// <summary>
		/// View holder for the child Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder
		{
			public void DisplaySong( Song song )
			{
				Track.Text = song.Track.ToString();
				Title.Text = song.Title;
				Duration.Text = TimeSpan.FromSeconds( song.Length ).ToString( @"mm\:ss" );
			}

			public TextView Track { get; set; }
			public TextView Title { get; set; }
			public TextView Duration { get; set; }
		}

		/// <summary>
		/// Derived classes must implement this method to provide a view for a group item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			// If the supplied view is null then create one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_album_layout, null );
				convertView.Tag = new AlbumViewHolder()
				{
					AlbumName = convertView.FindViewById<TextView>( Resource.Id.albumName ),
					ArtistName = convertView.FindViewById<TextView>( Resource.Id.artist ),
					Year = convertView.FindViewById<TextView>( Resource.Id.year ),
					Genre = convertView.FindViewById<TextView>( Resource.Id.genre ),
				};
			}

			// Display the album. Specify the genre test directly according to the current sort mode
			Album album = Groups[ groupPosition ];
			string genreText = ( SortType != SortType.genre ) ? album.Genre : fastScrollSections[ FastScrollSectionLookup[ groupPosition ] ].Item1;

			( ( AlbumViewHolder )convertView.Tag ).DisplayAlbum( album, genreText );

			return convertView;
		}

		/// <summary>
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) =>
			( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : ( object )Groups[ groupPosition ].Songs[ childPosition ];
	}
}
