using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public class ArtistAlbumListViewAdapter: BaseExpandableListAdapter, ISectionIndexer
	{
		/// <summary>
		/// ArtistAlbumListViewAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public ArtistAlbumListViewAdapter( Context context, ExpandableListView parentView, IArtistContentsProvider provider )
		{
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );
			parentView.OnItemLongClickListener = new OnItemLongClickListener() { Adapter = this };
			groupExpander = new GroupExpander() { Adapter = this, Provider = provider, Parent = parentView };
			parentView.SetOnGroupClickListener( groupExpander );
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
			Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Group {0} Child {1} View {2}", groupPosition, childPosition,
				( convertView == null ) ? "NULL" : convertView.Handle.ToString() ) );

			// If no longer in ActionMode and this view previously has a click handler installed then use another view
			// If a view that has an handler is reused then even if the click handler is removed the long click event is not caught
			if ( ( ActionMode == false ) && ( hasClickHandler.Contains( convertView ) == true ) )
			{
				Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Removing view from hasClickHandler" ) );

				hasClickHandler.Remove( convertView );
				convertView.Click -= DetailItemClick;

				convertView = null;
			}

			// The child can be either a ArtistAlbum or a Song which use different layouts
			object childObject = Artists[ groupPosition ].Contents[ childPosition ];
			if ( ( childObject is ArtistAlbum ) == true )
			{
				// If the supplied view previously contained a Song then dispose of it
				if ( convertView != null )
				{
					if ( convertView.FindViewById<TextView>( Resource.Id.AlbumName ) == null )
					{
						Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "View contained a song so cannot use" ) );

						// Not an ArtistAlbum entry so make sure a new view is created
						convertView = null;
					}
				}

				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.album_layout, null );

					Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Using view {0}", convertView.Handle ) );
				}

				// Set the album text
				TextView albumName = convertView.FindViewById<TextView>( Resource.Id.AlbumName );
				TextView actionAlbumName = convertView.FindViewById<TextView>( Resource.Id.ActionAlbumName );

				if ( ActionMode == true )
				{
					albumName.Visibility = ViewStates.Gone;
					actionAlbumName.Visibility = ViewStates.Visible;
					actionAlbumName.Text = ( ( ArtistAlbum )childObject ).Name;
					actionAlbumName.Click -= TempDetailItemClick;

					actionAlbumName.Click += TempDetailItemClick;
				}
				else
				{
					actionAlbumName.Visibility = ViewStates.Gone;
					albumName.Visibility = ViewStates.Visible;
					albumName.Text = ( ( ArtistAlbum )childObject ).Name;
					actionAlbumName.Click -= TempDetailItemClick;
				}
				//				TextView albumName = convertView.FindViewById<TextView>( Resource.Id.AlbumName );
				//				albumName.Text = ( ( ArtistAlbum )childObject ).Name;

//				if ( ActionMode == true )
//				{
//					albumName.Click -= TempDetailItemClick;

//					albumName.Click += TempDetailItemClick;
//				}
//				else
//				{
//					albumName.Click -= TempDetailItemClick;
//				}
			}
			else
			{
				// If the supplied view previously contained an ArtistAlbum then dispose of it
				if ( convertView != null )
				{
					if ( convertView.FindViewById<TextView>( Resource.Id.Title ) == null )
					{
						Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "View contained an album so cannot use" ) );

						// Not a Song entry so make sure a new view is created
						convertView = null;
					}
				}

				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.song_layout, null );

					Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Using view {0}", convertView.Handle ) );
				}

				Song songItem = ( Song )childObject;

				// Display the Track number, Title and Duration
				convertView.FindViewById<TextView>( Resource.Id.Track ).Text = songItem.Track.ToString();

				TextView songName = convertView.FindViewById<TextView>( Resource.Id.Title );
				TextView actionSongName = convertView.FindViewById<TextView>( Resource.Id.ActionTitle );

				if ( ActionMode == true )
				{
					songName.Visibility = ViewStates.Gone;
					actionSongName.Visibility = ViewStates.Visible;
					actionSongName.Text = songItem.Title;
					actionSongName.Click -= TempDetailItemClick;

					actionSongName.Click += TempDetailItemClick;
				}
				else
				{
					actionSongName.Visibility = ViewStates.Gone;
					songName.Visibility = ViewStates.Visible;
					songName.Text = songItem.Title;
					actionSongName.Click -= TempDetailItemClick;
				}


				convertView.FindViewById<TextView>( Resource.Id.Duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );
			}

			// Tag the view with the group and child position
			convertView.Tag = ( groupPosition << 16 ) + childPosition;

			// Show or hide the common checkbox
			RenderCheckbox( convertView, groupPosition, childPosition );

			// If in ActionMode then trap any clicks on the item to treat as check box clicks
			if ( ActionMode == true )
			{
//				Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Adding click handler for view" ) );

//				convertView.Click -= DetailItemClick;
//				convertView.Click += DetailItemClick;

				// Record that an handler has been set on this view
//				if ( hasClickHandler.Contains( convertView ) == false )
//				{
//					Log.WriteLine( LogPriority.Debug, "DBTest:GetChildView", string.Format( "Recording that view has a click handler" ) );

//					hasClickHandler.Add( convertView );
//				}
			}

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
			Log.WriteLine( LogPriority.Debug, "DBTest:GetGroupView", string.Format( "Group {0} View {1}", groupPosition, 
				( convertView == null ) ? "NULL" : convertView.Handle.ToString() ) );

			// If no longer in ActionMode and this view previously has a click handler installed then use another view
			// If a view that has an handler is reused then even if the click handler is removed the long click event is not caught
			if ( ( ActionMode == false ) && ( hasClickHandler.Contains( convertView ) == true ) )
			{
				Log.WriteLine( LogPriority.Debug, "DBTest:GetGroupView", string.Format( "Removing view from hasClickHandler" ) );

				hasClickHandler.Remove( convertView );
				convertView = null;
			}

			if ( convertView != null )
			{
				// Check if the existing view contained artist details
				if ( convertView.FindViewById<TextView>( Resource.Id.ArtistName ) == null )
				{
					Log.WriteLine( LogPriority.Debug, "DBTest:GetGroupView", string.Format( "View did not contain an artist so cannot use" ) );

					// Not an Artist entry so make sure a new view is created
					convertView = null;
				}
			}

			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.artist_layout, null );

				Log.WriteLine( LogPriority.Debug, "DBTest:GetGroupView", string.Format( "Using view {0}", convertView.Handle ) );
			}

			// Display the artist's name
			convertView.FindViewById<TextView>( Resource.Id.ArtistName ).Text = Artists[ groupPosition ].Name;

			// Tag the view with the group and child position
			convertView.Tag = ( groupPosition << 16 ) + UInt16.MaxValue;

			// Show or hide the common checkbox
			RenderCheckbox( convertView, groupPosition, UInt16.MaxValue );

			return convertView;
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
			Artists = newData;
			NotifyDataSetChanged();
		}

		/// <summary>
		/// Collapse any expanded groups
		/// </summary>
		public void OnCollapseRequest()
		{
			groupExpander.OnCollapseRequest();
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

			// Remove the click handler before setting the state
			selectionBox.Click -= SelectionBoxClick;

			// Show or hide the checkbox
			selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;

			// Retrieve the cheked state of the item and set the checkbox state accordingly
			selectionBox.Checked = ( checkedObjects.Contains( ( int )selectionBox.Tag ) == true );

			Log.WriteLine( LogPriority.Debug, "DBTest:RenderCheckbox", string.Format( "Checkbox for view {0} id {1} checked", convertView.Handle,
				( selectionBox.Checked  == true ) ? "" : "NOT" ) );

			// Trap checkbox clicks
			selectionBox.Click += SelectionBoxClick;
		}

		/// <summary>
		/// Called when a song or album item is clicked when in Action Mode
		/// Simulate a checkbox click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DetailItemClick( object sender, EventArgs e )
		{
			int tag = ( int )( ( View )sender ).Tag;

			// Only interested in album and song entries, i.e. when the child is not 0xffff
			if ( ( tag & 0xFFFF ) != 0xFFFF )
			{
				// Treat this as a checkbox click
				CheckBox selectionBox = ( ( View )sender ).FindViewById<CheckBox>( Resource.Id.checkBox );

				selectionBox.Checked = !selectionBox.Checked;

				Log.WriteLine( LogPriority.Debug, "DBTest:DetailItemClick", string.Format( "Checkbox for view {0} id {1} checked",
					( ( View )sender ).Handle, ( selectionBox.Checked == true ) ? "" : "NOT" ) );

				// Raise a click event to do the rest of the processing
				SelectionBoxClick( selectionBox, new EventArgs() );
			}
		}

		/// <summary>
		/// Called when a song or album item is clicked when in Action Mode
		/// Simulate a checkbox click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TempDetailItemClick( object sender, EventArgs e )
		{
			TextView text = ( TextView )sender;

			View layout = ( View )text.Parent;

			int tag = ( int )layout.Tag;

			// Only interested in album and song entries, i.e. when the child is not 0xffff
			if ( ( tag & 0xFFFF ) != 0xFFFF )
			{
				// Treat this as a checkbox click
				CheckBox selectionBox = layout.FindViewById<CheckBox>( Resource.Id.checkBox );

				selectionBox.Checked = !selectionBox.Checked;

				Log.WriteLine( LogPriority.Debug, "DBTest:DetailItemClick", string.Format( "Checkbox for view {0} id {1} checked",
					( ( View )sender ).Handle, ( selectionBox.Checked == true ) ? "" : "NOT" ) );

				// Raise a click event to do the rest of the processing
				SelectionBoxClick( selectionBox, new EventArgs() );
			}
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
			int tag = ( int )selectionBox.Tag;

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
		/// The event used to indicate that entry to action mode has been requested
		/// </summary>
		public event EventHandler ActionModeRequested;

		/// <summary>
		/// The set of artists representing the groups displayed by the ExpandableListView
		/// </summary>
		public List<Artist> Artists { get; set; } = new List<Artist>();

		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		private HashSet< int > checkedObjects = new HashSet< int >();

		/// <summary>
		/// Keep track of which views have had a click handler installed.
		/// These views cannot be reused when Action Mode has finished because once an handler is installed it stops the 
		/// ListView long click detection from working (even if the handler is removed) Strange but true.
		/// </summary>
		private HashSet< View > hasClickHandler = new HashSet<View>();

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		private bool actionMode = false;

		/// <summary>
		/// Inflator used to create the item view 
		/// </summary>
		private readonly LayoutInflater inflator = null;

		/// <summary>
		/// GroupExpander instance used to handle the expansion and collapsing of artist entries 
		/// </summary>
		private readonly GroupExpander groupExpander = null;

		/// <summary>
		/// Lookup table specifying the starting position for each section name
		/// </summary>
		private Dictionary<string, int> alphaIndexer = null;

		/// <summary>
		/// List of section names
		/// </summary>
		private string[] sections = null;

		/// <summary>
		/// Class used to listen for long clicks on the ListView items
		/// </summary>
		private class OnItemLongClickListener: Java.Lang.Object, AdapterView.IOnItemLongClickListener
		{
			public ArtistAlbumListViewAdapter Adapter { get; set; }

			/// <summary>
			/// Called when a long click has been detected 
			/// If ActionMode is not in progress initiate it.
			/// </summary>
			/// <param name="parent"></param>
			/// <param name="view"></param>
			/// <param name="position"></param>
			/// <param name="id"></param>
			/// <returns></returns>
			public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
			{
				// If action mode is not in efect then request it.
				// Otherwise ignore long presses
				if ( Adapter.ActionMode == false )
				{
					Adapter.ActionModeRequested?.Invoke( Adapter, new EventArgs() );
				}

				return true;
			}
		}

		/// <summary>
		/// Class used to listen out for clicks on group items
		/// </summary>
		private class GroupExpander: Java.Lang.Object, ExpandableListView.IOnGroupClickListener
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
			/// Called when a group collapse has been requested
			/// Either collapse the last group or all the groups
			/// </summary>
			public void OnCollapseRequest()
			{
				// Close either the last group opened or all groups
				if ( lastGroupOpened != -1 )
				{
					Parent.CollapseGroup( lastGroupOpened );
					Parent.SetSelection( lastGroupOpened );
					expandedGroups.Remove( lastGroupOpened );
					lastGroupOpened = -1;
				}
				else
				{
					// Close all open groups
					foreach ( int groupId in expandedGroups )
					{
						Parent.CollapseGroup( groupId );
					}

					expandedGroups.Clear();
				}

				// Report the new expanded count
				Provider.ExpandedGroupCountChanged( expandedGroups.Count );
			}

			/// <summary>
			/// The Adapter used to get the group's contents
			/// </summary>
			public ArtistAlbumListViewAdapter Adapter { get; set; }

			/// <summary>
			/// Interface used to obtain Artist details
			/// </summary>
			public IArtistContentsProvider Provider { get; set; }

			/// <summary>
			/// The parent ExpandableListView
			/// </summary>
			public ExpandableListView Parent { get; set; }

			/// <summary>
			/// Keep track of the id's of the groups that have been expanded
			/// </summary>
			private HashSet< int > expandedGroups = new HashSet<int>();

			/// <summary>
			/// The last group expanded
			/// </summary>
			private int lastGroupOpened = -1;
		}
	}
}