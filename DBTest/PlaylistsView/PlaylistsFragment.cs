using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	public class PlaylistsFragment: PagedFragment<Playlist>, ExpandableListAdapter<Playlist>.IGroupContentsProvider<Playlist>, PlaylistsAdapter.IActionHandler
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public PlaylistsFragment() { }

		/// <summary>
		/// Get all the entries associated with a specified SongPlaylist or AlbumPlaylist.
		/// Both SongPlaylist and AlbumPlaylist items are now read at startup. So this is no longer required for them
		/// </summary>
		/// <param name="selectedGroup_"></param>
		public void ProvideGroupContents( Playlist _ ) {}

		/// <summary>
		/// Called when the PlaylistsController has obtained or updated the playlists held in the model is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		public override void DataAvailable()
		{
			Adapter.SetData( PlaylistsViewModel.Playlists, SortType.alphabetic );
			base.DataAvailable();
		}

		/// <summary>
		/// Called when a specific playlist has been updated
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistUpdated( Playlist playlist ) => ( ( PlaylistsAdapter )Adapter ).PlaylistUpdated( playlist );

		/// <summary>
		/// Called when an AlbumPlaylistItem has been clicked
		/// </summary>
		/// <param name="albumPlaylist"></param>
		/// <param name="albumPlaylistItem"></param>
		public void AlbumPlaylistItemClicked( AlbumPlaylist albumPlaylist, AlbumPlaylistItem albumPlaylistItem )
		{
			AlertDialog dialogue = null;

			// Create the listview and its adapter
			View customView = LayoutInflater.FromContext( Context ).Inflate( Resource.Layout.album_songs_popup, null );
			ListView songView = customView.FindViewById<ListView>( Resource.Id.songsList );

			songView.Adapter = new SongsDisplayAdapter( Context, songView, albumPlaylistItem.Album.Songs, albumPlaylist.InProgressSong?.Id ?? -1,
				clickAction: () => dialogue.Dismiss() );

			// Create and show the dialogue
			dialogue = new AlertDialog.Builder( Context )
				.SetView( customView )
				.Create();

			dialogue.Show();
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new PlaylistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Register for data model changes
		/// </summary>
		protected override void PostViewCreateAction()
		{
			NotificationHandler.Register<PlaylistsViewModel>( DataAvailable );
			NotificationHandler.Register<PlaylistsViewModel>( nameof( PlaylistsViewModel.PlaylistUpdated ), 
				( sender ) => ( ( PlaylistsAdapter )Adapter ).PlaylistUpdated( ( Playlist )sender ) );
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => NotificationHandler.Deregister();

		/// <summary>
		/// Display the number of playlist items selected
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override int SelectedItemCount( GroupedSelection selectedObjects ) => selectedObjects.PlaylistItems.Count;

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.playlists_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.playlistsList;

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected override int Menu { get; } = Resource.Menu.menu_playlists;

		/// <summary>
		/// The menu resource for this fragment's action bar
		/// /// </summary>
		protected override int ActionMenu { get; } = Resource.Menu.menu_playlists_action;
	}
}
