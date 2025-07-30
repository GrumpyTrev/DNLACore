using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CoreMP;
using static Android.Widget.ExpandableListView;

namespace DBTest
{
	/// <summary>
	/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parentView"></param>
	/// <param name="provider"></param>
	internal class PlaylistsAdapter : ExpandableListAdapter<Playlist>, DragHelper.IAdapterInterface
	{
		public PlaylistsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<Playlist> provider, IActionHandler actionHandler ) :
			base( context, parentView, provider, PlaylistsAdapterModel.BaseModel, actionHandler ) => adapterHandler = actionHandler;

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => Groups[ groupPosition ].PlaylistItems.Count;

		/// <summary>
		/// There are two child types. The SongPlaylistItem and AlbumPlaylistItem
		/// </summary>
		public override int ChildTypeCount => 2;

		/// <summary>
		/// The child type depends on the group type.
		/// For SongPlaylists the child type is 0. For AlbumPlayLists the child type is 1
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override int GetChildType( int groupPosition, int childPosition ) => Groups[ groupPosition ] is SongPlaylist ? 0 : 1;

		/// <summary>
		/// There is only a single group type IPlaylist
		/// </summary>
		public override int GroupTypeCount => 1;

		/// <summary>
		/// Return 0 as only 1 group type
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetGroupType( int groupPosition ) => 0;

		/// <summary>
		/// Called when a child item has been clicked.
		/// If the item being clicked is an AlbumPlaylistItem then display a popup containing the songs associated with the Album
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool OnChildClick( ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id )
		{
			bool handled;
			if ( ( ActionMode == false ) && ( Groups[ groupPosition ] is AlbumPlaylist playlist ) )
			{
				( stateChangeReporter as IActionHandler ).AlbumPlaylistItemClicked( playlist, playlist.PlaylistItems[ childPosition ] as AlbumPlaylistItem );

				handled = true;
			}
			else
			{
				handled = base.OnChildClick( parent, clickedView, groupPosition, childPosition, id );
			}

			return handled;
		}

		/// <summary>
		/// Called when songs have been added to or deleted from a playlist
		/// Only process this if the playlist is being displayed.
		/// If the playlist contents are being shown then notify the base class 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="songs"></param>
		public void PlaylistUpdated( Playlist updatedObject )
		{
			// Find the group holding the playlist
			int groupPosition = Groups.IndexOf( updatedObject );

			if ( groupPosition != -1 )
			{
				// Is this group expanded
				if ( adapterModel.LastGroupOpened == groupPosition )
				{
					NotifyDataSetChanged();
				}
			}
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
			Playlist thePlaylist = Groups[ GetPackedPositionGroup( itemPosition ) ];

			if ( moveUp == true )
			{
				adapterHandler.MoveSongUp( thePlaylist, thePlaylist.PlaylistItems[ GetPackedPositionChild( itemPosition ) ] );
			}
			else
			{
				adapterHandler.MoveSongDown( thePlaylist, thePlaylist.PlaylistItems[ GetPackedPositionChild( itemPosition ) ] );
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
			int group = GetPackedPositionGroup( dragItemPosition );

			// The item position can be any of the group's children.
			minIndex = GetPackedPositionForChild( group, 0 );
			maxIndex = GetPackedPositionForChild( group, Groups[ group ].PlaylistItems.Count - 1 );

			// The DragShadow can only be shown where the children of the group containing the dragged item are displayed
			int firstChildFlatPosition = parentView.GetFlatListPosition( minIndex );
			int lastChildFlatPosition = parentView.GetFlatListPosition( maxIndex );

			// Is the first child of the group visible
			// Assume not visible
			minDrag = 0;

			if ( firstChildFlatPosition >= parentView.FirstVisiblePosition )
			{
				// The first child is visible. Get its view
				View firstChildView = parentView.GetChildAt( firstChildFlatPosition - parentView.FirstVisiblePosition );
				minDrag = ( firstChildView != null ) ? ( int )firstChildView.GetY() : 0;
			}

			// Is the last child of the group visible
			// Assume not visible
			maxDrag = parentView.Height;

			if ( lastChildFlatPosition <= parentView.LastVisiblePosition )
			{
				// The last child is visible. Get its view
				View lastChildView = parentView.GetChildAt( lastChildFlatPosition - parentView.FirstVisiblePosition );
				maxDrag = ( lastChildView != null ) ? ( int )lastChildView.GetY() + lastChildView.Height : parentView.Height;
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
			if ( Groups[ groupPosition ] is SongPlaylist playlist )
			{
				SongViewHolder viewHolder;

				// If no view supplied then create a new one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.playlists_song_layout, null );

					// Create a SongViewHolder to hold the controls required to render this song
					viewHolder = new SongViewHolder()
					{
						Artist = convertView.FindViewById<TextView>( Resource.Id.artist ),
						Title = convertView.FindViewById<TextView>( Resource.Id.title ),
						Duration = convertView.FindViewById<TextView>( Resource.Id.duration ),
						DragHandle = convertView.FindViewById<ImageView>( Resource.Id.dragHandle )
					};

					// Save the song holder in the view so it can be accessed via the view
					convertView.Tag = viewHolder;

					// Attach a handler to the song view's drag handler, and link the drag handle back to the parent view
					viewHolder.DragHandle.Touch += dragHelper.DragHandleTouch;
					viewHolder.DragHandle.Tag = convertView;
				}

				// Display the SongPlaylistItem
				viewHolder = ( SongViewHolder )convertView.Tag;
				viewHolder.DisplaySong( ( SongPlaylistItem )playlist.PlaylistItems[ childPosition ] );

				// Keep track of which item the SongViewHolder is displaying
				viewHolder.ItemPosition = ExpandableListView.GetPackedPositionForChild( groupPosition, childPosition );

				// If this song is currently being played then show with a different background
				convertView.SetBackgroundColor( ( playlist.SongIndex == childPosition ) ? Color.AliceBlue : Color.Transparent );

				// If this item is being dragged then hide it and record it's position.
				// Otherwise make sure it is visible
				dragHelper.HideViewIfBeingDragged( convertView );
			}
			else
			{
				AlbumViewHolder viewHolder = null;

				// If the supplied view is null then create one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.playlists_album_layout, null );

					// Create a AlbumViewHolder to hold the controls required to render this song
					viewHolder = new AlbumViewHolder()
					{
						AlbumName = convertView.FindViewById<TextView>( Resource.Id.albumName ),
						ArtistName = convertView.FindViewById<TextView>( Resource.Id.artist ),
						Year = convertView.FindViewById<TextView>( Resource.Id.year ),
						Genre = convertView.FindViewById<TextView>( Resource.Id.genre ),
						DragHandle = convertView.FindViewById<ImageView>( Resource.Id.dragHandle )
					};

					// Save the album holder in the view so it can be accessed via the view
					convertView.Tag = viewHolder;

					// Attach a handler to the album view's drag handler, and link the drag handle back to the parent view
					viewHolder.DragHandle.Touch += dragHelper.DragHandleTouch;
					viewHolder.DragHandle.Tag = convertView;
				}

				// Display the album
				viewHolder = ( AlbumViewHolder )convertView.Tag;
				AlbumPlaylist albumPlaylist = ( AlbumPlaylist )Groups[ groupPosition ];
				Album album = ( ( AlbumPlaylistItem )albumPlaylist.PlaylistItems[ childPosition ] ).Album;
				viewHolder.DisplayAlbum( album, album.Genre );

				// Keep track of which item the AlbumViewHolder is displaying
				viewHolder.ItemPosition = ExpandableListView.GetPackedPositionForChild( groupPosition, childPosition );

				// If this item is being dragged then hide it and record it's position.
				// Otherwise make sure it is visible
				dragHelper.HideViewIfBeingDragged( convertView );
			}

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
			// If no view supplied then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.playlists_playlist_layout, null );
				convertView.Tag = new PlaylistViewHolder()
				{
					Name = convertView.FindViewById<TextView>( Resource.Id.playListName )
				};
			}

			// Display the playlist's name
			( ( PlaylistViewHolder )convertView.Tag ).DisplayPlaylist( Groups[ groupPosition ] );

			return convertView;
		}

		/// <summary>
		/// Change the view's background if it is the album that is currently being played
		/// </summary>
		protected override void RenderBackground( View convertView )
		{
			if ( convertView.Tag is AlbumViewHolder albumView )
			{
				AlbumPlaylist albumPlaylist = ( AlbumPlaylist )Groups[ GetGroupFromTag( albumView.ItemTag ) ];
				Album album = ( ( AlbumPlaylistItem )albumPlaylist.PlaylistItems[ GetChildFromTag( albumView.ItemTag ) ] ).Album;
				if ( albumPlaylist.InProgressAlbum == album )
				{
					convertView.SetBackgroundColor( Color.AliceBlue );
				}
				else
				{
					base.RenderBackground( convertView );
				}
			}
			else
			{
				base.RenderBackground( convertView );
			}
		}

		/// <summary>
		/// View holder for the group SongPlaylist items
		/// </summary>
		private class PlaylistViewHolder : ExpandableListViewHolder
		{
			public void DisplayPlaylist( Playlist playlist ) => Name.Text = string.Format( "[{0}] {1}", ( playlist is SongPlaylist ) ? "S" : "A", playlist.Name );

			public TextView Name { get; set; }
		}

		/// <summary>
		/// View holder for the group Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder, DragHelper.IDragHolder
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
			public ImageView DragHandle { get; set; }
			public long ItemPosition { get; set; }
		}

		/// <summary>
		/// Called when a group has been selected or deselected to allow derived classes to perform their own processing
		/// If this is a Tag and is being selected then make sure that all the TaggedAlbum entries have their Song entries set
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		protected override bool GroupSelectionHasChanged( int groupPosition, bool selected )
		{
			if ( selected == true )
			{
				contentsProvider.ProvideGroupContents( Groups[ groupPosition ] );
			}

			return false;
		}

		/// <summary>
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) =>
			( childPosition == 0XFFFF ) ? ( object )Groups[ groupPosition ] : ( ( Playlist )Groups[ groupPosition ] ).PlaylistItems[ childPosition ];

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		private readonly IActionHandler adapterHandler = null;

		/// <summary>
		/// The DragHelper instance provided by the frgament
		/// </summary>
		private DragHelper dragHelper = null;

		/// <summary>
		/// Interface used to handler adapter request and state changes
		/// </summary>
		public interface IActionHandler : IAdapterEventHandler
		{
			void AlbumPlaylistItemClicked( AlbumPlaylist albumPlaylist, AlbumPlaylistItem albumPlaylistItem );

			void MoveSongUp( Playlist thePlaylist, PlaylistItem item );

			void MoveSongDown( Playlist thePlaylist, PlaylistItem item );
		}
	}
}
