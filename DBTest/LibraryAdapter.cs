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
			base( context, parentView, provider, ViewModelProvider.Get( typeof( LibraryAdapterModel ) ) as LibraryAdapterModel )
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
		/// Lookup table specifying the starting position for each section name
		/// </summary>
		private Dictionary<string, int> alphaIndexer = null;

		/// <summary>
		/// List of section names
		/// </summary>
		private string[] sections = null;
	}
}