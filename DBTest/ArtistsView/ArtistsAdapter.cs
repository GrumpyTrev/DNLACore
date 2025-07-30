using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// ArtistsAdapter constructor.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="parentView"></param>
	/// <param name="provider"></param>
	internal class ArtistsAdapter( Context context, ExpandableListView parentView,
		ExpandableListAdapter<object>.IGroupContentsProvider<object> provider, IAdapterEventHandler actionHandler ) :
		ExpandableListAdapter<object>( context, parentView, provider, ArtistsAdapterModel.BaseModel, actionHandler )
	{

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
		public override int GetPositionForSection( int sectionIndex ) => fastScrollSections[ sectionIndex ].Item2;

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public override Java.Lang.Object[] GetSections() => javaSections;

		/// <summary>
		/// Override the base method in order to not expand Artist groups
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public override bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id ) =>
			( Groups[ groupPosition ] is Artist ) != false || base.OnGroupClick( parent, clickedView, groupPosition, id );

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
						AlbumName = convertView.FindViewById<TextView>( Resource.Id.albumName ),
						Year = convertView.FindViewById<TextView>( Resource.Id.year ),
						Genre = convertView.FindViewById<TextView>( Resource.Id.genre ),
					};
				}

				// Display the album
				( ( AlbumViewHolder )convertView.Tag ).DisplayAlbum( ( ArtistAlbum )Groups[ groupPosition ] );
			}

			return convertView;
		}

		/// <summary>
		/// View holder for the group Artist items
		/// </summary>
		private class ArtistViewHolder : ExpandableListViewHolder
		{
			public void DisplayArtist( Artist artist ) => Name.Text = artist.Name;

			public TextView Name { get; set; }
		}

		/// <summary>
		/// View holder for the group AlbumView items
		/// </summary>
		private class AlbumViewHolder : ExpandableListViewHolder
		{
			public void DisplayAlbum( ArtistAlbum artistAlbum )
			{
				// Save the default colour if not already done so
				if ( albumNameColour == Color.Fuchsia )
				{
					albumNameColour = new Color( AlbumName.CurrentTextColor );
				}

				AlbumName.Text = artistAlbum.Name;
				AlbumName.SetTextColor( ( artistAlbum.Album.Played == true ) ? Color.Black : albumNameColour );

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
			private static Color albumNameColour = new( Color.Fuchsia );
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
		protected override bool GroupSelectionHasChanged( int groupPosition, bool selected )
		{
			bool selectionChanged = false;

			if ( Groups[ groupPosition ] is Artist artist )
			{
				// Expand or collapse all of the ArtistAlbums associated with the Artist
				int albumIndex = groupPosition + 1;
				while ( ( albumIndex < Groups.Count ) && ( Groups[ albumIndex ] is ArtistAlbum ) )
				{
					selectionChanged |= RecordItemSelection( FormGroupTag( albumIndex ), selected );
					selectionChanged |= SelectGroupContents( albumIndex, selected );
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
		/// Special background rendering is required for the Artist item
		/// </summary>
		protected override void RenderBackground( View convertView )
		{
			// If this is an unselected Artist then work out if any of the ArtistAlbums, or any Songs in the ArtistAlbums are selected
			if ( ( convertView.Tag is ArtistViewHolder artistView ) && ( IsItemSelected( artistView.ItemTag ) == false ) )
			{
				bool anythingSelected = false;

				// The first ArtistAlbum to check should be the next group item
				int albumIndex = GetGroupFromTag( artistView.ItemTag ) + 1;
				while ( ( anythingSelected == false ) && ( albumIndex < Groups.Count ) && ( Groups[ albumIndex ] is ArtistAlbum ) )
				{
					int albumTag = FormGroupTag( albumIndex );
					anythingSelected = IsItemSelected( albumTag ) || AnyChildSelected( albumTag );

					albumIndex++;
				}

				if ( anythingSelected == true )
				{
					SetPartialBackground( convertView );
				}
				else
				{
					SetUnselectedBackground( convertView );
				}
			}
			else
			{
				base.RenderBackground( convertView );
			}
		}

		/// <summary>
		/// Find the Artist index associated with the specified ArtistAlbum index
		/// </summary>
		/// <param name="artistAlbumPosition"></param>
		/// <returns></returns>
		private int FindArtistPosition( int artistAlbumPosition )
		{
			while ( Groups[ --artistAlbumPosition ] is ArtistAlbum )
			{
			}

			return artistAlbumPosition;
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
