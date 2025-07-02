using Android.Widget;
using CoreMP;
using System;
using System.Collections.Generic;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<object>, ExpandableListAdapter<object>.IGroupContentsProvider<object>
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment() => ActionMode.ActionModeTitle = string.Empty;

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( object artistOrArtistAlbum )
		{
			if ( artistOrArtistAlbum is ArtistAlbum artistAlbum )
			{
				Artist.GetArtistAlbumSongs( artistAlbum );
			}
			else
			{
				( ( Artist )artistOrArtistAlbum ).GetSongs();
			}
		}

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public override void DataAvailable()
		{
			// Generate the Fast Scroll data required by the adapter
			List<Tuple<string, int>> fastScrollSections = null;
			int[] fastScrollSectionLookup = null;

			// Generate the fast scroll data for alpha sorting
			if ( ArtistsViewModel.SortSelection.CurrentSortOrder is SortOrder.alphaDescending or SortOrder.alphaAscending )
			{
				GenerateIndex( ref fastScrollSections, ref fastScrollSectionLookup, ( artist ) => artist.Name.RemoveThe()[ ..1 ].ToUpper() );
			}

			// Pass shallow copies of the data to the adapter to protect the UI from changes to the model
			Adapter.SetData( [ .. ArtistsViewModel.ArtistsAndAlbums ], ArtistsViewModel.SortSelection.ActiveSortType, fastScrollSections, fastScrollSectionLookup );

			// Indicate whether or not a filter has been applied
			AppendToTabTitle();

			// Update the icon as well
			FilterSelector.DisplayFilterIcon();

			base.DataAvailable();
		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Initialise the ArtistsController
		/// </summary>
		protected override void PostViewCreateAction()
		{
			NotificationHandler.Register<ArtistsViewModel>( DataAvailable );
			NotificationHandler.Register<Album>( ( sender ) =>
			{
				// Only process this album if it is in the same library as is being displayed
				// It may be in another library if this is being called as part of a library synchronisation process
				if ( ( ( Album ) sender).LibraryId == ArtistsViewModel.LibraryId )
				{
					Adapter.NotifyDataSetInvalidated();
				}
			} );
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new ArtistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called to release any resources held by the fragment
		/// Remove this object from the controller
		/// </summary>
		protected override void ReleaseResources() => NotificationHandler.Deregister();

		/// <summary>
		/// Generate the fast scroll indexes using the provided function to obtain the section name
		/// The sorted albums have already been moved/copied to the ArtistsViewModel.ArtistsAndAlbums list
		/// </summary>
		/// <param name="sectionNameProvider"></param>
		private void GenerateIndex( ref List<Tuple<string, int>> fastScrollSections, ref int[] fastScrollSectionLookup, Func<Artist, string> sectionNameProvider )
		{
			// Initialise the index collections
			fastScrollSections = [];
			fastScrollSectionLookup = new int[ ArtistsViewModel.ArtistsAndAlbums.Count ];

			// Keep track of when a section has already been added to the FastScrollSections collection
			Dictionary<string, int> sectionLookup = [];

			// Keep track of the last section index allocated or found for an Artist as it will also be used for the associated ArtistAlbum entries
			int sectionIndex = -1;

			int index = 0;
			foreach ( object artistOrAlbum in ArtistsViewModel.ArtistsAndAlbums )
			{
				// Only add section names for Artists, not ArtistAlbums
				if ( artistOrAlbum is Artist artist )
				{
					// If this is the first occurrence of the section name then add it to the FastScrollSections collection together with the index 
					string sectionName = sectionNameProvider( artist );
					sectionIndex = sectionLookup.GetValueOrDefault( sectionName, -1 );
					if ( sectionIndex == -1 )
					{
						sectionIndex = sectionLookup.Count;
						sectionLookup[ sectionName ] = sectionIndex;
						fastScrollSections.Add( new Tuple<string, int>( sectionName, index ) );
					}
				}

				// Provide a quick section lookup for this entry
				fastScrollSectionLookup[ index++ ] = sectionIndex;
			}
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
		/// The menu resource for this fragment's action bar
		/// /// </summary>
		protected override int ActionMenu { get; } = Resource.Menu.menu_artists_action;

		/// <summary>
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected override FilterSelector FilterSelector { get; } = new FilterSelector( MainApp.CommandInterface.FilterArtists, ArtistsViewModel.FilterSelection );

		protected override SortSelector SortSelector { get; } = new SortSelector( MainApp.CommandInterface.SortArtists, ArtistsViewModel.SortSelection );
	}
}
