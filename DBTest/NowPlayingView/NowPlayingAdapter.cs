using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	internal class NowPlayingAdapter : ExpandableListAdapter<PlaylistItem>, DragHelper.IAdapterInterface
	{
		/// <summary>
		/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public NowPlayingAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<PlaylistItem> provider,
			IActionHandler actionHandler )
			: base( context, parentView, provider, NowPlayingAdapterModel.BaseModel, actionHandler ) => adapterHandler = actionHandler;

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

			NotifyDataSetChanged();
		}

		// DragHelper.IAdapterInterface methods

		/// <summary>
		/// Save the DragHelper instance
		/// </summary>
		/// <param name="helperToBind"></param>
		public void BindDragHelper( DragHelper helperToBind ) => dragHelper = helperToBind;

		/// <summary>
		/// Keep the interaction timer going when dragging is in effect
		/// </summary>
		public void UserInteraction() => UserActivityDetected();

		/// <summary>
		/// Allow the DragHelper access to the ActionMode
		/// </summary>
		public bool ActionModeInEffect => ActionMode;

		/// <summary>
		/// Pass on a DragHelper redraw request
		/// </summary>
		public void RedrawRequired() => NotifyDataSetChanged();

		/// <summary>
		/// Called by the DragHelper to actually carry out a move
		/// </summary>
		/// <param name="itemPosition"></param>
		/// <param name="moveUp"></param>
		public void MoveItem( long itemPosition, bool moveUp )
		{
			if ( moveUp == true )
			{
				adapterHandler.MoveSongUp( Groups[ ExpandableListView.GetPackedPositionGroup( itemPosition )] );
			}
			else
			{
				adapterHandler.MoveSongDown( Groups[ ExpandableListView.GetPackedPositionGroup( itemPosition ) ] );
			}
		}

		/// <summary>
		/// Return the drag feedback view and item position limits
		/// </summary>
		/// <param name="minDrag"></param>
		/// <param name="maxDrag"></param>
		/// <param name="minIndex"></param>
		/// <param name="maxIndex"></param>
		/// <param name="dragItemPosition"></param>
		public void GetLimits( out int minDrag, out int maxDrag, out long minIndex, out long maxIndex, long dragItemPosition )
		{
			// The DragShadow can be shown anywhere within the parent view
			minDrag = 0;
			maxDrag = parentView.Height - 1;

			// The item position can be any of the group value
			minIndex = ExpandableListView.GetPackedPositionForGroup( 0 );
			maxIndex = ExpandableListView.GetPackedPositionForGroup( Groups.Count - 1 );
		}

		/// <summary>
		/// Provide a view for a child at the specified position
		/// The Now Playing view contains Group items only
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="isLastChild"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent ) => null;

		/// <summary>
		/// Provide a view for the specified group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			SongViewHolder viewHolder = null;

			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.nowplaying_song_layout, null );

				// Create a SongViewHolder to hold the controls required to render this song
				viewHolder = new()
				{
					Artist = convertView.FindViewById<TextView>( Resource.Id.artist ),
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration ),
					Animation = convertView.FindViewById<ImageView>( Resource.Id.animation ),
					DragHandle = convertView.FindViewById<ImageView>( Resource.Id.dragHandle ),
				};

				// Save the song holder in the view so it can be accessed via the view
				convertView.Tag = viewHolder;

				// Attach a handler to the song view's drag handler, and link the drag handle back to the parent view
				viewHolder.DragHandle.Touch += dragHelper.DragHandleTouch;
				viewHolder.DragHandle.Tag = convertView;
			}

			// Display the Title, Duration and Artist
			viewHolder = ( SongViewHolder )convertView.Tag;
			viewHolder.DisplaySong( (SongPlaylistItem) Groups[ groupPosition ] );

			// Keep track of which item the SongViewHolder is displaying
			viewHolder.ItemPosition = ExpandableListView.GetPackedPositionForGroup( groupPosition );

			// If this item is being dragged then hide it and record it's position.
			// Otherwise make sure it is visible
			dragHelper.HideViewIfBeingDragged( convertView );

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
			// Otherwise highlight the item to indicate that it is the one being played
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

							_ = parentView.Post( () => parentView.SetSelection( Math.Max( NowPlayingAdapterModel.SongPlayingIndex - ( visibleRange / 2 ), 0 ) ) );
						}
					}
					else
					{
						// Display the current song just a couple of items down (if possible) to indicate that it is not the first item
						_ = parentView.Post( () => parentView.SetSelection( Math.Max( NowPlayingAdapterModel.SongPlayingIndex - 2, 0 ) ) );
					}
				}
			}
		}

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		private readonly IActionHandler adapterHandler = null;

		/// <summary>
		/// The DragHelper instance provided by the frgament
		/// </summary>
		private DragHelper dragHelper = null;

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

			void MoveSongUp( PlaylistItem item );

			void MoveSongDown( PlaylistItem item );
		}

		/// <summary>
		/// View holder for the playlist Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder, DragHelper.IDragHolder
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
			public ImageView DragHandle { get; set; }

			public long ItemPosition { get; set; }
		}
	}
}
