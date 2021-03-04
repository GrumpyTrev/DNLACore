using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	public abstract class ExpandableListAdapter< T > : BaseExpandableListAdapter, AdapterView.IOnItemLongClickListener, 
		ExpandableListView.IOnChildClickListener, ExpandableListView.IOnGroupClickListener, ISectionIndexer
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
			inflator = LayoutInflater.FromContext( context );

			// Set up listeners for group and child selection and item long click
			parentView.SetOnGroupClickListener( this );
			parentView.SetOnChildClickListener( this );
			parentView.OnItemLongClickListener = this;
		}

		/// <summary>
		/// The number of groups
		/// </summary>
		public override int GroupCount => Groups.Count;

		/// <summary>
		/// Required by interface
		/// </summary>
		public override bool HasStableIds => false;

		/// <summary>
		/// Required by interface
		/// </summary>
		public override Java.Lang.Object GetChild( int groupPosition, int childPosition ) => null;

		/// <summary>
		/// Required by interface
		/// </summary>
		public override long GetChildId( int groupPosition, int childPosition ) => childPosition;

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

			// Save the position for this view
			( ( ExpandableListViewHolder )convertView.Tag ).ItemTag = FormChildTag( groupPosition, childPosition );

			// Display the checkbox
			RenderCheckbox( convertView );

			return convertView;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override Java.Lang.Object GetGroup( int groupPosition ) => null;

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override long GetGroupId( int groupPosition ) => groupPosition;

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

			// Save the position for this view
			( ( ExpandableListViewHolder )convertView.Tag ).ItemTag = FormGroupTag( groupPosition );

			// Display the checkbox
			RenderCheckbox( convertView );

			return convertView;
		}

		/// <summary>
		/// Are child items selectable
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		public override bool IsChildSelectable( int groupPosition, int childPosition ) => true;

		/// <summary>
		/// Update the data and associated sections displayed by the list view
		/// </summary>
		/// <param name="newData"></param>
		public void SetData( List< T > newData, SortSelector.SortType sortType )
		{
			// If this is the first time data has been set then restore group expansions and the Action Mode.
			// If data is being replaced then clear all state data related to the previous data
			// Only restore group expansions if there is any data to display
			if ( ( Groups.Count == 0 ) && ( newData.Count > 0 ) )
			{
				Groups = newData;

				// Expand any groups that were previously expanded
				foreach ( int groupId in adapterModel.ExpandedGroups )
				{
					parentView.ExpandGroup( groupId );
				}

				// Report the new expanded count
				contentsProvider.ExpandedGroupCountChanged( adapterModel.ExpandedGroups.Count );

				// Report the selection count
				stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );

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

			// Let the derived adapters initialise an group index
			SortType = sortType;
			SetGroupIndex();

			NotifyDataSetChanged();
		}

		/// <summary>
		/// Set or clear Action Mode.
		/// In Action Mode checkboxes appear alongside the items and items can be selected
		/// </summary>
		public bool ActionMode
		{
			get => adapterModel.ActionMode;
			set
			{
				// Action mode determines whether or not check boxes are shown so refresh the displayed items
				if ( adapterModel.ActionMode != value )
				{
					adapterModel.ActionMode = value;

					if ( adapterModel.ActionMode == true )
					{
						// Report to the fragment that the adapter has entered action mode
						stateChangeReporter.EnteredActionMode();
					}
					else
					{
						// Clear all selections when leaving Action Mode
						adapterModel.CheckedObjects.Clear();

						// Report that nothing is selected
						stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );
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

				GroupCollapseStateChanged( parentView, adapterModel.LastGroupOpened );

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

			// Need to reset the index whenever a group is expanded or collapsed
			SetGroupIndex();
		}

		/// <summary>
		/// Return the collection of selected items
		/// </summary>
		/// <returns></returns>
		public SortedDictionary<int, object> SelectedItems => adapterModel.CheckedObjects;

		/// <summary>
		/// Called when an item has been long clicked
		/// This is used to enter Action Mode.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="view"></param>
		/// <param name="position"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
		{
			ExpandableListViewHolder holder = ( ExpandableListViewHolder )view.Tag;

			// If action mode is not in effect then request it.
			// Otherwise ignore long presses
			if ( ActionMode == false )
			{
				ActionMode = true;

				// Let derived classes control what happens in addition to just turning on action mode 
				if ( SelectLongClickedItem( holder.ItemTag ) == true )
				{
					OnChildClick( parentView, view, GetGroupFromTag( holder.ItemTag ), GetChildFromTag( holder.ItemTag ), 0 );
				}
			}

			return true;
		}

		/// <summary>
		/// Called when a child item has been clicked.
		/// Toggle the selection state for the item
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool OnChildClick( ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id )
		{
			// Only process this if Action Mode is in effect
			if ( ActionMode == true )
			{
				CheckBox selectionBox = ( ( ExpandableListViewHolder )clickedView.Tag ).SelectionBox;

				selectionBox.Checked = !selectionBox.Checked;

				// Raise a click event to do the rest of the processing
				SelectionBoxClickAsync( selectionBox, new EventArgs() );
			}

			return false;
		}

		/// <summary>
		/// Called when a group item has been clicked
		/// Process the expansion or collapse request asynchronously
		/// Return true to prevent the group expansion/collapse from occuring
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			OnGroupClickAsync( parent, groupPosition );

			return true;
		}

		/// <summary>
		/// Get the starting position for a section
		/// </summary>
		/// <param name="sectionIndex"></param>
		/// <returns></returns>
		public virtual int GetPositionForSection( int sectionIndex ) => 0;

		/// <summary>
		/// Get the section that the specified position is in
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public virtual int GetSectionForPosition( int position ) => 0;

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public virtual Java.Lang.Object[] GetSections() => null;

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
		/// Get the data item at the specified position. If the childPosition is -1 then the group item is required
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected abstract object GetItemAt( int groupPosition, int childPosition );

		/// <summary>
		/// Select or deselect all the child items associated with the specified group
		/// Keep track of whether or not any items have changed - they should have but carry out the check anyway
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		protected virtual async Task<bool> SelectGroupContents( int groupPosition, bool selected )
		{
			bool selectionChanged = false;

			// If there are no child items associated with this group call the provider to get the children
			if ( GetChildrenCount( groupPosition ) == 0 )
			{
				// Get the group contents 
				await contentsProvider.ProvideGroupContentsAsync( Groups[ groupPosition ] );
			}

			for ( int childIndex = 0; childIndex < GetChildrenCount( groupPosition ); childIndex++ )
			{
				selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, childIndex ), selected );
			}

			return selectionChanged;
		}

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
				while ( ( childIndex < GetChildrenCount( groupPosition ) ) && ( IsItemSelected( FormChildTag( groupPosition, childIndex ) ) == true ) )
				{
					childIndex++;
				}

				// If all the children have been iterated then they must all be selected
				if ( childIndex == GetChildrenCount( groupPosition ) )
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
		protected bool IsItemSelected( int tag ) => adapterModel.CheckedObjects.ContainsKey( tag );

		/// <summary>
		/// Record the selection state of the specified item
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="select"></param>
		protected bool RecordItemSelection( int tag, bool select )
		{
			bool selectionChanged = false;

			// Find the object associated with the tag
			object item = GetItemAt( GetGroupFromTag( tag ), GetChildFromTag( tag ) );

			if ( select == true )
			{
				if ( adapterModel.CheckedObjects.ContainsKey( tag ) == false )
				{
					adapterModel.CheckedObjects.Add( tag, item );
					selectionChanged = true;
				}
			}
			else
			{
				if ( adapterModel.CheckedObjects.ContainsKey( tag ) == true )
				{
					adapterModel.CheckedObjects.Remove( tag );
					selectionChanged = true;
				}
			}

			return selectionChanged;
		}

		/// <summary>
		/// Update the selected items dictionary to reflect any items that have changed their positions
		/// </summary>
		/// <param name="items"></param>
		protected void UpdateSelectionTags( IEnumerable<(object value, int tag)> items )
		{
			// The selected items collection needs to be updated for items that have changed their positions
			// Keep track of selected items that have changed position
			Dictionary<int, object> newCheckedObjects = new Dictionary<int, object>();

			// Iterate through the collection of objects and their tags
			foreach ( (object value, int tag) in items )
			{
				// If this item is selected then check if its position has changed
				KeyValuePair<int, object> selectedItem = adapterModel.CheckedObjects.SingleOrDefault( pair => ( pair.Value == value ) );
				if ( selectedItem.Value != null )
				{
					if ( tag != selectedItem.Key )
					{
						// Remove the item from the selected items collection, but don't put it back in as it may now occupy the position of 
						// an item yet to be processed.
						adapterModel.CheckedObjects.Remove( selectedItem.Key );
						newCheckedObjects.Add( tag, value );
					}
				}
			}

			// Now put all the selected items that have changed position into the selected collection
			foreach ( KeyValuePair<int, object> newItem in newCheckedObjects )
			{
				adapterModel.CheckedObjects.Add( newItem.Key, newItem.Value );
			}

			// The change in selected item order may change what commands are available so report that the selection has changed
			stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );
		}

		/// <summary>
		/// Called when a group has been selected or deselected to allow derived classes to perform their own processing
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		protected virtual async Task<bool> GroupSelectionHasChanged( int groupPosition, bool selected ) => false;

		/// <summary>
		/// Called when the collapse state of a group has changed.
		/// Does nothing in the base class
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="groupPosition"></param>
		protected virtual void GroupCollapseStateChanged( ExpandableListView parent, int groupPosition )
		{
		}

		/// <summary>
		/// By default a long click just turns on Action Mode, but derived classes may wish to modify this behaviour
		/// </summary>
		/// <param name="tag"></param>
		protected virtual bool SelectLongClickedItem( int tag ) => false;

		/// <summary>
		/// Overriden in derived classes to generate an index for the group
		/// </summary>
		protected virtual void SetGroupIndex() {}

		/// <summary>
		/// Form a tag for a group item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		protected static int FormGroupTag( int groupPosition ) => ( groupPosition << 16 ) + 0xFFFF;

		/// <summary>
		/// Form a tag for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		protected static int FormChildTag( int groupPosition, int childPosition ) => ( groupPosition << 16 ) + childPosition;

		/// <summary>
		/// Return the child number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static int GetChildFromTag( int tag ) => ( tag & 0xFFFF );

		/// <summary>
		/// Does the tag represent a group
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static bool IsGroupTag( int tag ) => ( tag & 0xFFFF ) == 0xFFFF;

		/// <summary>
		/// Return the group number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		protected static int GetGroupFromTag( int tag ) => tag >> 16;

		protected CheckBox GetSelectionBox( View view )
		{
			CheckBox selectionBox = view.FindViewById<CheckBox>( Resource.Id.checkBox );
			if ( selectionBox != null )
			{
				selectionBox.Tag = new TagHolder();
				selectionBox.Click += SelectionBoxClickAsync;
			}

			return selectionBox;
		}

		/// <summary>
		/// Called to perform the actual group collapse or expansion asynchronously
		/// If a group is being expanded then get its contents if not previously displayed
		/// Keep track of which groups have been expanded and the last group expanded
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="groupPosition"></param>
		private async void OnGroupClickAsync( ExpandableListView parent, int groupPosition )
		{
			if ( parent.IsGroupExpanded( groupPosition ) == false )
			{
				// This group is expanding. Get its contents
				// If any content is supplied and the group is selected then select the new items
				int childCount = GetChildrenCount( groupPosition );

				await contentsProvider.ProvideGroupContentsAsync( Groups[ groupPosition ] );

				// Have any items been supplied
				if ( GetChildrenCount( groupPosition ) != childCount )
				{
					// If the group is selected then select the new items
					// N.B. This is not changing the selection state of the group so there is no need for derived classes
					// to do any group selection processing
					if ( IsItemSelected( FormGroupTag( groupPosition ) ) == true )
					{
						await SelectGroupContents( groupPosition, true );
					}
				}

				// Add this to the record of which groups are expanded
				adapterModel.ExpandedGroups.Add( groupPosition );

				adapterModel.LastGroupOpened = groupPosition;

				// Now expand the group
				parent.ExpandGroup( groupPosition );
			}
			else
			{
				adapterModel.ExpandedGroups.Remove( groupPosition );

				adapterModel.LastGroupOpened = -1;

				// Now collapse the group
				parent.CollapseGroup( groupPosition );
			}

			// Let the derived classes process the group's new state
			GroupCollapseStateChanged( parent, groupPosition );

			// Report the new expanded count
			contentsProvider.ExpandedGroupCountChanged( adapterModel.ExpandedGroups.Count );

			// Need to reset the index whenever a group is expanded or collapsed
			SetGroupIndex();
		}

		/// <summary>
		/// Called when an item's checkbox has been selected
		/// Update the stored state for the item contained in the tag
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void SelectionBoxClickAsync( object sender, EventArgs e )
		{
			int tag = ( ( TagHolder ) ( ( CheckBox )sender ).Tag  ).Tag;
			int groupPosition = GetGroupFromTag( tag );

			// Toggle the selection
			RecordItemSelection( tag, !IsItemSelected( tag ) );

			// Get the new selection state
			bool selected = IsItemSelected( tag );

			// Keep track of whether or not any other items are selected as this will require the NotifyDataSetChanged method to be called
			bool selectionChanged;

			// If this is a group item then select or deselect all of its children
			if ( IsGroupTag( tag ) == true )
			{
				selectionChanged = await SelectGroupContents( groupPosition, selected );

				// Let derived classes carry out any extra processing required due to the selection or deselection of a group
				selectionChanged |= await GroupSelectionHasChanged( groupPosition, selected );
			}
			else
			{
				// Determine how the selection or deselection of a child alters the selection state of the containing group
				selectionChanged = UpdateGroupSelectionState( groupPosition, GetChildFromTag( tag ), selected );

				// If the group selection has changed then tell the derived classes
				if ( selectionChanged == true )
				{
					selectionChanged |= await GroupSelectionHasChanged( groupPosition, selected );
				}
			}

			if ( selectionChanged == true )
			{
				NotifyDataSetChanged();
			}

			stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );
		}

		/// <summary>
		/// Show or hide the check box and sets its state from that held for the item
		/// </summary>
		/// <param name="convertView"></param>
		/// <param name="tag"></param>
		private void RenderCheckbox( View convertView )
		{
			ExpandableListViewHolder viewHolder = ( ExpandableListViewHolder )convertView.Tag;
			CheckBox selectionBox = viewHolder.SelectionBox;

			if ( selectionBox != null )
			{
				// Save the item identifier in the check box for the click event
				( ( TagHolder )selectionBox.Tag ).Tag = viewHolder.ItemTag;

				if ( ActionMode == true )
				{
					// Show the checkbox
					selectionBox.Visibility = ViewStates.Visible;

					// Retrieve the checked state of the item and set the checkbox state accordingly
					selectionBox.Checked = IsItemSelected( viewHolder.ItemTag );
				}
				else
				{
					// Hide the checkbox
					selectionBox.Visibility = ViewStates.Gone;
				}
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
			Task ProvideGroupContentsAsync( U theGroup );

			/// <summary>
			/// The number of expanded groups has changed
			/// </summary>
			/// <param name="count"></param>
			void ExpandedGroupCountChanged( int count );
		}

		/// <summary>
		/// The set of groups items displayed by the ExpandableListView
		/// </summary>
		protected List<T> Groups { get; set; } = new List<T>();

		/// <summary>
		/// The type of sorting applied to the data - used for indexing
		/// </summary>
		protected SortSelector.SortType SortType { get; set; } = SortSelector.SortType.alphabetic;

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
		protected readonly IGroupContentsProvider<T> contentsProvider = null;

		/// <summary>
		/// The parent ExpandableListView
		/// </summary>
		protected readonly ExpandableListView parentView = null;

		/// <summary>
		/// Interface used to report adapter state changes
		/// </summary>
		protected readonly IAdapterActionHandler stateChangeReporter = null;

		/// <summary>
		/// The section names sent back to the Java Adapter base class
		/// </summary>
		protected Java.Lang.Object[] javaSections = null;

		/// <summary>
		/// The TagHolder class holds the tag for the view's Checkbox
		/// An object holding an int is used rather that an int directly to avoid casting and Java object garbage collection problems (that may not
		/// actually exist)
		/// </summary>
		private class TagHolder : Java.Lang.Object
		{
			public int Tag { get; set; } = 0;
		}
	}
}