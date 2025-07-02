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
		public AlbumsFragment() => ActionMode.ActionModeTitle = string.Empty;

		/// <summary>
		/// Get all the Song entries associated with a specified Album.
		/// These are now read on demand in the Album, so no action is required here
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Album _ ) {}

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
					GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( album, index ) => album.Name.RemoveThe()[ ..1 ].ToUpper() );
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
			Adapter.SetData( [ .. AlbumsViewModel.Albums ], AlbumsViewModel.SortSelection.ActiveSortType, fastScrollSections, fastScrollSectionLookup );

			// Indicate whether or not a filter has been applied
			AppendToTabTitle();

			// Update the icon as well
			FilterSelector.DisplayFilterIcon();

			base.DataAvailable();
		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Check for when the AlbumsViewModel has been initialised
			NotificationHandler.Register<AlbumsViewModel>( DataAvailable );

			// Register interest in the Album Played property.
			// If the Album is being displayed then invalidate the view
			NotificationHandler.Register<Album>( nameof( Album.Played ), ( sender ) =>
			{
				if ( AlbumsViewModel.Albums.Any( album => album.Id == ( ( Album )sender ).Id ) == true )
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
			fastScrollSections = [];
			fastScrollSectionLookup = new int[ AlbumsViewModel.Albums.Count ];

			// Keep track of when a section has already been added to the FastScrollSections collection
			Dictionary<string, int> sectionLookup = [];

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
		/// The menu resource for this fragment's action bar
		/// /// </summary>
		protected override int ActionMenu { get; } = Resource.Menu.menu_albums_action;

		/// <summary>
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelector FilterSelector { get; } = new FilterSelector( MainApp.CommandInterface.FilterAlbums, AlbumsViewModel.FilterSelection );

		protected override SortSelector SortSelector { get; } = new SortSelector( MainApp.CommandInterface.SortAlbums, AlbumsViewModel.SortSelection );
	}
}
