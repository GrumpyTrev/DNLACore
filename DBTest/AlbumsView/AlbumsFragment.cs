using Android.Widget;
using CoreMP;
using System.Linq;

namespace DBTest
{
	public class AlbumsFragment: PagedFragment<Album>, ExpandableListAdapter<Album>.IGroupContentsProvider<Album>, DataReporter.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public AlbumsFragment() => ActionMode.ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Get all the Song entries associated with a specified Album.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Album theAlbum )
		{
			if ( theAlbum.Songs == null )
			{
				AlbumsController.GetAlbumContents( theAlbum );
			}
		}

		/// <summary>
		/// Called when the Controller has obtained the Albums data
		/// Pass it on to the adapter
		/// </summary>
		public override void DataAvailable() => Activity.RunOnUiThread( () =>
		{
			// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
			Adapter.SetData( AlbumsViewModel.Albums.ToList(), AlbumsViewModel.SortSelection.ActiveSortType );

			// Indicate whether or not a filter has been applied
			AppendToTabTitle();

			// Update the icon as well
			FilterSelector.DisplayFilterIcon();

			base.DataAvailable();
		} );

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects )
		{
			if ( selectedObjects.Songs.Count == 0 )
			{
				ActionMode.ActionModeTitle = NoItemsSelectedText;
			}
			else
			{
				int albumsCount = selectedObjects.Albums.Count;
				int songsCount = selectedObjects.Songs.Count;
				string albumsText = ( albumsCount > 0 ) ? string.Format( "{0} album{1} ", albumsCount, ( albumsCount == 1 ) ? "" : "s" ) : "";
				string songsText = ( songsCount > 0 ) ? string.Format( "{0} song{1} ", songsCount, ( songsCount == 1 ) ? "" : "s" ) : "";

				ActionMode.ActionModeTitle = string.Format( ItemsSelectedText, albumsText, songsText );
			}

			ActionMode.AllSelected = ( selectedObjects.Albums.Count == AlbumsViewModel.Albums.Count );
		}

		/// <summary>
		/// Called when the Select All checkbox has been clicked on the Action Bar.
		/// Pass this on to the adapter
		/// </summary>
		/// <param name="checkedState"></param>
		public override void AllSelected( bool checkedState ) => ( ( AlbumsAdapter )Adapter ).SelectAll( checkedState );

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
		protected override FilterSelector FilterSelector { get; } = new FilterSelector( AlbumsController.SetNewFilter, AlbumsViewModel.FilterSelection );

		protected override SortSelector SortSelector { get; } = new SortSelector( AlbumsController.SortData, AlbumsViewModel.SortSelection );

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0}{1}selected";
	}
}
