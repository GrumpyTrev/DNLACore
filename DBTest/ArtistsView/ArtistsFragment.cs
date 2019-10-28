using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Linq;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<Artist>, ExpandableListAdapter<Artist>.IGroupContentsProvider<Artist>, ArtistsController.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment()
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
			View view = inflater.Inflate( Resource.Layout.library_fragment, container, false );

			// Get the ExpandableListView and link to an ArtistsAdapter
			ExpandableListView listView = view.FindViewById<ExpandableListView>( Resource.Id.libraryLayout );

			adapter = new ArtistsAdapter( Context, listView, this, this );
			base.Adapter = adapter;

			listView.SetAdapter( adapter );

			// Initialise the ArtistsController
			ArtistsController.Reporter = this;

			// Request the Artists data from the library - via a Post so that any response comes back after the UI has been created
			view.Post( () => {
				ArtistsController.GetArtistsAsync( ConnectionDetailsModel.LibraryId );
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
			inflater.Inflate( Resource.Menu.menu_library, menu );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Artist theArtist )
		{
			if ( theArtist.ArtistAlbums == null )
			{
				ArtistsController.GetArtistContents( theArtist );
			}
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnActionItemClicked( ActionMode mode, IMenuItem item )
		{
			int id = item.ItemId;

			// Form a list of Songs from the selected objects
			List<Song> selectedSongs = adapter.GetSelectedItems().Cast<Song>().ToList();
//			LeaveActionMode();

			if ( id == Resource.Id.action_add_queue )
			{
				// Get the sorted list of selected songs from the adapter and add them to the Now Playing playlist
				ArtistsController.AddSongsToNowPlayingList( selectedSongs, false );
				LeaveActionMode();
			}
			else if ( id == Resource.Id.action_playnow )
			{
				// Get the sorted list of selected songs from the adapter and replace the Now Playing playlist with them
				ArtistsController.AddSongsToNowPlayingList( selectedSongs, true );
				LeaveActionMode();
			}
			else if ( item.GroupId == PlaylistGroupId )
			{
				// Determine which Playlist has been selected and add the selected songs to the playlist
				ArtistsController.AddSongsToPlaylist( selectedSongs, item.TitleFormatted.ToString() );
				LeaveActionMode();
			}

			return false;
		}

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public void ArtistsDataAvailable()
		{
			adapter.SetData( ArtistsViewModel.Artists, ArtistsViewModel.AlphaIndex );
		}

		/// <summary>
		/// Called when the Contextual Action Bar is created.
		/// Add any configured menu items
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		protected override void OnSpecialisedCreateActionMode( IMenu menu )
		{
			// Add the list of playlists to the 'add to playlist' option
			IMenuItem playListItem = menu.FindItem( Resource.Id.action_add_playlist );

			ISubMenu subMenu = playListItem.SubMenu;

			// TO DO This is a bit iffy as the PlaylistsViewModel.PlaylistNames may not have been populated yet
			// and this 'view' should not be accessing someone else's model data
			foreach ( string name in PlaylistsViewModel.PlaylistNames )
			{
				subMenu.Add( PlaylistGroupId, Menu.None, 0, name );
			}
		}

		public override void SelectedItemsChanged( int selectedItemsCount )
		{
		}

		protected override void ReleaseResources()
		{
			ArtistsController.Reporter = null;
		}

		/// <summary>
		/// The ArtistsAdapter used to hold the Artist data and display it in the ExpandableListView
		/// </summary>
		private ArtistsAdapter adapter = null;

		/// <summary>
		/// Group id for related context menu items
		/// </summary>
		private const int PlaylistGroupId = 5555;
	}
}