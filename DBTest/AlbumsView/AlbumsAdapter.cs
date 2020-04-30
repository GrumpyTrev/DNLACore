using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The AlbumsAdapter class displays album and song data in an ExpandableListView
	/// </summary>
	class AlbumsAdapter: ExpandableListAdapter< Album >, ISectionIndexer
	{
		/// <summary>
		/// AlbumsAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public AlbumsAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider< Album > provider, IAdapterActionHandler actionHandler ) :
			base( context, parentView, provider,  AlbumsAdapterModel.BaseModel, actionHandler )
		{
		}

		/// <summary>
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => Groups[ groupPosition ].Songs?.Count ?? 0;

		/// <summary>
		/// Get the starting position for a section
		/// </summary>
		/// <param name="sectionIndex"></param>
		/// <returns></returns>
		public int GetPositionForSection( int sectionIndex ) => alphaIndexer[ sections[ sectionIndex ] ];

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
		public Java.Lang.Object[] GetSections() => new Java.Util.ArrayList( alphaIndexer.Keys ).ToArray();

		/// <summary>
		/// Update the data and associated sections displayed by the list view
		/// </summary>
		/// <param name="newData"></param>
		/// <param name="alphaIndex"></param>
		public void SetData( List<Album> newData, Dictionary<string, int> alphaIndex )
		{
			alphaIndexer = alphaIndex;
			sections = alphaIndexer.Keys.ToArray();

			SetData( newData );
		}

		/// <summary>
		/// Provide a view containing song details at the specified position
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
			// If the supplied view previously contained an Album heading then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.title ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unuasable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_song_layout, null );
			}

			Song item = Groups[ groupPosition ].Songs[ childPosition ];

			// Display the Tarck, Title and Duration
			convertView.FindViewById<TextView>( Resource.Id.track ).Text = item.Track.ToString();
			convertView.FindViewById<TextView>( Resource.Id.title ).Text = item.Title;
			convertView.FindViewById<TextView>( Resource.Id.duration ).Text = TimeSpan.FromSeconds( item.Length ).ToString( @"mm\:ss" );

			return convertView;
		}

		/// <summary>
		/// Derived classes must implement this method to provide a view for a group item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected override View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			// If the supplied view previously contained other than an Album then don't use it
			if ( ( convertView != null ) && ( convertView.FindViewById<TextView>( Resource.Id.albumName ) == null ) )
			{
				convertView = null;
			}

			// If no view supplied, or unusable, then create a new one
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_album_layout, null );
			}

			// Display the album and artist name
			Album displayAlbum = Groups[ groupPosition ];

			TextView albumText = convertView.FindViewById<TextView>( Resource.Id.albumName );
			TextView artistText = convertView.FindViewById<TextView>( Resource.Id.artist );

			// If the album has been played then display these fields in grey text
			if ( displayAlbum.Played == true )
			{
				albumText.SetTextColor( Color.Gray );
				artistText.SetTextColor( Color.Gray );
			}
			else
			{
				albumText.SetTextColor( Color.Black );
				artistText.SetTextColor( Color.Black );
			}

			albumText.Text = displayAlbum.Name;
			artistText.Text = ( displayAlbum.ArtistName.Length > 0 ) ? displayAlbum.ArtistName : "Unknown";

			return convertView;
		}

		/// <summary>
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected override object GetItemAt( int groupPosition, int childPosition ) => 
			( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : ( object )Groups[ groupPosition ].Songs[ childPosition ];

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// All items should be selected
		/// </summary>
		/// <param name="tag"></param>
		protected override bool SelectLongClickedItem( int tag ) => true;

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