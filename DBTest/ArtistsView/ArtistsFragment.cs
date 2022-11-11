using Android.Widget;
using CoreMP;
using System.Linq;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<object>, ExpandableListAdapter<object>.IGroupContentsProvider<object>, DataReporter.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment() => ActionMode.ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( object artistOrArtistAlbum )
		{
			if ( artistOrArtistAlbum is ArtistAlbum artistAlbum )
			{
				ArtistsController.GetArtistAlbumContents( artistAlbum );
			}
			else
			{
				ArtistsController.GetArtistContents( ( Artist )artistOrArtistAlbum );
			}
		}

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public override void DataAvailable() => Activity.RunOnUiThread( () =>
		{
			// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
			Adapter.SetData( ArtistsViewModel.ArtistsAndAlbums.ToList(), ArtistsViewModel.SortSelection.ActiveSortType );

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
				int artistsCount = selectedObjects.Artists.Count;
				int albumsCount = selectedObjects.ArtistAlbums.Count;
				int songsCount = selectedObjects.Songs.Count;
				string albumsText = ( albumsCount > 0 ) ? string.Format( "{0} album{1} ", albumsCount, ( albumsCount == 1 ) ? "" : "s" ) : "";
				string artistsText = ( artistsCount > 0 ) ? string.Format( "{0} artist{1} ", artistsCount, ( artistsCount == 1 ) ? "" : "s" ) : "";
				string songsText = ( songsCount > 0 ) ? string.Format( "{0} song{1} ", songsCount, ( songsCount == 1 ) ? "" : "s" ) : "";

				ActionMode.ActionModeTitle = string.Format( ItemsSelectedText, artistsText, songsText, albumsText );
			}

            ActionMode.AllSelected = ( selectedObjects.Artists.Count == ArtistsViewModel.Artists.Count );
        }

        /// <summary>
        /// Called when the Select All checkbox has been clicked on the Action Bar.
        /// Pass this on to the adapter
        /// </summary>
        /// <param name="checkedState"></param>
        public override void AllSelected( bool checkedState ) => ( ( ArtistsAdapter )Adapter ).SelectAll( checkedState );

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
		/// Remove this object from the controller
		/// </summary>
		protected override void ReleaseResources() => ArtistsController.DataReporter = null;

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
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelector FilterSelector { get; } = new FilterSelector( ArtistsController.SetNewFilter, ArtistsViewModel.FilterSelection );

		protected override SortSelector SortSelector { get; } = new SortSelector( ArtistsController.SortArtists, ArtistsViewModel.SortSelection );

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0}{1}{2} selected";
	}
}
