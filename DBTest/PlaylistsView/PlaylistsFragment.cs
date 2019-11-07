using Android.OS;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public class PlaylistsFragment: PagedFragment<Playlist>, ExpandableListAdapter< Playlist >.IGroupContentsProvider< Playlist >, 
		PlaylistsController.IReporter
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

			adapter = new PlaylistsAdapter( Context, listView, this, this );
			base.Adapter = adapter;

			listView.SetAdapter( adapter );

			// Initialise the PlaylistsController
			PlaylistsController.Reporter = this;

			// Request the PlayLists from the library - via a Post so that any response comes back after the UI has been created
			view.Post( () => {
				PlaylistsController.GetPlaylistsAsync( ConnectionDetailsModel.LibraryId );
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
		public void PlaylistsDataAvailable()
		{
			adapter.SetData( PlaylistsViewModel.Playlists );
		}

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void SongsAdded( string playlistName )
		{
			adapter.SongsAdded( playlistName );
		}

		protected override void ReleaseResources()
		{
			PlaylistsController.Reporter = null;
		}

		private PlaylistsAdapter adapter = null;
	}
}