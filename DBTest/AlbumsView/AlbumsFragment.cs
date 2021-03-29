using Android.Views;
using Android.Widget;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class AlbumsFragment: PagedFragment<Album>, ExpandableListAdapter<Album>.IGroupContentsProvider<Album>, DataReporter.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public AlbumsFragment() => ActionModeTitle = NoItemsSelectedText;

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
		public override void DataAvailable()
		{
			Activity.RunOnUiThread( () => 
			{
				// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
				Adapter.SetData( AlbumsViewModel.Albums.ToList(), BaseModel.SortSelector.ActiveSortType );

				// Indicate whether or not a filter has been applied
				AppendToTabTitle();

				// Update the icon as well
				AlbumsViewModel.FilterSelector.DisplayFilterIcon();

				base.DataAvailable();
			} );
		}

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects ) => 
			ActionModeTitle = ( selectedObjects.Songs.Count == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, selectedObjects.Songs.Count );

		/// <summary>
		/// Called when the sort selector has changes the sort order
		/// No need to wait for this to finish. Albums display will be refreshed when it is complete
		/// </summary>
		public override void SortOrderChanged() => AlbumsController.SortDataAsync();

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
		/// Remove this object from the controller
		/// </summary>
		protected override void ReleaseResources() => AlbumsController.DataReporter = null;

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
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelection FilterSelector { get; } = AlbumsViewModel.FilterSelector;

		/// <summary>
		/// The common model features are contained in the BaseViewModel
		/// </summary>
		protected override BaseViewModel BaseModel { get; } = AlbumsViewModel.BaseModel;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}