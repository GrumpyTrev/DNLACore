using Android.Widget;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class NowPlayingFragment: PagedFragment< PlaylistItem >, ExpandableListAdapter<PlaylistItem>.IGroupContentsProvider<PlaylistItem>, 
		NowPlayingController.INowPlayingReporter, NowPlayingAdapter.IActionHandler
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public NowPlayingFragment() => ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Get all the PlaylistItem entries associated with the Now Playing playlist.
		/// No group content required. Just run an empty task to prevent compiler warnings
		/// </summary>
		/// <param name="thePlayList"></param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task ProvideGroupContentsAsync( PlaylistItem _ )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
		}

		/// <summary>
		/// Called when the Now Playing playlist has been read or updated
		/// Display the data held in the Now Playing view model
		/// </summary>
		/// <param name="message"></param>
		public void DataAvailable()
		{
			Adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.ToList(), SortSelector.SortType.alphabetic );

			( ( NowPlayingAdapter )Adapter ).SongBeingPlayed( NowPlayingViewModel.SelectedSong );
		}

		/// <summary>
		/// Called when a the Now Playing playlist has been updated
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistUpdated() => ( ( NowPlayingAdapter )Adapter ).PlaylistUpdated( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.ToList() );

		/// <summary>
		/// Called when a song has been selected by the user
		/// Pass this change to the controller
		/// </summary>
		/// <param name="itemNo"></param>
		public void SongSelected( int itemNo ) => NowPlayingController.SetSelectedSong( itemNo );

		/// <summary>
		/// Called when song selection has been reported by the controller
		/// Pass on the changes to the adapter
		/// </summary>
		public void SongSelected() => ( ( NowPlayingAdapter )Adapter ).SongBeingPlayed( NowPlayingViewModel.SelectedSong );

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects ) => 
			ActionModeTitle = ( selectedObjects.PlaylistItemsCount == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, selectedObjects.PlaylistItemsCount );

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new NowPlayingAdapter( Context, listView, this, this );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Initialise the NowPlayingController
		/// </summary>
		protected override void PostViewCreateAction() => NowPlayingController.DataReporter = this;

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => NowPlayingController.DataReporter = null;

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
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}