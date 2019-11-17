using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Linq;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<Artist>, ExpandableListAdapter<Artist>.IGroupContentsProvider<Artist>, ArtistsController.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment()
		{
			ActionModeTitle = NoItemsSelectedText;
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_artists, menu );

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
			return false;
		}

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public void ArtistsDataAvailable()
		{
			( ( ArtistsAdapter) Adapter ).SetData( ArtistsViewModel.Artists, ArtistsViewModel.AlphaIndex );

			if ( ArtistsViewModel.ListViewState != null )
			{
				ListView.OnRestoreInstanceState( ArtistsViewModel.ListViewState );
				ArtistsViewModel.ListViewState = null;
			}
		}

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		public override void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			// Determine the number of songs in the selected items
			itemsSelected = selectedItems.Values.OfType< Song >().Count();

			// Update the Action Mode bar title
			ActionModeTitle = ( itemsSelected == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, itemsSelected );

			// Show the command bar if more than one item is selected
			CommandBar.Visibility = ShowCommandBar();
		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the ArtistsController
			ArtistsController.Reporter = this;

			// Get the data
			ArtistsController.GetArtistsAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new ArtistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		protected override void HandleCommand( int commandId )
		{
			if ( ( commandId == Resource.Id.add_to_queue ) || ( commandId == Resource.Id.play_now ) )
			{
				BaseController.AddSongsToNowPlayingList( Adapter.SelectedItems.Values.OfType<Song>().ToList(),
					( commandId == Resource.Id.play_now ), ArtistsViewModel.LibraryId );
				LeaveActionMode();
			}
			else if ( commandId == Resource.Id.add_to_playlist )
			{
				// Create a Popup menu containing the play list names and show it
				PopupMenu playlistsMenu = new PopupMenu( Context, addToPlaylistCommand.BoundButton );

				foreach ( string name in ArtistsViewModel.PlaylistNames )
				{
					playlistsMenu.Menu.Add( 0, Menu.None, 0, name );
				}

				// When a menu item is clicked get the songs from the adapter and the playlist name from the selected item
				// and pass them both to the ArtistsController
				playlistsMenu.MenuItemClick += ( sender1, args1 ) => {

					List<Song> selectedSongs = Adapter.SelectedItems.Values.OfType<Song>().ToList();

					// Determine which Playlist has been selected and add the selected songs to the playlist
					ArtistsController.AddSongsToPlaylist( selectedSongs, args1.Item.TitleFormatted.ToString() );

					LeaveActionMode();
				};

				playlistsMenu.Show();
			}
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources()
		{
			// Remove this object from the controller
			ArtistsController.Reporter = null;

			// Save the scroll position 
			ArtistsViewModel.ListViewState = ListView.OnSaveInstanceState();
		}

		/// <summary>
		/// Called to allow derived classes to bind to the command bar commands
		/// </summary>
		protected override void BindCommands( CommandBar commandBar )
		{
			// Need to bind to the add_to_playlist command in order to anchor the playlist context menu
			addToPlaylistCommand = commandBar.BindCommand( Resource.Id.add_to_playlist );
		}

		/// <summary>
		/// Let derived classes determine whether or not the command bar should be shown
		/// </summary>
		/// <returns></returns>
		protected override bool ShowCommandBar() => ( itemsSelected > 0 );

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.artists_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.artistsList;

		/// <summary>
		/// Keep track of the number of items reported as selected
		/// </summary>
		private int itemsSelected = 0;

		/// <summary>
		/// Binder to the add_to_playlist command to allow the actual button to be accessed
		/// </summary>
		private CommandBinder addToPlaylistCommand = null;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}