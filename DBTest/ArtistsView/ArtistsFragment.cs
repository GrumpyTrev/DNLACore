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
			inflater.Inflate( Resource.Menu.menu_artists, menu );

			ArtistsViewModel.SortSelector.BindToMenu( menu.FindItem( Resource.Id.sort ), Context, this );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public async Task ProvideGroupContentsAsync( object theArtist )
		{
			// If the object is an ArtistAlbum then get its contents (songs )
			// TO DO - regardless of whether an Artist or ArtistAlbum is selected, all the Artists contents are grabbed (already being done here?)
			if ( theArtist is ArtistAlbum )
			{
				await ArtistsController.GetArtistContentsAsync( ( theArtist as ArtistAlbum ).Artist );
			}
			else
			{
				await ArtistsController.GetArtistContentsAsync( theArtist as Artist );
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
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public void ArtistsDataAvailable()
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
		protected override void SelectedItemsChanged( List<object> selectedObjects )
		{
			// Determine the number of songs in the selected items
			songsSelected = selectedObjects.OfType< Song >().Count();

			// Update the Action Mode bar title
			ActionModeTitle = ( songsSelected == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, songsSelected );
		}

		/// <summary>
		/// Called when the sort selector has changed the sort order
		/// </summary>
		public void SortOrderChanged() => ArtistsController.SortArtistsAsync( true );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the ArtistsController
			ArtistsController.Reporter = this;

			// Get the data
			ArtistsController.GetArtists( ConnectionDetailsModel.LibraryId );
		}

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
			ArtistsController.Reporter = null;

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
		/// Keep track of the number of songs reported as selected
		/// </summary>
		private int songsSelected = 0;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}