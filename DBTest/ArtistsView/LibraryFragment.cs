using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Linq;

namespace DBTest
{
	public class LibraryFragment: PagedFragment, ExpandableListAdapter<Artist>.IGroupContentsProvider<Artist>
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public LibraryFragment()
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

			// Get the ExpandableListView and link to a LibraryAdapter
			ExpandableListView listView = view.FindViewById<ExpandableListView>( Resource.Id.libraryLayout );

			adapter = new LibraryAdapter( Context, listView, this );
			listView.SetAdapter( adapter );

			// Detect when the adapter has entered Action Mode
			adapter.EnteredActionMode += EnteredActionMode;

			// Detect when the number of selected items has changed
			adapter.SelectedItemsChanged += SelectedItemsChanged;

			// Request the Artists data from the library
			ArtistsController.GetArtistsAsync( connectionModel.LibraryId );

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

			if ( id == Resource.Id.action_add_queue )
			{
				// Get the sorted list of selected songs from the adapter and add them to the Now Playing playlist
				PlaylistsController.AddSongsToNowPlayingList( selectedSongs, false );
				LeaveActionMode();
			}
			else if ( id == Resource.Id.action_playnow )
			{
				// Get the sorted list of selected songs from the adapter and replace the Now Playing playlist with them
				PlaylistsController.AddSongsToNowPlayingList( selectedSongs, true );
				LeaveActionMode();
			}
			else if ( item.GroupId == PlaylistGroupId )
			{
				// Determine which Playlist has been selected and add the selected songs to the playlist
				PlaylistsController.AddSongsToPlaylist( selectedSongs, item.TitleFormatted.ToString() );
				LeaveActionMode();
			}

			return false;
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

			foreach ( string name in PlaylistsViewModel.PlaylistNames )
			{
				subMenu.Add( PlaylistGroupId, Menu.None, 0, name );
			}
		}

		protected override void RegisterMessages()
		{
			// Register interest in Artist data available message
			Mediator.Register( ArtistsDataAvailable, typeof( ArtistsDataAvailableMessage ) );
		}

		protected override void DeregisterMessages()
		{
			Mediator.Deregister( ArtistsDataAvailable, typeof( ArtistsDataAvailableMessage ) );
		}

		/// <summary>
		/// Called when the ArtistsDataAvailableMessage is received
		/// Display the data held in the Artists view model
		/// </summary>
		/// <param name="message"></param>
		private void ArtistsDataAvailable( object message )
		{
			adapter.SetData( ArtistsViewModel.Artists, ArtistsViewModel.AlphaIndex );
		}

		private void SelectedItemsChanged( object sender, ExpandableListAdapter<Artist>.SelectedItemsArgs e )
		{
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

		private LibraryAdapter adapter = null;

		private const int PlaylistGroupId = 5555;
	}
}