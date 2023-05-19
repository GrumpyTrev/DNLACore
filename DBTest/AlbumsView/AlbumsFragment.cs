using Android.Widget;
using CoreMP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	public class AlbumsFragment: PagedFragment<Album>, ExpandableListAdapter<Album>.IGroupContentsProvider<Album>
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public AlbumsFragment() => ActionMode.ActionModeTitle = NoItemsSelectedText;

		/// <summary>
		/// Get all the Song entries associated with a specified Album.
		/// These are now read on demand in the Album, so no action is required here
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Album _ )
		{
		}

		/// <summary>
		/// Called when the Controller has obtained the Albums data
		/// Pass it on to the adapter
		/// </summary>
		public override void DataAvailable()
		{
			// Generate the Fast Scroll data required by the adapter
			List<Tuple<string, int>> fastScrollSections = null;
			int[] fastScrollSectionLookup = null;

			// Do the indexing according to the sort order stored in the model
			switch ( AlbumsViewModel.SortSelection.CurrentSortOrder )
			{
				case SortOrder.alphaAscending:
				case SortOrder.alphaDescending:
				{
					GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( album, index ) => album.Name.RemoveThe().Substring( 0, 1 ).ToUpper() );
					break;
				}

				case SortOrder.yearAscending:
				case SortOrder.yearDescending:
				{
					GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( album, index ) => album.Year.ToString() );
					break;
				}

				case SortOrder.genreAscending:
				{
					GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( album, index ) => AlbumsViewModel.AlbumIndexToGenreLookup[ index ] );
					break;
				}

				case SortOrder.genreDescending:
				{
					GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( album, index ) => AlbumsViewModel.AlbumIndexToGenreLookup[ AlbumsViewModel.Albums.Count - 1 - index ] );
					break;
				}
			}

			// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
			Adapter.SetData( AlbumsViewModel.Albums.ToList(), AlbumsViewModel.SortSelection.ActiveSortType, fastScrollSections, fastScrollSectionLookup );

			// Indicate whether or not a filter has been applied
			AppendToTabTitle();

			// Update the icon as well
			FilterSelector.DisplayFilterIcon();

			base.DataAvailable();
		}

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
		protected override void PostViewCreateAction()
		{
			NotificationHandler.Register( typeof( AlbumsViewModel ), DataAvailable );
			NotificationHandler.Register( typeof( Album ), ( sender, _ ) =>
			{
				// Is this album being displayed
				int albumId = ( ( Album )sender ).Id;
				if ( AlbumsViewModel.Albums.Any( album => album.Id == albumId ) == true )
				{
					Adapter.NotifyDataSetInvalidated();
				}
			} );
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new AlbumsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called to release any resources held by the fragment
		/// Remove this object from the controller
		/// </summary>
		protected override void ReleaseResources() => NotificationHandler.Deregister();

		/// <summary>
		/// Generate the fast scroll indexes using the provided function to obtain the section name
		/// The sorted albums have already been moved/copied to the AlbumsViewModel.Albums list
		/// </summary>
		/// <param name="sectionNameProvider"></param>
		private static void GenerateIndex( ref List<Tuple<string, int>> fastScrollSections, ref int[] fastScrollSectionLookup, Func<Album, int, string> sectionNameProvider )
		{
			// Initialise the index collections
			fastScrollSections = new List<Tuple<string, int>>();
			fastScrollSectionLookup = new int[ AlbumsViewModel.Albums.Count ];

			// Keep track of when a section has already been added to the FastScrollSections collection
			Dictionary<string, int> sectionLookup = new();

			int index = 0;
			foreach ( Album album in AlbumsViewModel.Albums )
			{
				// If this is the first occurrence of the section name then add it to the FastScrollSections collection together with the index 
				string sectionName = sectionNameProvider( album, index );
				int sectionIndex = sectionLookup.GetValueOrDefault( sectionName, -1 );
				if ( sectionIndex == -1 )
				{
					sectionIndex = sectionLookup.Count;
					sectionLookup[ sectionName ] = sectionIndex;
					fastScrollSections.Add( new Tuple<string, int>( sectionName, index ) );
				}

				// Provide a quick section lookup for this album
				fastScrollSectionLookup[ index++ ] = sectionIndex;
			}
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
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelector FilterSelector { get; } = new FilterSelector( MainApp.CommandInterface.FilterAlbums, AlbumsViewModel.FilterSelection );

		protected override SortSelector SortSelector { get; } = new SortSelector( MainApp.CommandInterface.SortAlbums, AlbumsViewModel.SortSelection );

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0}{1}selected";
	}
}
