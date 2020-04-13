using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.V7.App;

namespace DBTest
{
	public class AlbumsFragment: PagedFragment<Album>, ExpandableListAdapter<Album>.IGroupContentsProvider<Album>, AlbumsController.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public AlbumsFragment() => ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_albums, menu );

			sortItem = menu.FindItem( Resource.Id.sort );
			sortItem?.SetIcon( AlbumsViewModel.SortSelector.SelectedResource );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			bool handled = false;

			if ( item.ItemId == Resource.Id.sort )
			{
				// Select the next sort order
				AlbumsViewModel.SortSelector.SelectNext();

				// Display the next sort option
				sortItem.SetIcon( AlbumsViewModel.SortSelector.SelectedResource );

				// Redisplay the data with the new sort applied
				AlbumsController.SortDataAsync( true );
			}

			if ( handled == false )
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Get all the Song entries associated with a specified Album.
		/// </summary>
		/// <param name="theArtist"></param>
		public async Task ProvideGroupContentsAsync( Album theAlbum )
		{
			if ( theAlbum.Songs == null )
			{
				await AlbumsController.GetAlbumContentsAsync( theAlbum );
			}
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnActionItemClicked( ActionMode mode, IMenuItem item ) => false;

		/// <summary>
		/// Called when the Controller has obtained the Albums data
		/// Pass it on to the adapter
		/// </summary>
		public void AlbumsDataAvailable()
		{
			Activity.RunOnUiThread( () => 
			{
				// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
				( ( AlbumsAdapter )Adapter ).SetData( AlbumsViewModel.Albums.ToList(), new Dictionary<string, int>( AlbumsViewModel.AlphaIndex ) );

				if ( AlbumsViewModel.ListViewState != null )
				{
					ListView.OnRestoreInstanceState( AlbumsViewModel.ListViewState );
					AlbumsViewModel.ListViewState = null;
				}

				// Indicate whether or not a filter has been applied
				AppendToTabTitle( ( CurrentFilter == null ) ? "" : string.Format( "\r\n[{0}]", CurrentFilter.ShortName ) );

				// Update the icon as well
				SetFilterIcon();
			} );
		}

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		public override void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			// Determine the number of songs in the selected items
			songsSelected = selectedItems.Values.OfType< Song >().Count();

			// Update the Action Mode bar title
			ActionModeTitle = ( songsSelected == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, songsSelected );

			// Show the tag command if any albums are selected
			tagCommand.Visible = ( selectedItems.Values.OfType<Album>().Count() > 0 );

			// Show the command bar if more than one item is selected
			CommandBar.Visibility = ShowCommandBar();
		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the AlbumsController
			AlbumsController.Reporter = this;

			// Get the data
			AlbumsController.GetAlbumsAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new AlbumsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		protected override void HandleCommand( int commandId )
		{
			if ( ( commandId == Resource.Id.add_to_queue ) || ( commandId == Resource.Id.play_now ) )
			{
				BaseController.AddSongsToNowPlayingListAsync( Adapter.SelectedItems.Values.OfType<Song>().ToList(),
					( commandId == Resource.Id.play_now ), AlbumsViewModel.LibraryId );
				LeaveActionMode();
			}
			else if ( commandId == Resource.Id.add_to_playlist )
			{
				// Create a Popup menu containing the play list names and show it
				PopupMenu playlistsMenu = new PopupMenu( Context, addToPlaylistCommand.BoundButton );

				AlbumsViewModel.PlaylistNames.ForEach( name => playlistsMenu.Menu.Add( 0, Menu.None, 0, name ) );

				// When a menu item is clicked get the songs from the adapter and the playlist name from the selected item
				// and pass them both to the ArtistsController
				playlistsMenu.MenuItemClick += ( sender1, args1 ) => {

					List<Song> selectedSongs = Adapter.SelectedItems.Values.OfType<Song>().ToList();

					// Determine which Playlist has been selected and add the selected songs to the playlist
					AlbumsController.AddSongsToPlaylistAsync( selectedSongs, args1.Item.TitleFormatted.ToString() );

					LeaveActionMode();
				};

				playlistsMenu.Show();
			}
			else if ( commandId == Resource.Id.tag )
			{
				List<Album> selectedAlbums = Adapter.SelectedItems.Values.OfType<Album>().ToList();

				// Create TagSelection dialogue and display it
				TagSelection selectionDialogue = new TagSelection( ( AppCompatActivity )Activity, ( List<AppliedTag> appliedTags ) => 
				{
					// Apply the changes
					FilterManagementController.ApplyTagsAsync( selectedAlbums, appliedTags );

					// Leave action mode
					LeaveActionMode();
				} );

				selectionDialogue.SelectFilter( selectedAlbums );
			}
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources()
		{
			// Remove this object from the controller
			AlbumsController.Reporter = null;

			// Save the scroll position 
			AlbumsViewModel.ListViewState = ListView.OnSaveInstanceState();
		}

		/// <summary>
		/// Called to allow derived classes to bind to the command bar commands
		/// </summary>
		protected override void BindCommands( CommandBar commandBar )
		{
			// Need to bind to the add_to_playlist command in order to anchor the playlist context menu
			addToPlaylistCommand = commandBar.BindCommand( Resource.Id.add_to_playlist );

			// Bind the tag command
			tagCommand = commandBar.BindCommand( Resource.Id.tag );
		}

		/// <summary>
		/// Let derived classes determine whether or not the command bar should be shown
		/// </summary>
		/// <returns></returns>
		protected override bool ShowCommandBar() => ( songsSelected > 0 );

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.albums_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.albumsList;

		/// <summary>
		/// Keep track of the number of songs reported as selected
		/// </summary>
		private int songsSelected = 0;

		/// <summary>
		/// The base class does nothing special with the CurrentTag property.
		/// Derived classes use it to filter what is being displayed
		/// </summary>
		protected override Tag CurrentFilter => AlbumsViewModel.CurrentFilter;

		/// <summary>
		/// The delegate used to apply a filter change
		/// </summary>
		/// <returns></returns>
		protected override FilterSelection.FilterSelectionDelegate FilterSelectionDelegate() => AlbumsController.ApplyFilterAsync;

		/// <summary>
		/// Command handlers
		/// </summary>
		private CommandBinder addToPlaylistCommand = null;
		private CommandBinder tagCommand = null;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";

		/// <summary>
		/// The sort menu item that is used to control the sort order of the displayed albums
		/// </summary>
		private IMenuItem sortItem = null;
	}
}