using System;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class PlaylistsAdapter: ExpandableListAdapter< Playlist >
	{
		/// <summary>
		/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public PlaylistsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider< Playlist > provider, IAdapterActionHandler actionHandler ) :
			base( context, parentView, provider,  PlaylistsAdapterModel.BaseModel, actionHandler )
		{
		}

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => Groups[ groupPosition ].PlaylistItems?.Count ?? 0;

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
		/// There is one group type, the playlist
		/// </summary>
		public override int GroupTypeCount => 1;

		/// <summary>
		/// As there is only one group Type always return 0
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetGroupType( int groupPosition ) => 0;

		/// <summary>
		/// Called when songs have been added to or deleted from a playlist
		/// Only process this if the playlist is being displayed.
		/// If the playlist contents are being shown then notify the base class 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="songs"></param>
		public void PlaylistUpdated( Playlist playlist )
		{
			// Find the group holding the playlist
			int groupPosition = Groups.IndexOf( playlist );

			if ( groupPosition != -1 )
			{
				// If action mode is still active then this is most likely being called due to item repositioning.
				if ( ActionMode == true )
				{
					// Form a collection of the playlist items and their tags and use it to update the selection tags
					UpdateSelectionTags( Groups[ groupPosition ].PlaylistItems.Select( ( object value, int i ) => (value, FormChildTag( groupPosition, i )) ) );
				}

				// Is this group expanded
				if ( adapterModel.ExpandedGroups.Contains( groupPosition ) == true )
				{
					NotifyDataSetChanged();
				}
			}
		}

		/// <summary>
		/// Provide a view containing either album or song details at the specified position
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
			// If no view supplied then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlistitem_layout, null );
				convertView.Tag = new SongViewHolder()
				{
					SelectionBox = GetSelectionBox( convertView ),
					Artist = convertView.FindViewById<TextView>( Resource.Id.artist ),
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration )
				};
			}

			// Display the Title, Duration and Artist
			( ( SongViewHolder )convertView.Tag ).DisplaySong( Groups[ groupPosition ].PlaylistItems[ childPosition ] );

			return convertView;
		}

		/// <summary>
		/// View holder for the child Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder
		{
			public void DisplaySong( PlaylistItem playlistItem )
			{
				Title.Text = playlistItem.Song.Title;
				Duration.Text = TimeSpan.FromSeconds( playlistItem.Song.Length ).ToString( @"mm\:ss" );
				Artist.Text = playlistItem.Artist.Name;
			}

			public TextView Artist { get; set; }
			public TextView Title { get; set; }
			public TextView Duration { get; set; }
		}

		/// <summary>
		/// Derived classes must implement this method to provide a view for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			// If no view supplied then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlist_layout, null );
				convertView.Tag = new PlaylistViewHolder()
				{
					SelectionBox = GetSelectionBox( convertView ),
					Name = convertView.FindViewById<TextView>( Resource.Id.playListName )
				};
			}

			// Display the playlist's name
			( ( PlaylistViewHolder )convertView.Tag ).DisplayPlaylist( Groups[ groupPosition ] );

			return convertView;
		}

		/// <summary>
		/// View holder for the child Song items
		/// </summary>
		private class PlaylistViewHolder : ExpandableListViewHolder
		{
			public void DisplayPlaylist( Playlist playlist ) => Name.Text = playlist.Name;

			public TextView Name { get; set; }
		}

		/// <summary>
		/// Get the data item at teh specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) => 
			( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : ( object )Groups[ groupPosition ].PlaylistItems[ childPosition ];

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// All items should be selected
		/// </summary>
		/// <param name="tag"></param>
		protected override bool SelectLongClickedItem( int tag ) => true;
	}
}