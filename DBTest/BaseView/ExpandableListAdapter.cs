using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public abstract class ExpandableListAdapter< T > : BaseExpandableListAdapter, AdapterView.IOnItemLongClickListener, 
		ExpandableListView.IOnChildClickListener, ExpandableListView.IOnGroupClickListener
	{
		/// <summary>
		/// ExpandableListAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="view"></param>
		/// <param name="provider"></param>
		public ExpandableListAdapter( Context context, ExpandableListView view, IGroupContentsProvider< T > provider, 
			ExpandableListAdapterModel model, IAdapterActionHandler stateChange )
		{
			// Save the parameters
			adapterModel = model;
			contentsProvider = provider;
			parentView = view;
			stateChangeReporter = stateChange;

			// Save the inflator to use when creating the item views
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );

			// Set up listeners for group and child selection and item long click
			parentView.SetOnGroupClickListener( this );
			parentView.SetOnChildClickListener( this );
			parentView.OnItemLongClickListener = this;
		}

		/// <summary>
		/// The number of groups
		/// </summary>
		public override int GroupCount
		{
			get
			{
				return Groups.Count;
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
		/// Number of child items of selected group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public abstract override int GetChildrenCount( int groupPosition );

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
			convertView = GetSpecialisedChildView( groupPosition, childPosition, isLastChild, convertView, parent );

			// Tag the view with the group and child position
			convertView.Tag = FormChildTag( groupPosition, childPosition );

			// Display the checkbox
			RenderCheckbox( convertView, ( int )convertView.Tag );

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
			convertView = GetSpecialisedGroupView( groupPosition, isExpanded, convertView, parent );

			// Tag the view with the group and child position
			convertView.Tag = FormGroupTag( groupPosition );

			// Display the checkbox
			RenderCheckbox( convertView, ( int )convertView.Tag );

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
		/// Update the data and associated sections displayed by the list view
		/// </summary>
		/// <param name="newData"></param>
		public void SetData( List< T > newData )
		{
			// If this is the first time data has been set then restore group expansions and the Action Mode.
			// If data is being replaced then clear all state data related to the previous data
			if ( Groups.Count == 0 )
			{
				Groups = newData;

				// Expand any groups that were previously expanded
				foreach ( int groupId in adapterModel.ExpandedGroups )
				{
					parentView.ExpandGroup( groupId );
				}

				// Report the new expanded count
				contentsProvider.ExpandedGroupCountChanged( adapterModel.ExpandedGroups.Count );

				// Report if ActionMode is in effect
				if ( adapterModel.ActionMode == true )
				{
					stateChangeReporter.EnteredActionMode();
				}
			}
			else
			{
				Groups = newData;
				adapterModel.OnClear();
			}

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
				return adapterModel.ActionMode;
			}
			set
			{
				// Action mode determines whether or not check boxes are shown so refresh the displayed items
				if ( adapterModel.ActionMode != value )
				{
					adapterModel.ActionMode = value;

					if ( adapterModel.ActionMode == true )
					{
						stateChangeReporter.EnteredActionMode();
					}
					else
					{
						// Clear all selections when leaving Action Mode
						adapterModel.CheckedObjects.Clear();
					}

					NotifyDataSetChanged();
				}
			}
		}

		/// <summary>
		/// Called when a group collapse has been requested
		/// Either collapse the last group or all the groups
		/// </summary>
		public void OnCollapseRequest()
		{
			// Close either the last group opened or all groups
			if ( adapterModel.LastGroupOpened != -1 )
			{
				parentView.CollapseGroup( adapterModel.LastGroupOpened );
				parentView.SetSelection( adapterModel.LastGroupOpened );
				adapterModel.ExpandedGroups.Remove( adapterModel.LastGroupOpened );
				adapterModel.LastGroupOpened = -1;
			}
			else
			{
				// Close all open groups
				foreach ( int groupId in adapterModel.ExpandedGroups )
				{
					parentView.CollapseGroup( groupId );
				}

				adapterModel.LastGroupOpened = -1;
				adapterModel.ExpandedGroups.Clear();
			}

			// Report the new expanded count
			contentsProvider.ExpandedGroupCountChanged( adapterModel.ExpandedGroups.Count );
		}

		/// <summary>
		/// Form a list of all the currently selected items sorted by their tags
		/// </summary>
		/// <returns></returns>
		public List<object> GetSelectedItems()
		{
			List<object> selectedItems = new List<object>();

			// Copy the selected tags from the hashset
			int[] tags = new int[ adapterModel.CheckedObjects.Count ];
			adapterModel.CheckedObjects.CopyTo( tags );

			// Sort by numeric tag order
			Array.Sort( tags );

			foreach ( int tag in tags )
			{
				object taggedObject = FilteredSelection( tag );
				if ( taggedObject != null )
				{
					selectedItems.Add( taggedObject );
				}
			}

			return selectedItems;
		}

		public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
		{
			int tag = ( int )view.Tag;

			// If action mode is not in efect then request it.
			// Otherwise ignore long presses
			if ( ActionMode == false )
			{
				ActionMode = true;
			}

			Toast.MakeText( parent.Context, string.Format( "Long click Group {0}, Child {1}", GetGroupFromTag( tag ), GetChildFromTag( tag ) ),
				ToastLength.Short ).Show();
			return true;
		}

		public bool OnChildClick( ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id )
		{
			// Only process this if Action Mode is in effect
			if ( ActionMode == true )
			{
				CheckBox selectionBox = clickedView.FindViewById<CheckBox>( Resource.Id.checkBox );

				selectionBox.Checked = !selectionBox.Checked;

				// Raise a click event to do the rest of the processing
				SelectionBoxClick( selectionBox, new EventArgs() );
			}

			Toast.MakeText( parent.Context, string.Format( "Group {0}, Child {1}", groupPosition, childPosition ), ToastLength.Short ).Show();
			return false;
		}

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
		public virtual bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			if ( parent.IsGroupExpanded( groupPosition ) == false )
			{
				// This group is expanding. Get its contents
				// If any content is supplied and the group is selected then select the new items
				int childCount = GetChildrenCount( groupPosition );

				contentsProvider.ProvideGroupContents( Groups[ groupPosition ] );

				// Have any items been supplied
				if ( GetChildrenCount( groupPosition ) != childCount )
				{
					// If the group is selected then select the new items
					if ( IsItemSelected( FormGroupTag( groupPosition ) ) == true )
					{
						SelectGroupContents( groupPosition, true );
					}
				}

				// Add this to the record of which groups are expanded
				adapterModel.ExpandedGroups.Add( groupPosition );

				adapterModel.LastGroupOpened = groupPosition;
			}
			else
			{
				adapterModel.ExpandedGroups.Remove( groupPosition );

				adapterModel.LastGroupOpened = -1;
			}

			// Report the new expanded count
			contentsProvider.ExpandedGroupCountChanged( adapterModel.ExpandedGroups.Count );

			return false;
		}

		/// <summary>
		/// Derived classes must implement this method to provide a view for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="isLastChild"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected abstract View GetSpecialisedChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent );

		/// <summary>
		/// Derived classes must implement this method to provide a view for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		protected abstract View GetSpecialisedGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent );

		/// <summary>
		/// The base implementation selects or deselects the containing group according to the state of its children
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="selected"></param>
		protected virtual bool UpdateGroupSelectionState( int groupPosition, int childPosition, bool selected )
		{
			// Keep track of whether the group selection state has changed
			bool selectionChanged = false;

			// If a child is deselected then deselect the group item
			if ( selected == false )
			{
				selectionChanged = RecordItemSelection( FormGroupTag( groupPosition ), false );
			}
			else
			{
				// If all of the child items are now selected then select the group as well
				int childIndex = 0;
				bool allSelected = true;
				while ( ( allSelected == true ) && ( childIndex < GetChildrenCount( groupPosition ) ) )
				{
					if ( IsItemSelected( FormChildTag( groupPosition, childIndex ) ) == false )
					{
						allSelected = false;
					}

					childIndex++;
				}

				if ( allSelected == true )
				{
					selectionChanged = RecordItemSelection( FormGroupTag( groupPosition ), true );
				}
			}

			return selectionChanged;
		}

		/// <summary>
		/// Is the specified item selected
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected bool IsItemSelected( int tag )
		{
			return adapterModel.CheckedObjects.Contains( tag );
		}

		/// <summary>
		/// Record the selection state of the specified item
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="select"></param>
		protected bool RecordItemSelection( int tag, bool select )
		{
			return ( select == true ) ? adapterModel.CheckedObjects.Add( tag ) : adapterModel.CheckedObjects.Remove( tag );
		}

		/// <summary>
		/// Can the specified object be included in operations on the selected items 
		/// </summary>
		/// <param name="selectedObject"></param>
		/// <returns></returns>
		protected virtual object FilteredSelection( int tag )
		{
			return null;
		}

		/// <summary>
		/// Form a tag for a group item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		protected static int FormGroupTag( int groupPosition )
		{
			return ( groupPosition << 16 ) + 0xFFFF;
		}

		/// <summary>
		/// Form a tag for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected static int FormChildTag( int groupPosition, int childPosition )
		{
			return ( groupPosition << 16 ) + childPosition;
		}

		/// <summary>
		/// Return the child number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static int GetChildFromTag( int tag )
		{
			return ( tag & 0xFFFF );
		}

		/// <summary>
		/// Does the tag represent a group
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static bool IsGroupTag( int tag )
		{
			return ( tag & 0xFFFF ) == 0xFFFF;
		}

		/// <summary>
		/// Return the group number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static int GetGroupFromTag( int tag )
		{
			return tag >> 16;
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
			int groupPosition = GetGroupFromTag( tag );

			// Toggle the selection
			RecordItemSelection( tag, !IsItemSelected( tag ) );

			// Keep track of whether or not any other items are selected as this will require the NotifyDataSetChanged method to be called
			bool selectionChanged = false;

			// If this is a group item then select or deselect all of its children
			if ( IsGroupTag( tag ) == true )
			{
				selectionChanged = SelectGroupContents( groupPosition, IsItemSelected( tag ) );
			}
			else
			{
				// Determine how the selection or deselection of a child alters the selection state of the containing group
				selectionChanged = UpdateGroupSelectionState( groupPosition, GetChildFromTag( tag ), IsItemSelected( tag ) );
			}

			if ( selectionChanged == true )
			{
				NotifyDataSetChanged();
			}

			stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects.Count );
		}

		/// <summary>
		/// Select or deselect all the child items associated with the specified group
		/// Keep track of whether or not any items have changed - they should have but carry out hte check anyway
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		private bool SelectGroupContents( int groupPosition, bool selected )
		{
			bool selectionChanged = false;

			for ( int childIndex = 0; childIndex < GetChildrenCount( groupPosition ) ; childIndex++ )
			{
				selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, childIndex ), selected );
			}

			return selectionChanged;
		}

		/// <summary>
		/// Show or hide the check box and sets its state from that held for the item
		/// </summary>
		/// <param name="convertView"></param>
		/// <param name="tag"></param>
		private void RenderCheckbox( View convertView, int tag )
		{
			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );

			if ( selectionBox != null )
			{
				// Save the item identifier in the check box for the click event
				selectionBox.Tag = tag;

				// Show or hide the checkbox
				selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;

				// Retrieve the cheked state of the item and set the checkbox state accordingly
				selectionBox.Checked = IsItemSelected( tag );

				// Trap checkbox clicks
				selectionBox.Click -= SelectionBoxClick;
				selectionBox.Click += SelectionBoxClick;
			}
		}

		/// <summary>
		/// Interface that classes providing group details must implement.
		/// </summary>
		public interface IGroupContentsProvider< U >
		{
			/// <summary>
			/// Provide the details for the specified group
			/// </summary>
			/// <param name="theGroup"></param>
			void ProvideGroupContents( U theGroup );

			/// <summary>
			/// The number of expanded groups has changed
			/// </summary>
			/// <param name="count"></param>
			void ExpandedGroupCountChanged( int count );
		}

		/// <summary>
		/// The set of groups items displayed by the ExpandableListView
		/// </summary>
		public List< T > Groups { get; set; } = new List< T >();

		/// <summary>
		/// Inflator used to create the item view 
		/// </summary>
		protected readonly LayoutInflater inflator = null;

		/// <summary>
		/// ExpandableListAdapterModel instance holding details of the UI state
		/// </summary>
		protected readonly ExpandableListAdapterModel adapterModel = null;

		/// <summary>
		/// Interface used to obtain Artist details
		/// </summary>
		private readonly IGroupContentsProvider<T> contentsProvider = null;

		/// <summary>
		/// The parent ExpandableListView
		/// </summary>
		private readonly ExpandableListView parentView = null;

		/// <summary>
		/// Interface used to report adapter state changes
		/// </summary>
		private readonly IAdapterActionHandler stateChangeReporter = null;
	}
}