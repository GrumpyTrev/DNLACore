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
		public override int GetChildrenCount( int groupPosition )
		{
			int childCount = 0;
			Playlist playList = Groups[ groupPosition ];
			if ( playList.PlaylistItems != null )
			{
				childCount = Groups[ groupPosition ].PlaylistItems.Count;
			}

			return childCount;
		}

		/// <summary>
		/// Called when songs have been added to or deleted from a playlist
		/// Only process this if the playlist is being displayed.
		/// If the playlist contents are being shown then notify the base class 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="songs"></param>
		public void PlaylistUpdated( string playlistName )
		{
			// Find the group holding the playlist
			int groupPosition = Groups.FindIndex( p => p.Name == playlistName );

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
			// If the supplied view previously contained a Playlist heading then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.title ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unuasable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlistitem_layout, null );
			}

			PlaylistItem item = Groups[ groupPosition ].PlaylistItems[ childPosition ];

			// Display the Title, Duration and Artist
			convertView.FindViewById<TextView>( Resource.Id.title ).Text = item.Song.Title;
			convertView.FindViewById<TextView>( Resource.Id.duration ).Text = TimeSpan.FromSeconds( item.Song.Length ).ToString( @"mm\:ss" );
			convertView.FindViewById<TextView>( Resource.Id.artist ).Text = item.Artist.Name;

			return convertView;
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
			// If the supplied view previously contained other than a PLaylist then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.playListName ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unusable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlist_layout, null );
			}

			// Display the artist's name
			convertView.FindViewById<TextView>( Resource.Id.playListName ).Text = Groups[ groupPosition ].Name;

			return convertView;
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