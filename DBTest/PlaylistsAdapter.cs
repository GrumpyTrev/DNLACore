using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Color = Android.Graphics.Color;

namespace DBTest
{
	class PlaylistsAdapter: BaseExpandableListAdapter, ISectionIndexer
	{
		/// <summary>
		/// PlaylistsAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public PlaylistsAdapter( Context context, ExpandableListView parentView, IArtistContentsProvider provider )
		{
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );
			parentView.SetOnGroupClickListener( new GroupClickListener() { Adapter = this, Provider = provider } );
			parentView.SetOnChildClickListener( new ChildClickListener() { Adapter = this } );
			parentView.OnItemLongClickListener = new LongClickListener() { Adapter = this };
		}

		/// <summary>
		/// The number of artists (groups)
		/// </summary>
		public override int GroupCount
		{
			get
			{
				return Artists.Count;
			}
		}

		/// <summary>
		/// Required by interface
		/// </summary>
		public override bool HasStableIds
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Required by interface
		/// </summary>
		public override Java.Lang.Object GetChild( int groupPosition, int childPosition )
		{
			return null;
		}

		/// <summary>
		/// Required by interface
		/// </summary>
		public override long GetChildId( int groupPosition, int childPosition )
		{
			return childPosition;
		}

		/// <summary>
		/// Albums and songs associated with artist
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition )
		{
			return Artists[ groupPosition ].Contents.Count;
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
		public override View GetChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent )
		{
			// The child can be either a ArtistAlbum or a Song which use different layouts
			object childObject = Artists[ groupPosition ].Contents[ childPosition ];
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

			// Tag the view with the group and child position
			convertView.Tag = ( groupPosition << 16 ) + childPosition;

			// Display the checkbox
			RenderCheckbox( convertView, groupPosition, childPosition );

			return convertView;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override Java.Lang.Object GetGroup( int groupPosition )
		{
			return null;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override long GetGroupId( int groupPosition )
		{
			return groupPosition;
		}

		/// <summary>
		/// Provide a view containing artist details at the specified position
		/// Attempt to reuse the supplied view if it previously contained the same type of data.
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
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
			convertView.FindViewById<TextView>( Resource.Id.ArtistName ).Text = Artists[ groupPosition ].Name;

			// Tag the view with the group and child position
			convertView.Tag = ( groupPosition << 16 ) + 0x0FFFF;

			// Display the checkbox
			RenderCheckbox( convertView, groupPosition, 0x0FFFF );

			return convertView;
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
		/// Are child items selectable
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override bool IsChildSelectable( int groupPosition, int childPosition )
		{
			return true;
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
			Artists = newData;
			NotifyDataSetChanged();
		}

		/// <summary>
		/// Set or clear Action Mode.
		/// In Action Mode checkboxes appear alongside the items and items can be selected
		/// </summary>
		public bool ActionMode
		{
			get
			{
				return actionMode;
			}
			set
			{
				// Action mode determines whether or not check boxes are shown so refresh the displayed items
				if ( actionMode != value )
				{
					actionMode = value;

					if ( actionMode == false )
					{
						// Clear all selections when leaving Action Mode
						checkedObjects.Clear();
					}

					NotifyDataSetChanged();
				}
			}
		}

		/// <summary>
		/// Collapse any expanded groups
		/// </summary>
		public void OnCollapseRequest()
		{
		}

		/// <summary>
		/// Called when an item's checkbox has been selected
		/// Update the stored state for the item contained in the tag
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionBoxClick( object sender, EventArgs e )
		{
			CheckBox selectionBox = sender as CheckBox;
			int tag = ( int )( ( CheckBox )sender ).Tag;

			if ( checkedObjects.Contains( tag ) == true )
			{
				checkedObjects.Remove( tag );
			}
			else
			{
				checkedObjects.Add( tag );
			}
		}

		/// <summary>
		/// Show or hide the check box and sets its state from that held for the item
		/// </summary>
		/// <param name="convertView"></param>
		/// <param name="group"></param>
		/// <param name="child"></param>
		private void RenderCheckbox( View convertView, int group, int child )
		{
			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );

			// Save the item identifier in the check box for the click event
			selectionBox.Tag = ( group << 16 ) + child;

			// Show or hide the checkbox
			selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;

			// Retrieve the cheked state of the item and set the checkbox state accordingly
			selectionBox.Checked = ( checkedObjects.Contains( ( int )selectionBox.Tag ) == true );

			// Trap checkbox clicks
			selectionBox.Click -= SelectionBoxClick;
			selectionBox.Click += SelectionBoxClick;
		}

		/// <summary>
		/// Interface that classes providing Artist details must implement.
		/// </summary>
		public interface IArtistContentsProvider
		{
			/// <summary>
			/// Provide album and song details for the specified Artist
			/// </summary>
			/// <param name="theArtist"></param>
			void ProvideArtistContents( Artist theArtist );

			/// <summary>
			/// The number of expanded groups has changed
			/// </summary>
			/// <param name="count"></param>
			void ExpandedGroupCountChanged( int count );
		}

		/// <summary>
		/// The set of artists representing the groups displayed by the ExpandableListView
		/// </summary>
		public List<Artist> Artists { get; set; } = new List<Artist>();

		/// <summary>
		/// The event used to indicate that Acion Mode has been entered
		/// </summary>
		public event EventHandler EnteredActionMode;

		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		private HashSet< int > checkedObjects = new HashSet< int >();

		/// <summary>
		/// Inflator used to create the item view 
		/// </summary>
		private readonly LayoutInflater inflator = null;

		/// <summary>
		/// Lookup table specifying the starting position for each section name
		/// </summary>
		private Dictionary<string, int> alphaIndexer = null;

		/// <summary>
		/// List of section names
		/// </summary>
		private string[] sections = null;

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		private bool actionMode = false;

		/// <summary>
		/// Class used to listen out for clicks on group items
		/// </summary>
		private class GroupClickListener: Java.Lang.Object, ExpandableListView.IOnGroupClickListener
		{
			/// <summary>
			/// Called when a group item has been clicked
			/// If a group/artist is being expanded then get its contents if not previously displayed
			/// Keep track of which groups have been expanded and the last group expanded
			/// </summary>
			/// <param name="parent"></param>
			/// <param name="clickedView"></param>
			/// <param name="groupPosition"></param>
			/// <param name="id"></param>
			/// <returns></returns>
			public bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
			{
				if ( parent.IsGroupExpanded( groupPosition ) == false )
				{
					// This group is expanding. If not previously displayed, get its contents now
					Artist artistClicked = Adapter.Artists[ groupPosition ];
					if ( artistClicked.ArtistAlbums == null )
					{
						Provider.ProvideArtistContents( artistClicked );
					}

					// Add this to the record of which groups are expanded
					expandedGroups.Add( groupPosition );

					lastGroupOpened = groupPosition;
				}
				else
				{
					expandedGroups.Remove( groupPosition );

					lastGroupOpened = -1;
				}

				// Report the new expanded count
				Provider.ExpandedGroupCountChanged( expandedGroups.Count );

				return false;
			}

			/// <summary>
			/// The Adapter used to get the group's contents
			/// </summary>
			public PlaylistsAdapter Adapter { get; set; }

			/// <summary>
			/// Interface used to obtain Artist details
			/// </summary>
			public IArtistContentsProvider Provider { get; set; }

			/// <summary>
			/// Keep track of the id's of the groups that have been expanded
			/// </summary>
			private HashSet< int > expandedGroups = new HashSet<int>();

			/// <summary>
			/// The last group expanded
			/// </summary>
			private int lastGroupOpened = -1;
		}

		/// <summary>
		/// Class used to listen out for clicks on group items
		/// </summary>
		private class ChildClickListener: Java.Lang.Object, ExpandableListView.IOnChildClickListener
		{
			public bool OnChildClick( ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id )
			{
				// Only process this if Action Mode is in effect
				if ( Adapter.ActionMode == true )
				{
					CheckBox selectionBox = clickedView.FindViewById<CheckBox>( Resource.Id.checkBox );

					selectionBox.Checked = !selectionBox.Checked;

					// Raise a click event to do the rest of the processing
					Adapter.SelectionBoxClick( selectionBox, new EventArgs() );
				}

				Toast.MakeText( parent.Context, string.Format( "Group {0}, Child {1}", groupPosition, childPosition ), ToastLength.Short ).Show();
				return false;
			}

			/// <summary>
			/// The Adapter used to get the group's contents
			/// </summary>
			public PlaylistsAdapter Adapter { get; set; }
		}

		/// <summary>
		/// Class used to listen for long clicks on the ListView items
		/// </summary>
		private class LongClickListener: Java.Lang.Object, AdapterView.IOnItemLongClickListener
		{
			public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
			{
				int tag = ( int )view.Tag;

				// If action mode is not in efect then request it.
				// Otherwise ignore long presses
				if ( Adapter.ActionMode == false )
				{
					Adapter.ActionMode = true;
					Adapter.EnteredActionMode?.Invoke( Adapter, new EventArgs() );
				}
				else
				{
					Adapter.ActionMode = false;
				}

				Adapter.NotifyDataSetChanged();

				Toast.MakeText( parent.Context, string.Format( "Long click Group {0}, Child {1}", tag >> 16, tag & 0x0ffff ), ToastLength.Short ).Show();
				return true;
			}

			public PlaylistsAdapter Adapter { get; set; }
		}
	}
}