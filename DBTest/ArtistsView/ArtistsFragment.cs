using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<object>, ExpandableListAdapter<object>.IGroupContentsProvider<object>, ArtistsController.IReporter, 
		SortSelector.ISortReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment() => ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			base.OnCreateOptionsMenu( menu, inflater );
			ArtistsViewModel.SortSelector.BindToMenu( menu.FindItem( Resource.Id.sort ), Context, this );
		}

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public async Task ProvideGroupContentsAsync( object theArtist ) => 
			await ArtistsController.GetArtistContentsAsync( ( theArtist is ArtistAlbum ) ? ( ( ArtistAlbum )theArtist ).Artist : ( Artist )theArtist );

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnActionItemClicked( ActionMode mode, IMenuItem item ) => false;

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public void DataAvailable()
		{
			// Make sure that this is being processed in the UI thread as it may have arrived during libraray scanning
			Activity.RunOnUiThread( () => {

				// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
				Adapter.SetData( ArtistsViewModel.ArtistsAndAlbums.ToList(), ArtistsViewModel.SortSelector.ActiveSortType );

				if ( ArtistsViewModel.ListViewState != null )
				{
					ListView.OnRestoreInstanceState( ArtistsViewModel.ListViewState );
					ArtistsViewModel.ListViewState = null;
				}

				// Indicate whether or not a filter has been applied
				AppendToTabTitle();

				// Update the icon as well
				SetFilterIcon();

				// Display the current sort order
				ArtistsViewModel.SortSelector.DisplaySortIcon();
			} );
		}

		/// <summary>
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects ) => 
			ActionModeTitle = ( selectedObjects.SongsCount == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, selectedObjects.SongsCount );

		/// <summary>
		/// Called when the sort selector has changed the sort order
		/// </summary>
		public void SortOrderChanged() => ArtistsController.SortArtistsAsync( true );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Initialise the ArtistsController
		/// </summary>
		protected override void PostViewCreateAction() => ArtistsController.DataReporter = this;

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new ArtistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources()
		{
			// Remove this object from the controller
			ArtistsController.DataReporter = null;

			// Save the scroll position 
			ArtistsViewModel.ListViewState = ListView.OnSaveInstanceState();
		}

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.artists_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.artistsList;

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected override int Menu { get; } = Resource.Menu.menu_artists;

		/// <summary>
		/// Show or hide genres
		/// </summary>
		/// <param name="showGenre"></param>
		protected override void ShowGenre( bool showGenre ) => ( ( ArtistsAdapter )Adapter ).ShowGenre( showGenre );

		/// <summary>
		/// Return the filter held by the model
		/// </summary>
		protected override Tag CurrentFilter => ArtistsViewModel.CurrentFilter;

		/// <summary>
		/// The current groups Tags applied to the albums
		/// </summary>
		protected override List<TagGroup> TagGroups => ArtistsViewModel.TagGroups;

		/// <summary>
		/// The delegate used to apply a filter change
		/// </summary>
		/// <returns></returns>
		protected override FilterSelection.FilterSelectionDelegate FilterSelectionDelegate() => ArtistsController.ApplyFilterAsync;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}