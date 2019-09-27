using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class LibraryAdapter : ExpandableListAdapter<Artist>, ISectionIndexer
	{
		/// <summary>
		/// ArtistAlbumListViewAdapter constructor.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public LibraryAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider<Artist> provider ) :
			base( context, parentView, provider, StateModelProvider.Get( typeof( LibraryAdapterModel ) ) as LibraryAdapterModel )
		{
		}

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition )
		{
			return Groups[ groupPosition ].Contents.Count;
		}

		/// <summary>
		/// Get the starting position for a section
		/// </summary>
		/// <param name="sectionIndex"></param>
		/// <returns></returns>
		public int GetPositionForSection( int sectionIndex )
		{
			return alphaIndexer[ sections[ sectionIndex ] ];
		}

		/// <summary>
		/// Get the section that the specified position is in
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetSectionForPosition( int position )
		{
			int prevSection = 0;
			int index = 0;
			bool positionFound = false;

			while ( ( positionFound == false ) && ( index < sections.Length ) )
			{
				if ( GetPositionForSection( index ) > position )
				{
					positionFound = true;
				}
				else
				{
					prevSection = index++;
				}
			}

			return prevSection;
		}

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public Java.Lang.Object[] GetSections()
		{
			return new Java.Util.ArrayList( alphaIndexer.Keys ).ToArray();
		}

		/// <summary>
		/// Update the data and associated sections displayed by the list view
		/// </summary>
		/// <param name="newData"></param>
		/// <param name="alphaIndex"></param>
		public void SetData( List<Artist> newData, Dictionary<string, int> alphaIndex )
		{
			alphaIndexer = alphaIndex;
			sections = alphaIndexer.Keys.ToArray();

			SetData( newData );
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
			// The child can be either a ArtistAlbum or a Song which use different layouts
			object childObject = Groups[ groupPosition ].Contents[ childPosition ];
			if ( ( childObject is ArtistAlbum ) == true )
			{
				// If the supplied view previously contained a Song then don't use it
				if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.AlbumName ) == null ) )
				{
					convertView = null;
				}

				// If no view supplied, or unusable, then create a new one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.album_layout, null );
				}

				// Set the album text.
				convertView.FindViewById<TextView>( Resource.Id.AlbumName ).Text = ( ( ArtistAlbum )childObject ).Name;
			}
			else
			{
				// If the supplied view previously contained an ArtistAlbum then don't use it
				if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.Title ) == null ) )
				{
					convertView = null;
				}

				// If no view supplied, or unuasable, then create a new one
				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.song_layout, null );
				}

				Song songItem = ( Song )childObject;

				// Display the Track number, Title and Duration
				convertView.FindViewById<TextView>( Resource.Id.Track ).Text = songItem.Track.ToString();
				convertView.FindViewById<TextView>( Resource.Id.Title ).Text = songItem.Title;
				convertView.FindViewById<TextView>( Resource.Id.Duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );
			}

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
			// If the supplied view previously contained other than an Artits then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.ArtistName ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unusable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.artist_layout, null );
			}

			// Display the artist's name
			convertView.FindViewById<TextView>( Resource.Id.ArtistName ).Text = Groups[ groupPosition ].Name;

			return convertView;
		}

		/// <summary>
		/// Can the specified object be included in operations on the selected items
		/// Only Song items can be included
		/// </summary>
		/// <param name="selectedObject"></param>
		/// <returns></returns>
		protected override object FilteredSelection( int tag )
		{
			object filteredObject = null;

			if ( IsGroupTag( tag ) == false )
			{
				filteredObject = Groups[ GetGroupFromTag( tag ) ].Contents[ GetChildFromTag( tag ) ];
				if ( ( filteredObject is Song ) == false )
				{
					filteredObject = null;
				}
			}

			return filteredObject;
		}


		/// <summary>
		/// The base implementation selects or deselects the containing group according to the state of its children
		/// For the library things aren't so simple.
		/// The children are made up of one or more ArtistAlbum items and their associated Song items
		/// If the changed item is an ArtstAlbum then all of its child Song entries must be updated to reflect its state. If the ArtistAlbum is
		/// being deselected then the Artist group may also need to be deselected. If the ArtistAlbum is being selected then all of the Artist's
		/// child items need to be checked in case the Artist needs to be selected.
		/// If the changed item is a Song and it is being deselected then its AlbumArtist may need to be deselected and the Artist may need to be 
		/// deselected. If the Song is being selected then all the AlbumArtist's Songs need to be checked in case the AlbumArtist needs
		/// to be selected, this also applies to the Artist group.
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="selected"></param>
		protected override bool UpdateGroupSelectionState( int groupPosition, int childPosition, bool selected )
		{
			// Keep track of whether the group selection state has changed
			bool selectionChanged = false;

			// Need to determine the position of the ArtistAlbum associated with the selected items and the number of Songs associated with it
			int artistAlbumPosition = -1;

			if ( ( Groups[ groupPosition ].Contents[ childPosition ] is ArtistAlbum ) == true )
			{
				artistAlbumPosition = childPosition;
			}
			else
			{
				// Go back up the children until an ArtistAlbum is found
				int childIndex = childPosition - 1;
				while ( ( childIndex >= 0 ) && ( artistAlbumPosition == -1 ) )
				{
					if ( ( Groups[ groupPosition ].Contents[ childIndex ] is ArtistAlbum ) == true )
					{
						artistAlbumPosition = childIndex;
					}
					else
					{
						childIndex--;
					}
				}
			}

			// Has the ArtistAlbum been found
			if ( artistAlbumPosition != -1 )
			{
				// Now find out how many Songs there are associated with this AlbumArtist. 
				// This could be 0
				bool nextArstistAlbumFound = false;
				int childIndex = artistAlbumPosition + 1;
				int songCount = 0;
				while ( ( childIndex < Groups[ groupPosition ].Contents.Count ) && ( nextArstistAlbumFound == false ) )
				{
					if ( ( Groups[ groupPosition ].Contents[ childIndex ] is ArtistAlbum ) == true )
					{
						nextArstistAlbumFound = true;
					}
					else
					{
						songCount++;
						childIndex++;
					}
				}

				// Was the selected item an ArtistAlbum
				if ( artistAlbumPosition == childPosition )
				{
					// Select or deselect all of its children Songs
					for ( childIndex = 1; childIndex <= songCount; childIndex++ )
					{
						selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, artistAlbumPosition + childIndex ), selected );
					}

					// Reflect these changes in the Artist's state
					selectionChanged |= base.UpdateGroupSelectionState( groupPosition, 0xFFFF, selected );
				}
				else
				{
					// If a Song is deselected then deselect the ArtistAlbum and Artist items
					if ( selected == false )
					{
						selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, artistAlbumPosition ), false );
						selectionChanged |= RecordItemSelection( FormGroupTag( groupPosition ), false );
					}
					else
					{
						// If all of the Songs items are now selected then select the ArtistAlbum as well
						childIndex = 1;
						bool allSelected = true;
						while ( ( allSelected == true ) && ( childIndex <= songCount ) )
						{
							if ( IsItemSelected( FormChildTag( groupPosition, artistAlbumPosition + childIndex ) ) == false )
							{
								allSelected = false;
							}

							childIndex++;
						}

						if ( allSelected == true )
						{
							selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, artistAlbumPosition ), true );

							// Reflect this change in the Artist's state
							selectionChanged |= base.UpdateGroupSelectionState( groupPosition, 0xFFFF, true );
						}
					}
				}
			}

			return selectionChanged;
		}

		/// <summary>
		/// Lookup table specifying the starting position for each section name
		/// </summary>
		private Dictionary<string, int> alphaIndexer = null;

		/// <summary>
		/// List of section names
		/// </summary>
		private string[] sections = null;
	}
}