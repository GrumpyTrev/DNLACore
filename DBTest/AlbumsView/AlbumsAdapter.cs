﻿using System;
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
	class AlbumsAdapter: ExpandableListAdapter< Album >
	{
		/// <summary>
		/// AlbumsAdapter constructor
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
		/// Show or hide the genre information
		/// </summary>
		/// <param name="show"></param>
		public void ShowGenre( bool show )
		{
			showGenre = show;
			NotifyDataSetChanged();
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
			if ( convertView?.FindViewById<TextView>( Resource.Id.title ) == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_song_layout, null );
			}

			Song item = Groups[ groupPosition ].Songs[ childPosition ];

			// Display the Track, Title and Duration
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
			if ( convertView?.FindViewById<TextView>( Resource.Id.albumName ) == null )
			{
				convertView = inflator.Inflate( Resource.Layout.albums_album_layout, null );
			}

			// Display the album, artist name and album year
			Album displayAlbum = Groups[ groupPosition ];

			// Set the album text and colour
			TextView albumName = convertView.FindViewById<TextView>( Resource.Id.albumName );
			albumName.Text = displayAlbum.Name;

			// Save the default colour if not already done so
			if ( albumNameColour == Color.Fuchsia )
			{
				albumNameColour = new Color( albumName.CurrentTextColor );
			}

			// If the album has been played then display the album text grey text
			albumName.SetTextColor( ( displayAlbum.Played == true ) ? Color.Black : albumNameColour );

			// Set the artist name
			TextView artistText = convertView.FindViewById<TextView>( Resource.Id.artist );
			artistText.Text = ( displayAlbum.ArtistName.Length > 0 ) ? displayAlbum.ArtistName : "Unknown";

			// And the year
			TextView yearText = convertView.FindViewById<TextView>( Resource.Id.year );
			yearText.Text = ( displayAlbum.Year == 0 ) ? "" : displayAlbum.Year.ToString();

			// If genres are being displayed then show the genre layout and set the genre name
			// Get the genre layout view so we can show or hide it
			RelativeLayout genreLayout = convertView.FindViewById<RelativeLayout>( Resource.Id.genreLayout );
			if ( ( showGenre == true ) && ( displayAlbum.Genre.Length > 0 ) )
			{
				// When genres are displayed the genre and year are displayed on their own line. So hide the year field that sit on the album anme line
				genreLayout.Visibility = ViewStates.Visible;
				yearText.Visibility = ViewStates.Gone;

				// Display the genre name. Alter the left margin according to whether the checkbox id being displayed ( ActonMode on)
				TextView genreView = convertView.FindViewById<TextView>( Resource.Id.genre );
				genreView.Text = displayAlbum.Genre;

				// Set the year
				convertView.FindViewById<TextView>( Resource.Id.albumGenreYear ).Text = yearText.Text;
			}
			else
			{
				// Hide the seperate genre line and make sure the year field is shown on the album name line
				genreLayout.Visibility = ViewStates.Gone;
				yearText.Visibility = ViewStates.Visible;
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
			( childPosition == 0XFFFF ) ? Groups[ groupPosition ] : ( object )Groups[ groupPosition ].Songs[ childPosition ];

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// All items should be selected
		/// </summary>
		/// <param name="tag"></param>
		protected override bool SelectLongClickedItem( int tag ) => true;

		/// <summary>
		/// Create an index from the Groups data taking into account whether or not they are expanded
		/// </summary>
		protected override void SetGroupIndex()
		{
			alphaIndexer.Clear();

			if ( ( SortType == SortSelector.SortType.alphabetic ) || ( SortType == SortSelector.SortType.year ) ||
				 ( SortType == SortSelector.SortType.genre ) )
			{
				// Work out the section indexes for the sorted data
				int index = 0;
				foreach ( Album album in Groups )
				{
					if ( SortType == SortSelector.SortType.alphabetic )
					{
						alphaIndexer.TryAdd( album.Name.RemoveThe().Substring( 0, 1 ).ToUpper(), index++ );
					}
					else if ( SortType == SortSelector.SortType.year )
					{
						alphaIndexer.TryAdd( album.Year.ToString(), index++ );
					}
					else
					{
						alphaIndexer.TryAdd( album.Genre, index++ );
					}
				}

				// Save a copy of the keys
				sections = alphaIndexer.Keys.ToArray();
			}
		}


		/// <summary>
		/// The Colour used to display the name of an album. Initialised to a colour we're never going to use
		/// </summary>
		private Color albumNameColour = new Color( Color.Fuchsia );

		/// <summary>
		/// Is genre information to be displayed
		/// </summary>
		private bool showGenre = false;
	}
}