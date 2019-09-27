using Android.OS;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class NowPlayingFragment : PagedFragment
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

			// Get the ListView and link to a NowPlayingAdapter
			ListView listView = view.FindViewById<ListView>( Resource.Id.nowplayingList );

			adapter = new NowPlayingAdapter( Context, listView );
			listView.Adapter = adapter;

			// Detect when the adapter has entered Action Mode
			adapter.EnteredActionMode += EnteredActionMode;

			adapter.PlaySongRequested += PlaySongRequested;

			// Request the Now Playing playlist from the library
			NowPlayingController.GetNowPlayingListAsync( connectionModel.LibraryId );

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
		/// Called when the NowPlayingDataAvailableMessage is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		private void NowPlayingDataAvailable( object message )
		{
			adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems );
		}

		/// <summary>
		/// Called when the NowPlayingSongsAddedMessage is received
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		private void SongsAdded( object message )
		{
			adapter.SetData( NowPlayingViewModel.NowPlayingPlaylist.PlaylistItems );
		}

		/// <summary>
		/// Called when a request has been received to play a particular song in the Now Playing list
		/// Raise the PlaySongMessage.
		/// For TESTING purposes tell the adapter that a song is being played
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlaySongRequested( object sender, NowPlayingAdapter.PlaySongArgs e )
		{
			new PlaySongMessage() { TrackId = e.TrackNo, SongToPlay = e.SelectedSong }.Send();
			adapter.SongBeingPlayed( e.TrackNo, e.SelectedSong );
		}

		/// <summary>
		/// Overridden in specialised classes to tell the adapter to turn off action mode
		/// </summary>
		protected override void AdapterActionModeOff()
		{
			adapter.ActionMode = false;
		}

		/// <summary>
		/// Overridden in specialised classes to tell the adapter to turn off action mode
		/// </summary>
		protected override void AdapterCollapseRequest()
		{
//			adapter.OnCollapseRequest();
		}


		protected override void RegisterMessages()
		{
			// Register interest in Now Playing messages
			// Register interest in Playlist messages
			Mediator.Register( NowPlayingDataAvailable, typeof( NowPlayingDataAvailableMessage ) );
			Mediator.Register( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
		}

		protected override void DeregisterMessages()
		{
			Mediator.Deregister( NowPlayingDataAvailable, typeof( NowPlayingDataAvailableMessage ) );
			Mediator.Deregister( SongsAdded, typeof( NowPlayingSongsAddedMessage ) );
		}


		private NowPlayingAdapter adapter = null;

	}
}