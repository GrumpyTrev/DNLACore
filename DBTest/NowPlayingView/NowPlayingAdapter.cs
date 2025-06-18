using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parentView"></param>
	/// <param name="provider"></param>
	internal class NowPlayingAdapter( Context context, ExpandableListView parentView, 
		ExpandableListAdapter<PlaylistItem>.IGroupContentsProvider<PlaylistItem> provider, NowPlayingAdapter.IActionHandler actionHandler ) : 
		ExpandableListAdapter<PlaylistItem>( context, parentView, provider, NowPlayingAdapterModel.BaseModel, actionHandler )
	{

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => 0;

		/// <summary>
		/// There are no child types
		/// </summary>
		public override int ChildTypeCount => 0;

		/// <summary>
		/// This should never be called. Return zero anyway
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override int GetChildType( int groupPosition, int childPosition ) => 0;

		/// <summary>
		/// There is one group type, the songs
		/// </summary>
		public override int GroupTypeCount => 1;

		/// <summary>
		/// As there is only one group Type always return 0
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetGroupType( int groupPosition ) => 0;

		/// <summary>
		/// Notification that a particular song is being played.
		/// Highlight the item by forcing a redraw
		/// This can sometimes be called before the ListView has sorted itself out. So Post the highlighting action on the ListView's queue
		/// </summary>
		public void SongBeingPlayed( int index ) => parentView.Post( () =>
		{
			NowPlayingAdapterModel.SongPlayingIndex = index;

			// If there is no user interaction going on then make sure this item is visible
			if ( IsUserActive == false )
			{
				UserActivityChanged();
			}

			NotifyDataSetChanged();
		} );

		/// <summary>
		/// Called when a group item has been clicked
		/// If ActionMode is in effect then add this item to the collection of selected items
		/// If Action mode is not in effect then treat this as a song selection event. To prevent this being called
		/// erroneously when attempting to scroll the list, only convert this to a song selection event on a double click
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			// If the adapter is in Action Mode then select this item.
			if ( ActionMode == true )
			{
				_ = OnChildClick( parent, clickedView, groupPosition, 0, 0 );
			}
			else
			{
				// Detect a double click
				if ( ( groupPosition == lastClickedItem ) && ( ( DateTime.Now - lastClickTime ).TotalMilliseconds < DoubleClickDurationMilliseconds ) )
				{
					// Pass the index back to the handler
					adapterHandler.SongSelected( groupPosition );
				}
				else
				{
					// Not a completed double-click, treat as the start of a double-click
					lastClickedItem = groupPosition;
					lastClickTime = DateTime.Now;
				}
			}

			// Report this interaction
			UserActivityDetected();

			return false;
		}

		/// <summary>
		/// Called when the order of the songs in the playlist have changed
		/// </summary>
		/// <param name="list"></param>
		public void PlaylistUpdated( List<PlaylistItem> newData )
		{
			Groups = newData;

			// Make sure that this is a reordering, i.e. if action mode is still active.
			if ( ActionMode == true )
			{
				// Only update the selection tags if this playlist still has any children
				if ( Groups.Count > 0 )
				{
					// Form a collection of the playlist items and their tags and use it to update the selection tags
					UpdateSelectionTags( Groups.Select( ( object value, int i ) => (value, FormGroupTag( i )) ) );
				}
			}

			NotifyDataSetChanged();
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
		protected override View GetSpecialisedChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent ) => null;

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
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.nowplaying_song_layout, null );
				convertView.Tag = new SongViewHolder()
				{
					Artist = convertView.FindViewById<TextView>( Resource.Id.artist ),
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration ),
					Animation = convertView.FindViewById<ImageView>( Resource.Id.animation )
				};
			}

			// Display the Title, Duration and Artist
			( ( SongViewHolder )convertView.Tag ).DisplaySong( (SongPlaylistItem) Groups[ groupPosition ] );

			return convertView;
		}

		/// <summary>
		/// Change the view's background if it the song currently being played
		/// </summary>
		protected override void RenderBackground( View convertView )
		{
			SongViewHolder viewHolder = ( SongViewHolder )convertView.Tag;

			// The song index is encoded as a group so..
			int songIndex = GetGroupFromTag( viewHolder.ItemTag );

			// If the item is selected or is not the current song then display it without any highlight.
			// Otehrwise highlight the item to indicate that it is the one being played
			if ( ( IsItemSelected( viewHolder.ItemTag ) == true ) || ( NowPlayingAdapterModel.SongPlayingIndex != songIndex ) )
			{
				viewHolder.UnHighlight();
			}
			else
			{
				viewHolder.Highlight( NowPlayingAdapterModel.IsPlaying );
			}

			base.RenderBackground( convertView );
		}

		/// <summary>
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) => ( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : null;

		/// <summary>
		/// Check if the Current Song (if there is one) is being displayed.
		/// If it isn't then display it
		/// </summary>
		protected override void UserActivityChanged()
		{
			if ( IsUserActive == false )
			{
				if ( NowPlayingAdapterModel.SongPlayingIndex != -1 )
				{
					// If the list hasn't been displayed yet then the LastVisiblePosition is -1
					if ( parentView.LastVisiblePosition != -1 )
					{
						if ( ( NowPlayingAdapterModel.SongPlayingIndex < parentView.FirstVisiblePosition ) ||
							( NowPlayingAdapterModel.SongPlayingIndex > parentView.LastVisiblePosition ) )
						{
							// Attempt to display this somewhere in the middle
							int visibleRange = parentView.LastVisiblePosition - parentView.FirstVisiblePosition;

                            // If this is called when the ListView does not have focus it does not always actually do the scroll.
                            // So get focus first.
                            // Do all this on the ListView's queue
                            parentView.ClearFocus();
							_ = parentView.Post( () =>
							{
								_ = parentView.RequestFocusFromTouch();
								parentView.SetSelection( Math.Max( NowPlayingAdapterModel.SongPlayingIndex - ( visibleRange / 2 ), 0 ) );
								_ = parentView.RequestFocus();
							} );

						}
					}
					else
					{
						// Display the current song just a couple of items down (if possible) to indicate that it is not the first item
						parentView.SetSelection( Math.Max( NowPlayingAdapterModel.SongPlayingIndex - 2, 0 ) );
					}
				}
			}
		}

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		private readonly IActionHandler adapterHandler = actionHandler;

		/// <summary>
		/// The time when the last item was clicked
		/// </summary>
		private DateTime lastClickTime = DateTime.MinValue;

		/// <summary>
		/// The identity of the last item clicked
		/// </summary>
		private int lastClickedItem = -1;

		/// <summary>
		/// The time two click events on the same item for them to be treated as a double-click event
		/// </summary>
		private const int DoubleClickDurationMilliseconds = 500;

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		public interface IActionHandler: IAdapterEventHandler
		{
			void SongSelected( int itemNo );
		}

		/// <summary>
		/// View holder for the playlist Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder
		{
			public void DisplaySong( SongPlaylistItem playlistItem )
			{
				Title.Text = playlistItem.Song.Title;
				Duration.Text = TimeSpan.FromSeconds( playlistItem.Song.Length ).ToString( @"mm\:ss" );
				Artist.Text = string.Format( "{0} : {1}", playlistItem.Artist.Name, playlistItem.Song.Album.Name );
			}

			/// <summary>
			/// Highlight this item by showing the animation
			/// </summary>
			/// <param name="isPlaying"></param>
			public void Highlight( bool isPlaying)
			{
				// Show the animation image. Only animate it if the the song is being played
				Animation.Visibility = ViewStates.Visible;

				AnimationDrawable animationFrame = ( AnimationDrawable )Animation.Drawable;
				if ( isPlaying == true )
				{
					// Set the animation going if not already running
					if ( animationFrame.IsRunning == false )
					{
						animationFrame.Start();
					}
				}
				else
				{
					animationFrame.Stop();
				}

				// Show the title in large text
				Title.TextSize = Title.Context.Resources.GetDimensionPixelOffset( Resource.Dimension.text_size_heading ) / Title.Context.Resources.DisplayMetrics.Density;
			}

			/// <summary>
			/// Unhightlight this item by hiding the animation and displaying the song title in normal size texst
			/// </summary>
			public void UnHighlight()
			{
				// Alway hide the animation image. Stop the animation if it is running
				Animation.Visibility = ViewStates.Gone;

				AnimationDrawable animationFrame = ( AnimationDrawable )Animation.Drawable;
				if ( animationFrame.IsRunning == true )
				{
					animationFrame.Stop();
				}

				// Show the song title in normal size text
				Title.TextSize = Title.Context.Resources.GetDimensionPixelOffset( Resource.Dimension.text_size_normal ) / Title.Context.Resources.DisplayMetrics.Density;
			}

			public TextView Artist { get; set; }
			public TextView Title { get; set; }
			public TextView Duration { get; set; }
			public ImageView Animation { get; set; }
		}
	}
}
