using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class NowPlayingFragment: PagedFragment< PlaylistItem >, ExpandableListAdapter<PlaylistItem>.IGroupContentsProvider<PlaylistItem>, 
		NowPlayingController.IReporter, NowPlayingAdapter.IActionHandler
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public NowPlayingFragment() => ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_nowplaying, menu );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Get all the PlaylistItem entries associated with the Now Playing playlist.
		/// No group content required. Just run an empty task to prevent compiler warnings
		/// </summary>
		/// <param name="thePlayList"></param>
		public async Task ProvideGroupContentsAsync( PlaylistItem theItem ) => await Task.Run( () => { } );

		/// <summary>
		/// Called when the Now Playing playlist has been read or updated
		/// Display the data held in the Now Playing view model
		/// </summary>
		/// <param name="message"></param>
		public void NowPlayingDataAvailable()
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
		public override void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			// Determine the number of songs in the selected items
			IEnumerable<PlaylistItem> itemSelected = selectedItems.Values.OfType< PlaylistItem>();
			int itemsSelectedCount = itemSelected.Count();

			// Update the Action Mode bar title
			ActionModeTitle = ( itemsSelectedCount == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, itemsSelectedCount );

			// The delete command is enabled when one or more items are selected
			deleteCommand.Visible = ( itemsSelectedCount > 0 );

			// The move_up command is enabled if one or more items are selected and the first item is not selected
			// The move_down command is enabled if one or more items are selected and the last item is not selected
			moveUpCommand.Visible = ( itemsSelectedCount > 0 ) && 
				( itemSelected.Any( list => ( list.Id == NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.First().Id ) ) == false );
			moveDownCommand.Visible = ( itemsSelectedCount > 0 ) && 
				( itemSelected.Any( list => ( list.Id == NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems.Last().Id ) ) == false );

			// Show the command bar if more than one item is selected
			CommandBar.Visibility = ShowCommandBar();
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new NowPlayingAdapter( Context, listView, this, this );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the NowPlayingController
			NowPlayingController.Reporter = this;

			// Get the data
			NowPlayingController.GetNowPlayingList( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => NowPlayingController.Reporter = null;

		/// <summary>
		/// Called to allow derived classes to bind to the command bar commands
		/// </summary>
		protected override void BindCommands( CommandBar commandBar )
		{
			deleteCommand = commandBar.BindCommand( Resource.Id.delete );
			moveUpCommand = commandBar.BindCommand( Resource.Id.move_up );
			moveDownCommand = commandBar.BindCommand( Resource.Id.move_down );
		}

		/// <summary>
		/// Let derived classes determine whether or not the command bar should be shown
		/// </summary>
		/// <returns></returns>
		protected override bool ShowCommandBar() => deleteCommand.Visible || moveUpCommand.Visible || moveDownCommand.Visible;

		/// <summary>
		/// Called when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		protected override void HandleCommand( int commandId )
		{
			List<PlaylistItem> selectedItems = Adapter.SelectedItems.Values.OfType<PlaylistItem>().ToList();
			if ( commandId == Resource.Id.delete )
			{
				NowPlayingController.DeleteNowPlayingItems( selectedItems );
				LeaveActionMode();
			}
			else if ( commandId == Resource.Id.move_up )
			{
				NowPlayingController.MoveItemsUp( selectedItems );
			}
			else if ( commandId == Resource.Id.move_down )
			{
				NowPlayingController.MoveItemsDown( selectedItems );
			}
		}

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.nowplaying_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.nowplayingList;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
		
		/// <summary>
		/// Command handlers
		/// </summary>
		private CommandBinder deleteCommand = null;
		private CommandBinder moveUpCommand = null;
		private CommandBinder moveDownCommand = null;
	}
}