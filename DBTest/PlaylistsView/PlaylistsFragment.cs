using Android.OS;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public class PlaylistsFragment: PagedFragment, ExpandableListAdapter< Playlist >.IGroupContentsProvider< Playlist >
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public PlaylistsFragment()
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
			View view = inflater.Inflate( Resource.Layout.playlists_fragment, container, false );

			// Get the ExpandableListView and link to a PlaylistsAdapter
			ExpandableListView listView = view.FindViewById<ExpandableListView>( Resource.Id.playlistsLayout );

			adapter = new PlaylistsAdapter( Context, listView, this );
			listView.SetAdapter( adapter );

			// Detect when the adapter has entered Action Mode
			adapter.EnteredActionMode += EnteredActionMode;

			// Request the PlayLists from the library
			PlaylistsController.GetPlaylistsAsync( connectionModel.LibraryId );

			return view;
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_playlists, menu );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Get all the PlaylistItem entries associated with a specified Playlist.
		/// </summary>
		/// <param name="thePlayList"></param>
		public void ProvideGroupContents( Playlist thePlayList )
		{
			if ( thePlayList.PlaylistItems == null )
			{
				PlaylistsController.GetPlaylistContents( thePlayList );
			}
		}

		/// <summary>
		/// Called when the PlaylistsDataAvailableMessage is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		private void PlaylistsDataAvailable( object message )
		{
			adapter.SetData( PlaylistsViewModel.Playlists );
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		private void SongsAdded( object message )
		{
			PlaylistSongsAddedMessage songsAddedMessage = message as PlaylistSongsAddedMessage;
			adapter.SongsAdded( songsAddedMessage.Playlist, songsAddedMessage.Songs );
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
			adapter.OnCollapseRequest();
		}

		protected override void RegisterMessages()
		{
			// Register interest in Playlist messages
			Mediator.Register( PlaylistsDataAvailable, typeof( PlaylistsDataAvailableMessage ) );
			Mediator.Register( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
		}

		protected override void DeregisterMessages()
		{
			Mediator.Deregister( PlaylistsDataAvailable, typeof( PlaylistsDataAvailableMessage ) );
			Mediator.Deregister( SongsAdded, typeof( PlaylistSongsAddedMessage ) );
		}


		private PlaylistsAdapter adapter = null;
	}
}