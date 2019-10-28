using Android.OS;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public class NowPlayingFragment: PagedFragment< PlaylistItem >, ExpandableListAdapter<PlaylistItem>.IGroupContentsProvider<PlaylistItem>, 
		NowPlayingController.IReporter, NowPlayingAdapter.IActionHandler
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public NowPlayingFragment()
		{
		}

		/// <summary>
		/// Called to create the UI components to display playlists
		/// </summary>
		/// <param name="inflater"></param>
		/// <param name="container"></param>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		protected override View OnSpecialisedCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			// Create the view
			View view = inflater.Inflate( Resource.Layout.nowplaying_fragment, container, false );

			// Get the ExpandableListView and link to a PlaylistsAdapter
			ExpandableListView listView = view.FindViewById<ExpandableListView>( Resource.Id.nowplayingList );

			adapter = new NowPlayingAdapter( Context, listView, this, this );
			base.Adapter = adapter;

			listView.SetAdapter( adapter );

			// Initialise the NowPlayingController
			NowPlayingController.Reporter = this;

			// Request the Now Playing list from the library - via a Post so that any response comes back after the UI has been created
			view.Post( () => {
				NowPlayingController.GetNowPlayingListAsync( ConnectionDetailsModel.LibraryId );
			} );

			return view;
		}

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
		/// Get all the PlaylistItem entries associated with a specified Playlist.
		/// </summary>
		/// <param name="thePlayList"></param>
		public void ProvideGroupContents( PlaylistItem theItem)
		{
		}

		public override void SelectedItemsChanged( int selectedItemsCount )
		{
		}

		/// <summary>
		/// Called when the Now Playing playlist has been read or updated
		/// Display the data held in the Now Playing view model
		/// </summary>
		/// <param name="message"></param>
		public void NowPlayingDataAvailable()
		{
			adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems );
			adapter.SongBeingPlayed( NowPlayingViewModel.SelectedSong );
		}

		/// <summary>
		/// Called when a song has been selected by the adapter
		/// Pass this change to the controller
		/// </summary>
		/// <param name="itemNo"></param>
		public void SongSelected( int itemNo )
		{
			NowPlayingController.SetSelectedSong( itemNo );
		}

		/// <summary>
		/// Called when song addition has been reported by the controller
		/// Pass on the changes to the adapter
		/// </summary>
		/// <param name="message"></param>
		private void SongsAdded( object message )
		{
			adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems );
		}

		/// <summary>
		/// Called when song selection has been reported by the controller
		/// Pass on the changes to the adapter
		/// </summary>
		public void SongSelected()
		{
			adapter.SongBeingPlayed( NowPlayingViewModel.SelectedSong );
		}

		protected override void ReleaseResources()
		{
			NowPlayingController.Reporter = null;
		}

		private NowPlayingAdapter adapter = null;
	}
}