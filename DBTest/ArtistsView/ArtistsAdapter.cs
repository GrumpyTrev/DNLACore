using System;
using System.Linq;
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
		public ArtistsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<object> provider, IAdapterActionHandler actionHandler ) :
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
				bool expandGroup = ( parent.IsGroupExpanded( groupPosition ) == false );

				// Expand or collapse all of the ArtistAlbums associated with the Artist
				for ( int albumIndex = 1; albumIndex <= artist.ArtistAlbums.Count; albumIndex++ )
				{
					if ( parent.IsGroupExpanded( groupPosition + albumIndex ) != expandGroup )
					{
						base.OnGroupClick( parent, clickedView, groupPosition + albumIndex, id );
					}
				}
			}
			else
			{
				retVal = base.OnGroupClick( parent, clickedView, groupPosition, id );
			}

			return retVal;
		}

		/// <summary>
		/// Create an index from the Groups data taking into account whether or not they are expanded
		/// </summary>
		protected override void SetGroupIndex()
		{
			alphaIndexer.Clear();

			if ( SortType == SortSelector.SortType.alphabetic )
			{
				// Work out the section indexes for the sorted data
				int index = 0;
				foreach ( object groupObject in Groups )
				{
					if ( groupObject is Artist artist )
					{
						// Remember to ignore leading 'The ' here as well
						alphaIndexer.TryAdd( artist.Name.RemoveThe().Substring( 0, 1 ).ToUpper(), index );
					}

					index++;
				}
			}

			// Save a copy of the keys
			sections = alphaIndexer.Keys.ToArray();
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
			// If no view is supplied, or the supplied view does not contain a song title field then create a new view
			if ( convertView?.FindViewById<TextView>( Resource.Id.title ) == null )
			{
				convertView = inflator.Inflate( Resource.Layout.artists_song_layout, null );
			}

			// Display the Track number, Title and Duration
			Song songItem = ( Groups[ groupPosition ] as ArtistAlbum ).Songs[ childPosition ];
			convertView.FindViewById<TextView>( Resource.Id.track ).Text = songItem.Track.ToString();
			convertView.FindViewById<TextView>( Resource.Id.title ).Text = songItem.Title;
			convertView.FindViewById<TextView>( Resource.Id.duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );

			return convertView;
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
			if ( Groups[ groupPosition ] is Artist )
			{
				// If no view is supplied, or the supplied view previously contained other than an Artist then create a new view
				if ( convertView?.FindViewById<TextView>( Resource.Id.artistName ) == null )
				{
					convertView = inflator.Inflate( Resource.Layout.artists_artist_layout, null );
				}

				// Display the artist's name
				convertView.FindViewById<TextView>( Resource.Id.artistName ).Text = ( Groups[ groupPosition ] as Artist ).Name;
			}
			else
			{
				// Assuming here that it must be an ArtistAlbum
				// If no view supplied, or the supplied view previously contained a Song then create a new view
				if ( convertView?.FindViewById<TextView>( Resource.Id.albumName ) == null )
				{
					convertView = inflator.Inflate( Resource.Layout.artists_album_group_layout, null );
				}

				// Set the album text and colour
				TextView albumName = convertView.FindViewById<TextView>( Resource.Id.albumName );
				TextView albumYear = convertView.FindViewById<TextView>( Resource.Id.albumYear );

				// Save the default colours if not already done so
				if ( ColoursInitialised == false )
				{
					albumNameColour = new Color( albumName.CurrentTextColor );
					albumYearColour = new Color( albumYear.CurrentTextColor );
					ColoursInitialised = true;
				}

				// A very nasty workaround here. If action mode is in effect then remove the AlignParentLeft from the album name.
				// When the Checkbox is being shown then the album name can be positioned between the checkbox and the album year, 
				// but when there is no checkbox this does not work and the name has to be aligned with the parent.
				// This seems to be too complicated for static layout
				( ( RelativeLayout.LayoutParams )albumName.LayoutParameters ).AddRule( LayoutRules.AlignParentLeft,
					( ActionMode == true ) ? 0 : 1 );

				ArtistAlbum artAlbum = Groups[ groupPosition ] as ArtistAlbum;

				// Text is the name and year of the album but colour depends on whether or not the associated album has been played 
				albumName.Text = artAlbum.Name;
				albumYear.Text = ( artAlbum.Album.Year > 0 ) ? artAlbum.Album.Year.ToString() : " ";

				albumName.SetTextColor( ( artAlbum.Album.Played == true ) ? Color.Gray : albumNameColour );
				albumYear.SetTextColor( ( artAlbum.Album.Played == true ) ? Color.Gray : albumYearColour );
			}

			return convertView;
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
				for ( int albumIndex = 1; albumIndex <= artist.ArtistAlbums.Count; ++albumIndex )
				{
					selectionChanged |= RecordItemSelection( FormGroupTag( groupPosition + albumIndex ), selected );
					selectionChanged |= await SelectGroupContents( groupPosition + albumIndex, selected );
				}
			}
			else
			{
				// Need to find the position of the associated Artist and then determine if its selection state also needs changing
				int artistPosition = groupPosition - 1;
				while ( Groups[ artistPosition ] is ArtistAlbum )
				{
					--artistPosition;
				}

				// We now have the position of the Artist, if the ArtistAlbum has been deselected then deselect the Artist as well
				if ( selected == false )
				{
					selectionChanged |= RecordItemSelection( FormGroupTag( artistPosition ), false );
				}
				else
				{
					// Need to check all of the ArtistAlbum entries associated with the Artist. If they are all selected then select the Artist as well
					int artistAlbumCount = ( ( Artist )Groups[ artistPosition ] ).ArtistAlbums.Count;

					int albumIndex = 1;
					while ( ( albumIndex <= artistAlbumCount ) && ( IsItemSelected( FormGroupTag( artistPosition + albumIndex ) ) == true ) )
					{
						albumIndex++;
					}

					if ( albumIndex > artistAlbumCount )
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
				int artistPosition = groupPosition - 1;
				while ( Groups[ artistPosition ] is ArtistAlbum )
				{
					--artistPosition;
				}

				// If the ArtistAlbum is now expanded then check if all the other ArtistAlbums are also expanded
				// If the ArtistAlbum is now collapsed then collapse the Artist
				if ( parent.IsGroupExpanded( groupPosition ) == true )
				{
					// Need to check all of the ArtistAlbum entries associated with the Artist. If they are all selected then select the Artist as well
					int artistAlbumCount = ( ( Artist )Groups[ artistPosition ] ).ArtistAlbums.Count;

					int albumIndex = 1;
					while ( ( albumIndex <= artistAlbumCount ) && ( parent.IsGroupExpanded( artistPosition + albumIndex ) == true ) )
					{
						albumIndex++;
					}

					if ( albumIndex > artistAlbumCount )
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
		/// The Colour used to display the name of an album
		/// </summary>
		private Color albumNameColour;

		/// <summary>
		/// The Colour used to display the album year
		/// </summary>
		private Color albumYearColour;

		/// <summary>
		/// Have the default album colours been initialised
		/// </summary>
		private bool ColoursInitialised { get; set; } = false;
	}
}