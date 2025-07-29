using Android.Widget;
using CoreMP;

namespace DBTest
{
	public class NowPlayingFragment : PagedFragment<PlaylistItem>, ExpandableListAdapter<PlaylistItem>.IGroupContentsProvider<PlaylistItem>,
		NowPlayingAdapter.IActionHandler
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public NowPlayingFragment() => ActionMode.ActionModeTitle = string.Empty;

		/// <summary>
		/// Get all the SongPlaylistItem entries associated with the Now Playing playlist.
		/// No group content required. Just run an empty task to prevent compiler warnings
		/// </summary>
		/// <param name="thePlayList"></param>
		public void ProvideGroupContents( PlaylistItem _ ) {}

		/// <summary>
		/// Called when the Now Playing playlist has been read or updated
		/// Display the data held in the Now Playing view model
		/// </summary>
		/// <param name="message"></param>
		public override void DataAvailable()
		{
			Adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems, SortType.alphabetic );

			( ( NowPlayingAdapter )Adapter ).SongBeingPlayed( NowPlayingViewModel.CurrentSongIndex );

			base.DataAvailable();
		}

		/// <summary>
		/// Called when a song has been selected by the user
		/// Pass this change to the controller
		/// </summary>
		/// <param name="itemNo"></param>
		public void SongSelected( int itemNo ) => MainApp.CommandInterface.UserSongSelected( itemNo );

		public void MoveSongUp( PlaylistItem item ) => MainApp.CommandInterface.MoveItemsUp( [ item ] );

		public void MoveSongDown( PlaylistItem item ) => MainApp.CommandInterface.MoveItemsDown( [ item ] );

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView )
		{
			// Create the adapter
			Adapter = new NowPlayingAdapter( Context, listView, this, this );

			// Create a DragHelper for the main drag functionality and bind it to the adapter
			_ = new DragHelper( listView, FragmentView, (DragHelper.IAdapterInterface)Adapter );
		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Register for data model changes
		/// </summary>
		protected override void PostViewCreateAction()
		{
			NotificationHandler.Register<NowPlayingViewModel>( nameof(ModelAvailable.IsSet), DataAvailable );
			NotificationHandler.Register<NowPlayingViewModel>( nameof( NowPlayingViewModel.CurrentSongIndex ),
				() => ( ( NowPlayingAdapter )Adapter ).SongBeingPlayed( NowPlayingViewModel.CurrentSongIndex ) );
			NotificationHandler.Register<NowPlayingViewModel>( nameof( NowPlayingViewModel.PlaylistUpdated ),
				() => ( ( NowPlayingAdapter )Adapter ).PlaylistUpdated( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems ) );
			NotificationHandler.Register<NowPlayingViewModel>( nameof( NowPlayingViewModel.IsPlaying ), () =>
			{
				NowPlayingAdapterModel.IsPlaying = NowPlayingViewModel.IsPlaying;
				( ( NowPlayingAdapter )Adapter ).NotifyDataSetChanged();
			} );
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
		protected override int Layout { get; } = Resource.Layout.nowplaying_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.nowplayingList;

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected override int Menu { get; } = Resource.Menu.menu_nowplaying;

		/// <summary>
		/// The menu resource for this fragment's action bar
		/// /// </summary>
		protected override int ActionMenu { get; } = Resource.Menu.menu_nowplaying_action;

		/// <summary>
		/// The layout resource to be used for this fragment's MediaControlsView
		/// </summary>
		protected override int MediaControlsLayout { get; } = Resource.Id.media_controller_playing_layout;
	}
}
