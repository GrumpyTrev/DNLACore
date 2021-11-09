using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class ArtistsAdapter : ExpandableListAdapter<object>
	{
		/// <summary>
		/// ArtistsAdapter constructor.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public ArtistsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<object> provider, IAdapterEventHandler actionHandler ) :
			base( context, parentView, provider, ArtistsAdapterModel.BaseModel, actionHandler )
		{
		}

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition )
		{
			int count = 0;

			// Only attempt to give a non-zero count if the group is an ArtistAlbum
			if ( Groups[ groupPosition ] is ArtistAlbum artistAlbum )
			{
				count = artistAlbum.Songs?.Count ?? 0;
			}

			return count;
		}

		/// <summary>
		/// There is only one Child Type - the songs
		/// </summary>
		public override int ChildTypeCount => 1;

		/// <summary>
		/// As there is only one Child Type always return 0
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override int GetChildType( int groupPosition, int childPosition ) => 0;

		/// <summary>
		/// There are two group types, the artist and the artistalbum
		/// </summary>
		public override int GroupTypeCount => 2;

		/// <summary>
		/// Return 0 if the group is an Artist, return 1 if the group is an ArtistAlbum
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetGroupType( int groupPosition ) => ( Groups[ groupPosition ] is Artist ) ? 0 : 1;

		/// <summary>
		/// Get the starting position for a section
		/// </summary>
		/// <param name="sectionIndex"></param>
		/// <returns></returns>
		public override int GetPositionForSection( int sectionIndex ) => ArtistsViewModel.FastScrollSections[ sectionIndex ].Item2;

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public override Java.Lang.Object[] GetSections() => javaSections;

		/// <summary>
		/// Override the base method in order to process Artist groups differently
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			bool retVal = true;

			if ( Groups[ groupPosition ] is Artist artist )
			{
				// If the Artist group is collapsed then expand all the ArtistAlbum groups that are collapsed
				// If the Artist group is expanded then collapse all the ArtistAlbum groups that are expanded
				bool expandGroup = !parent.IsGroupExpanded( groupPosition );

				// Expand or collapse all of the ArtistAlbums associated with the Artist
				PerformActionOnArtistAlbums( groupPosition, ( artistAlbumIndex ) =>
				{
					if ( parent.IsGroupExpanded( artistAlbumIndex ) != expandGroup )
					{
						base.OnGroupClick( parent, clickedView, artistAlbumIndex, id );
					}
				} );
			}
			else
			{
				retVal = base.OnGroupClick( parent, clickedView, groupPosition, id );
			}

			return retVal;
		}

        /// <summary>
        /// Either select or deselect all the displayed items
        /// </summary>
        /// <param name="select"></param>
        public async void SelectAll( bool select )
        {
            bool selectionChanged = false;
            for ( int groupIndex = 0; groupIndex < Groups.Count; ++groupIndex )
            {
                if ( Groups[ groupIndex ] is ArtistAlbum artistAlbum )
                {
                    if ( artistAlbum.Songs == null )
                    {
                        await contentsProvider.ProvideGroupContentsAsync( Groups[ groupIndex ] );
                    }

                    selectionChanged |= await SelectGroupContents( groupIndex, select );
                }

                selectionChanged |= RecordItemSelection( FormGroupTag( groupIndex ), select );
            }

            if ( selectionChanged == true )
            {
                stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// This is called by the base class when new data has been provided in order to create some of the fast scroll data.
        /// Most of this has already been done in the ArtistsController.
        /// All that is missing is the copying of the section names into an array of Java strings
        /// </summary>
        protected override void SetGroupIndex()
		{
			// Clear the array first in case there are none
			javaSections = null;

			if ( ArtistsViewModel.FastScrollSections != null )
			{
				// Size the section array from the ArtistsViewModel.FastScrollSections
				javaSections = new Java.Lang.Object[ ArtistsViewModel.FastScrollSections.Count ];
				for ( int index = 0; index < javaSections.Length; ++index )
				{
					javaSections[ index ] = new Java.Lang.String( ArtistsViewModel.FastScrollSections[ index ].Item1 );
				}
			}
		}

		/// <summary>
		/// Provide a view containing either album or song details at the specified position
		/// Attempt to reuse the supplied view if it previously contained the same type of detail.
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="isLastChild"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent )
		{
			// If no view is supplied then create a new view
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.artists_song_layout, null );
				convertView.Tag = new SongViewHolder()
				{
					SelectionBox = GetSelectionBox( convertView ),
					Track = convertView.FindViewById<TextView>( Resource.Id.track ),
					Title = convertView.FindViewById<TextView>( Resource.Id.title ),
					Duration = convertView.FindViewById<TextView>( Resource.Id.duration )
				};
			}

			// Display the Track number, Title and Duration
			( ( SongViewHolder )convertView.Tag ).DisplaySong( ( Groups[ groupPosition ] as ArtistAlbum ).Songs[ childPosition ] );

			return convertView;
		}

		/// <summary>
		/// View holder for the child Song items
		/// </summary>
		private class SongViewHolder : ExpandableListViewHolder
		{
			public void DisplaySong( Song song )
			{
				Track.Text = song.Track.ToString();
				Title.Text = song.Title;
				Duration.Text = TimeSpan.FromSeconds( song.Length ).ToString( @"mm\:ss" );
			}

			public TextView Track { get; set; }
			public TextView Title { get; set; }
			public TextView Duration { get; set; }
		}

		/// <summary>
		/// Derived classes must implement this method to provide a view for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			// Display either an Artist or ArtistAlbum according to the type of the object
			if ( Groups[ groupPosition ] is Artist artist )
			{
				// If no view is supplied then create a new view
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.artists_artist_layout, null );
					convertView.Tag = new ArtistViewHolder()
					{
						SelectionBox = GetSelectionBox( convertView ),
						Name = convertView.FindViewById<TextView>( Resource.Id.artistName )
					};
				}

				// Display the artist's name
				( ( ArtistViewHolder )convertView.Tag ).DisplayArtist( artist );
			}
			else
			{
				// Assuming here that it must be an ArtistAlbum
				// If no view supplied then create a new view
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.artists_album_layout, null );
					convertView.Tag = new AlbumViewHolder()
					{
						SelectionBox = GetSelectionBox( convertView ),
						AlbumName = convertView.FindViewById<TextView>( Resource.Id.albumName ),
						Year = convertView.FindViewById<TextView>( Resource.Id.year ),
						Genre = convertView.FindViewById<TextView>( Resource.Id.genre ),
					};
				}

				// Display the album
				( ( AlbumViewHolder )convertView.Tag ).DisplayAlbum( ( ArtistAlbum )Groups[ groupPosition ], ActionMode );
			}

			return convertView;
		}

		/// <summary>
		/// View holder for the group Artist items
		/// </summary>
		private class ArtistViewHolder : ExpandableListViewHolder
		{
			public void DisplayArtist( Artist artist )
			{
				Name.Text = artist.Name;
			}

			public TextView Name { get; set; }
		}

		/// <summary>
		/// View holder for the group AlbumView items
		/// </summary>
		private class AlbumViewHolder : ExpandableListViewHolder
		{
			public void DisplayAlbum( ArtistAlbum artistAlbum, bool actionMode )
			{
				// Save the default colour if not already done so
				if ( albumNameColour == Color.Fuchsia )
				{
					albumNameColour = new Color( AlbumName.CurrentTextColor );
				}

				AlbumName.Text = artistAlbum.Name;
				AlbumName.SetTextColor( ( artistAlbum.Album.Played == true ) ? Color.Black : albumNameColour );

				// A very nasty workaround here. If action mode is in effect then remove the AlignParentLeft from the album name.
				// When the Checkbox is being shown then the album name can be positioned between the checkbox and the album year, 
				// but when there is no checkbox this does not work and the name has to be aligned with the parent.
				// This seems to be too complicated for static layout
				( ( RelativeLayout.LayoutParams )AlbumName.LayoutParameters ).AddRule( LayoutRules.AlignParentLeft, ( actionMode == true ) ? 0 : 1 );

				// Set the year text
				Year.Text = ( artistAlbum.Album.Year > 0 ) ? artistAlbum.Album.Year.ToString() : " ";

				// Display the genres. Replace any spaces in the genres with non-breaking space characters. This prevents a long genres string with a 
				// space near the start being broken at the start, It just looks funny.
				Genre.Text = artistAlbum.Album.Genre.Replace( ' ', '\u00a0' );
			}

			public TextView AlbumName { get; set; }
			public TextView Year { get; set; }
			public TextView Genre { get; set; }

			/// <summary>
			/// The Colour used to display the name of an album. Initialised to a colour we're never going to use
			/// </summary>
			private static Color albumNameColour = new Color( Color.Fuchsia );
		}

		/// <summary>
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) =>
			( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : ( object )( Groups[ groupPosition ] as ArtistAlbum ).Songs[ childPosition ];

		/// <summary>
		/// Called when a group has been selected or deselected to allow derived classes to perform their own processing
		/// If an Artist has been selected then select all of its associated ArtistAlbum entries.
		/// If an Artist has been deselected then deselect all of its associated ArtistAlbum entries.
		/// If an ArtistAlbum has been selected then check if all the Artist's ArtistAlbum entries have been selected and if so select the Artist
		/// If an ArtistAlbum has been deselected then deselect the Artist
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		protected override async Task<bool> GroupSelectionHasChanged( int groupPosition, bool selected )
		{
			bool selectionChanged = false;

			if ( Groups[ groupPosition ] is Artist artist )
			{
                // Expand or collapse all of the ArtistAlbums associated with the Artist
                int albumIndex = groupPosition + 1;
                while ( ( albumIndex < Groups.Count ) && ( Groups[ albumIndex ] is ArtistAlbum ) )
                {
                    selectionChanged |= RecordItemSelection( FormGroupTag( albumIndex ), selected );
                    selectionChanged |= await SelectGroupContents( albumIndex, selected );
                    albumIndex++;
                }
            }
			else
			{
				// Need to find the position of the associated Artist and then determine if its selection state also needs changing
				int artistPosition = FindArtistPosition( groupPosition );

				// We now have the position of the Artist, if the ArtistAlbum has been deselected then deselect the Artist as well
				if ( selected == false )
				{
					selectionChanged |= RecordItemSelection( FormGroupTag( artistPosition ), false );
                }
				else
				{
					// Need to check all of the ArtistAlbum entries associated with the Artist. If they are all selected then select the Artist as well
					if ( CheckAllArtistAlbums( artistPosition, ( artistAlbumIndex ) => IsItemSelected( FormGroupTag( artistAlbumIndex ) ) ) == true )
					{
						selectionChanged |= RecordItemSelection( FormGroupTag( artistPosition ), true );
					}
				}
			}

			return selectionChanged;
		}

		/// <summary>
		/// Called when the collapse state of a group has changed.
		/// If the group is an ArtistAlbum then check whether or not the parent Artist should be shown as expanded or collapsed
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="groupPosition"></param>
		protected override void GroupCollapseStateChanged( ExpandableListView parent, int groupPosition )
		{
			if ( Groups[ groupPosition ] is ArtistAlbum artistAlbum )
			{
				// Find the Artist associated with this ArtistAlbum
				int artistPosition = FindArtistPosition( groupPosition );

				// If the ArtistAlbum is now expanded then check if all the other ArtistAlbums are also expanded
				// If the ArtistAlbum is now collapsed then collapse the Artist
				if ( parent.IsGroupExpanded( groupPosition ) == true )
				{
					// Need to check all of the ArtistAlbum entries associated with the Artist. If they are all expanded then expand the Artist as well
					if ( CheckAllArtistAlbums( artistPosition, ( artistAlbumIndex ) => parent.IsGroupExpanded( artistAlbumIndex ) ) == true )
					{
						// Add this to the record of which groups are expanded
						adapterModel.ExpandedGroups.Add( artistPosition );

						// Now expand the group
						parent.ExpandGroup( artistPosition );
					}
				}
				else
				{
					// If the parent is expanded then collapse it
					if ( parent.IsGroupExpanded( artistPosition ) == true )
					{
						adapterModel.ExpandedGroups.Remove( artistPosition );

						// Now collapse the group
						parent.CollapseGroup( artistPosition );
					}
				}
			}
		}

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// If the item selected when going into Action Mode is not an Artist item then select it
		/// </summary>
		/// <param name="tag"></param>
		protected override bool SelectLongClickedItem( int tag ) => ( IsGroupTag( tag ) == false ) || ( Groups[ GetGroupFromTag( tag ) ] is ArtistAlbum );

		/// <summary>
		/// The section lookup used for fast scrolling
		/// </summary>
		protected override int[] FastScrollLookup { get; } = ArtistsViewModel.FastScrollSectionLookup;

		/// <summary>
		/// Find the Artist index associated with the specified ArtistAlbum index
		/// </summary>
		/// <param name="artistAlbumPosition"></param>
		/// <returns></returns>
		private int FindArtistPosition( int artistAlbumPosition )
		{
			while ( Groups[ --artistAlbumPosition ] is ArtistAlbum ) {}

			return artistAlbumPosition;
		}

		/// <summary>
		/// Perform the specified action on all the ArtistAlbum entries associated with an Artist
		/// </summary>
		/// <param name="artistPosition"></param>
		/// <param name="action"></param>
		private void PerformActionOnArtistAlbums( int artistPosition, Action<int> action )
		{
			int albumIndex = artistPosition + 1;
			while ( ( albumIndex < Groups.Count ) && ( Groups[ albumIndex ] is ArtistAlbum ) )
			{
				// Perform the provided action on this ArtistAlbum entry
				action( albumIndex );
				albumIndex++;
			}
		}

		/// <summary>
		/// Check whether of not all the ArtistAlbum entries associated with an Artist fulfill the specified condition
		/// </summary>
		/// <param name="artistPosition"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		private bool CheckAllArtistAlbums( int artistPosition, Func<int, bool> condition )
		{
			bool allArtistAlbumsFulfillCondition = true;

			int albumIndex = artistPosition + 1;
			while ( ( allArtistAlbumsFulfillCondition == true ) && ( albumIndex < Groups.Count ) && ( Groups[ albumIndex ] is ArtistAlbum ) )
			{
				allArtistAlbumsFulfillCondition = condition( albumIndex );
				albumIndex++;
			}

			return allArtistAlbumsFulfillCondition;
		}
	}
}
