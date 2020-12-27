using Android.Views;
using Android.Widget;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class AlbumsFragment: PagedFragment<Album>, ExpandableListAdapter<Album>.IGroupContentsProvider<Album>, AlbumsController.IReporter, 
		SortSelector.ISortReporter
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
			base.OnCreateOptionsMenu( menu, inflater );
			AlbumsViewModel.SortSelector.BindToMenu( menu.FindItem( Resource.Id.sort ), Context, this );
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
		public void DataAvailable()
		{
			Activity.RunOnUiThread( () => 
			{
				// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
				Adapter.SetData( AlbumsViewModel.Albums.ToList(), AlbumsViewModel.SortSelector.ActiveSortType );

				if ( AlbumsViewModel.ListViewState != null )
				{
					ListView.OnRestoreInstanceState( AlbumsViewModel.ListViewState );
					AlbumsViewModel.ListViewState = null;
				}

				// Indicate whether or not a filter has been applied
				AppendToTabTitle();

				// Update the icon as well
				AlbumsViewModel.FilterSelector.DisplayFilterIcon();

				// Display the current sort order
				AlbumsViewModel.SortSelector.DisplaySortIcon();
			} );
		}

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects ) => 
			ActionModeTitle = ( selectedObjects.SongsCount == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, selectedObjects.SongsCount );

		/// <summary>
		/// Called when the sort selector has changes the sort order
		/// No need to wait for this to finish. Albums display will be refreshed when it is complete
		/// </summary>
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		public void SortOrderChanged() => AlbumsController.SortDataAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Initialise the AlbumsController
		/// </summary>
		protected override void PostViewCreateAction() => AlbumsController.DataReporter = this;

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new AlbumsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources()
		{
			// Remove this object from the controller
			AlbumsController.DataReporter = null;

			// Remove this object from the sort selector
			AlbumsViewModel.SortSelector.Reporter = null;

			// Save the scroll position 
			AlbumsViewModel.ListViewState = ListView.OnSaveInstanceState();
		}

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.albums_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.albumsList;

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected override int Menu { get; } = Resource.Menu.menu_albums;

		/// <summary>
		/// Show or hide genres
		/// </summary>
		/// <param name="showGenre"></param>
		protected override void ShowGenre( bool showGenre ) => ( ( AlbumsAdapter )Adapter ).ShowGenre( showGenre );

		/// <summary>
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelection FilterSelector { get; } = AlbumsViewModel.FilterSelector;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}