using System;
using System.Collections.Generic;
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
		public PlaylistsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider< Playlist > provider ) :
			base( context, parentView, provider, StateModelProvider.Get( typeof( PlaylistsAdapterModel ) ) as PlaylistsAdapterModel )
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
		/// Called when songs have been added to a playlist
		/// Only process this if the playlist is being displayed.
		/// If the playlist contents are being shown then notify the base class 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="songs"></param>
		public void SongsAdded( Playlist list, List<Song> songs )
		{
			// Find the group holding the playlist
			int groupPosition = Groups.IndexOf( list );

			if ( groupPosition != -1 )
			{
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
			// If the supplied view previously contained an ArtistAlbum then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.Title ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unuasable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlistitem_layout, null );
			}

			Song songItem = Groups[ groupPosition ].PlaylistItems[ childPosition ].Song;

			// Display the Title and Duration
			convertView.FindViewById<TextView>( Resource.Id.Title ).Text = songItem.Title;
			convertView.FindViewById<TextView>( Resource.Id.Duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );

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
			// If the supplied view previously contained other than an Artits then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.PlayListName ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unusable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlist_layout, null );
			}

			// Display the artist's name
			convertView.FindViewById<TextView>( Resource.Id.PlayListName ).Text = Groups[ groupPosition ].Name;

			return convertView;
		}
	}
}