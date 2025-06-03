using System.Linq;
using Android.Content;
using Android.Graphics;
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
	internal class PlaylistsAdapter( Context context, ExpandableListView parentView, 
		ExpandableListAdapter<Playlist>.IGroupContentsProvider<Playlist> provider, PlaylistsAdapter.IActionHandler actionHandler ) : 
		ExpandableListAdapter< Playlist >( context, parentView, provider,  PlaylistsAdapterModel.BaseModel, actionHandler )
	{

		/// <summary>
		/// Number of child items of selected group
		/// If the group is a SongPlaylist then return the number of SongPlaylistItem entries.
		/// If the group is a Tag then return the number of TaggedAlbum entries
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
				// If action mode is still active then this is most likely being called due to item repositioning.
				if ( ActionMode == true )
				{
					// Only update the selection tags if this playlist still has any children
					if ( updatedObject.PlaylistItems.Count > 0 )
					{
						// Form a collection of the playlist items and their tags and use it to update the selection tags
						UpdateSelectionTags( updatedObject.PlaylistItems.Select( ( object value, int i ) => (value, FormChildTag( groupPosition, i )) ) );
					}
				}

				// Is this group expanded
				if ( adapterModel.LastGroupOpened == groupPosition )
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
			if ( Groups[ groupPosition ] is SongPlaylist playlist )
			{
				// If no view supplied then create a new one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.playlists_song_layout, null );
					convertView.Tag = new SongViewHolder()
					{
						Artist = convertView.FindViewById<TextView>( Resource.Id.artist ),
						Title = convertView.FindViewById<TextView>( Resource.Id.title ),
						Duration = convertView.FindViewById<TextView>( Resource.Id.duration )
					};
				}

				// Display the SongPlaylistItem
				( ( SongViewHolder )convertView.Tag ).DisplaySong( ( SongPlaylistItem )playlist.PlaylistItems[ childPosition ] );

				// If this song is currently being played then show with a different background
				convertView.SetBackgroundColor( ( playlist.SongIndex == childPosition ) ? Color.AliceBlue : Color.Transparent );
			}
			else
			{
				// If the supplied view is null then create one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.playlists_album_layout, null );
					convertView.Tag = new AlbumViewHolder()
					{
						AlbumName = convertView.FindViewById<TextView>( Resource.Id.albumName ),
						ArtistName = convertView.FindViewById<TextView>( Resource.Id.artist ),
						Year = convertView.FindViewById<TextView>( Resource.Id.year ),
						Genre = convertView.FindViewById<TextView>( Resource.Id.genre ),
					};
				}

				// Display the album
				AlbumPlaylist albumPlaylist = ( AlbumPlaylist )Groups[ groupPosition ];
				Album album = ( (AlbumPlaylistItem)albumPlaylist.PlaylistItems[ childPosition ] ).Album;
				( ( AlbumViewHolder )convertView.Tag ).DisplayAlbum( album, ActionMode, album.Genre );
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
				AlbumPlaylist albumPlaylist = ( AlbumPlaylist )Groups[ GetGroupFromTag( albumView.ItemTag) ];
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
		public interface IActionHandler : IAdapterEventHandler
		{
			void AlbumPlaylistItemClicked( AlbumPlaylist albumPlaylist, AlbumPlaylistItem albumPlaylistItem );
		}
	}
}
