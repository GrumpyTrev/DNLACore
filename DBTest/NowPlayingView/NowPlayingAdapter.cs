using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class NowPlayingAdapter: ExpandableListAdapter<PlaylistItem>
	{
		/// <summary>
		/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public NowPlayingAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<PlaylistItem> provider, IActionHandler actionHandler ) :
			base( context, parentView, provider, NowPlayingAdapterModel.BaseModel, actionHandler )
		{
			adapterHandler = actionHandler;
		}

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition )
		{
			return 0;
		}

		/// <summary>
		/// Notification that a particular song is being played.
		/// </summary>
		public void SongBeingPlayed( int index )
		{
			// Highlight the item
			if ( NowPlayingAdapterModel.SongPlayingIndex != index )
			{
				NowPlayingAdapterModel.SongPlayingIndex = index;
				NotifyDataSetChanged();
			}
		}

		/// <summary>
		/// Called when a group item has been clicked
		/// If a group/artist is being expanded then get its contents if not previously displayed
		/// Keep track of which groups have been expanded and the last group expanded
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			// If the adapter is in Action Mode then select this item.
			// Otherwise pass the selection back to the handler
			if ( ActionMode == true )
			{
				OnChildClick( parent, clickedView, groupPosition, 0, 0 );
			}
			else
			{
				// Pass the index back to the handler
				adapterHandler.SongSelected( groupPosition );
			}

			return false;
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
			return null;
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
			View view = convertView;

			if ( view == null )
			{
				view = inflator.Inflate( Resource.Layout.playlistitem_layout, null );
			}

			Song songItem = Groups[ groupPosition ].Song;

			if ( songItem != null )
			{
				if ( view != null )
				{
					// Display the Title and Duration
					view.FindViewById<TextView>( Resource.Id.Title ).Text = songItem.Title;
					view.FindViewById<TextView>( Resource.Id.Duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );

					// If this song is currently being played then show with a different background
					view.SetBackgroundColor( ( NowPlayingAdapterModel.SongPlayingIndex == groupPosition ) ? Color.AliceBlue : Color.Transparent );
				}
			}
			else
			{
			}

			return view;
		}

		/// <summary>
		/// Get the data item at teh specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition )
		{
			return ( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : null;
		}

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// </summary>
		/// <param name="tag"></param>
		protected override bool SelectLongClickedItem( int tag )
		{
			// Always select the clicked item
			return true;
		}

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		private IActionHandler adapterHandler = null;

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		public interface IActionHandler: IAdapterActionHandler
		{
			void SongSelected( int itemNo );
		}
	}

}