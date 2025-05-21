using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	public abstract class ExpandableListAdapter<T> : BaseExpandableListAdapter, AdapterView.IOnItemLongClickListener,
		ExpandableListView.IOnChildClickListener, ExpandableListView.IOnGroupClickListener, ISectionIndexer, AbsListView.IOnScrollListener
	{
		/// <summary>
		/// ExpandableListAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="view"></param>
		/// <param name="provider"></param>
		public ExpandableListAdapter( Context context, ExpandableListView view, IGroupContentsProvider<T> provider,
			ExpandableListAdapterModel model, IAdapterEventHandler stateChange )
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
			parentView.SetOnScrollListener( this );
		}

		/// <summary>
		/// The number of groups
		/// Required by interface
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
		/// Provide a view for a group item at the specified position
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			convertView = GetSpecialisedGroupView( groupPosition, isExpanded, convertView, parent );

			// Save the position for this view in the view holder
			( ( ExpandableListViewHolder )convertView.Tag ).ItemTag = FormGroupTag( groupPosition );

			// Set the view's background. This can be overriden by specialised classes
			RenderBackground( convertView );

			return convertView;
		}

		/// <summary>
		/// Provide a view for a child item at the specified position
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

			// Save the position for this view in the view holder
			( ( ExpandableListViewHolder )convertView.Tag ).ItemTag = FormChildTag( groupPosition, childPosition );

			// Set the view's background. This can be overriden by specialised classes
			RenderBackground( convertView );

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
		public void SetData( List<T> newData, SortType sortType, List<Tuple<string, int>> fastScrollSectionsList = null, int[] fastScrollSectionIndex = null )
		{
			fastScrollSections = fastScrollSectionsList;
			FastScrollSectionLookup = fastScrollSectionIndex;

			// If this is the first time data has been set then restore group expansions and the Action Mode.
			// If data is being replaced then clear all state data related to the previous data
			// Only restore group expansions if there is any data to display
			if ( ( Groups.Count == 0 ) && ( newData.Count > 0 ) )
			{
				Groups = newData;

				// Expand any group that was previously expanded
				if ( adapterModel.LastGroupOpened != -1 )
				{
					_ = parentView.ExpandGroup( adapterModel.LastGroupOpened );
				}

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
		/// Return the collection of selected items
		/// </summary>
		/// <returns></returns>
		public SortedDictionary<int, object> SelectedItems => adapterModel.CheckedObjects;

		/// <summary>
		/// Called when an item has been long clicked
		/// This is used to enter Action Mode and to select the item being clicked
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
			if ( ActionMode == false )
			{
				ActionMode = true;
			}

			// Let derived classes control what happens in addition to just turning on action mode 
			if ( SelectLongClickedItem( holder.ItemTag ) == true )
			{
				_ = OnChildClick( parentView, view, GetGroupFromTag( holder.ItemTag ), GetChildFromTag( holder.ItemTag ), 0 );
			}

			// Report this interaction
			UserActivityDetected();

			return true;
		}

		/// <summary>
		/// Called when a child item has been clicked. This is also called when a group items has been long-clicked.
		/// If Action Mode is in effect toggle the selection state for the item
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual bool OnChildClick( ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id )
		{
			// Only process this if Action Mode is in effect
			if ( ActionMode == true )
			{
				ItemSelected( FormChildTag( groupPosition, childPosition ) );
			}

			// Report this interaction
			UserActivityDetected();

			return false;
		}

		/// <summary>
		/// Called when a group item has been clicked
		/// Process the expansion or collapse request
		/// Return true to prevent the group expansion/collapse from being carried out by the base class
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="clickedView"></param>
		/// <param name="groupPosition"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual bool OnGroupClick( ExpandableListView parent, View clickedView, int groupPosition, long id )
		{
			OnGroupClick( parent, groupPosition );

			// Report this interaction
			UserActivityDetected();

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
		public int GetSectionForPosition( int position ) => ( FastScrollSectionLookup == null ) ? 0 : FastScrollSectionLookup[ Math.Min( position, FastScrollSectionLookup.Length - 1 ) ];

		/// <summary>
		/// Return the names of all the sections
		/// </summary>
		/// <returns></returns>
		public virtual Java.Lang.Object[] GetSections() => null;

		/// <summary>
		/// Called when the list view has been scrolled.
		/// This is not required for activity detection, but is part of the IOnScrollListener interface
		/// </summary>
		/// <param name="view"></param>
		/// <param name="firstVisibleItem"></param>
		/// <param name="visibleItemCount"></param>
		/// <param name="totalItemCount"></param>
		public void OnScroll( AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount ) {}

		/// <summary>
		/// Called when the scroll has started or stopped. Report user action
		/// </summary>
		/// <param name="view"></param>
		/// <param name="scrollState"></param>
		public void OnScrollStateChanged( AbsListView view, [GeneratedEnum] ScrollState scrollState ) => UserActivityDetected();

		/// <summary>
		/// The Action to call when user activity is detected
		/// </summary>
		public Action UserActivityDetectedAction { get; set; } = null;

		/// <summary>
		/// Is the user currently interacting with the view
		/// </summary>
		public bool IsUserActive
		{
			get => isUserActive;
			set
			{
				if ( isUserActive != value )
				{
					isUserActive = value;
					UserActivityChanged();
				}
			}
		}

		/// <summary>
		/// Interface that classes providing group details must implement.
		/// </summary>
		public interface IGroupContentsProvider<U>
		{
			/// <summary>
			/// Provide the details for the specified group
			/// </summary>
			/// <param name="theGroup"></param>
			void ProvideGroupContents( U theGroup );
		}

		/// <summary>
		/// Called when some user activity has been detected
		/// </summary>
		protected void UserActivityDetected() => UserActivityDetectedAction?.Invoke();

		/// <summary>
		/// Called when the user activity has changed state
		/// </summary>
		protected virtual void UserActivityChanged() {}

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
		protected bool SelectGroupContents( int groupPosition, bool selected )
		{
			bool selectionChanged = false;

			// If there are no child items associated with this group call the provider to get the children
			if ( GetChildrenCount( groupPosition ) == 0 )
			{
				// Get the group contents 
				contentsProvider.ProvideGroupContents( Groups[ groupPosition ] );
			}

			for ( int childIndex = 0; childIndex < GetChildrenCount( groupPosition ); childIndex++ )
			{
				selectionChanged |= RecordItemSelection( FormChildTag( groupPosition, childIndex ), selected );
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

			if ( select == true )
			{
				if ( adapterModel.CheckedObjects.ContainsKey( tag ) == false )
				{
					adapterModel.CheckedObjects.Add( tag, GetItemAt( GetGroupFromTag( tag ), GetChildFromTag( tag ) ) );
					selectionChanged = true;
				}
			}
			else
			{
				if ( adapterModel.CheckedObjects.ContainsKey( tag ) == true )
				{
					_ = adapterModel.CheckedObjects.Remove( tag );
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
			Dictionary<int, object> newCheckedObjects = [];

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
						_ = adapterModel.CheckedObjects.Remove( selectedItem.Key );
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
		protected virtual bool GroupSelectionHasChanged( int groupPosition, bool selected ) => false;

		/// <summary>
		/// Called when the collapse state of a group has changed.
		/// Does nothing in the base class
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="groupPosition"></param>
		protected virtual void GroupCollapseStateChanged( ExpandableListView parent, int groupPosition ) {}

		/// <summary>
		/// By default a long click turns on Action mode and selects the item clicked. 
		/// Let specialised classes prevent the item being selected
		/// </summary>
		/// <param name="tag"></param>
		protected virtual bool SelectLongClickedItem( int tag ) => true;

		/// <summary>
		/// This is called when new data has been provided in order to create some of the fast scroll data.
		/// Most of this has already been done in the controllers.
		/// All that is missing is the copying of the section names into an array of Java strings
		/// </summary>
		protected void SetGroupIndex()
		{
			// Clear the array first in case there are none
			javaSections = null;

			if ( fastScrollSections != null )
			{
				// Size the section array from the ArtistsViewModel.FastScrollSections
				javaSections = new Java.Lang.Object[ fastScrollSections.Count ];
				for ( int index = 0; index < javaSections.Length; ++index )
				{
					javaSections[ index ] = new Java.Lang.String( fastScrollSections[ index ].Item1 );
				}
			}
		}

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

		/// <summary>
		/// The FastScrollSectionLookup provided by the derived adapter
		/// </summary>
		/// <returns></returns>
		protected virtual int[] FastScrollSectionLookup { get; set; } = null;

		/// <summary>
		/// Change the view's background according to it's selection state
		/// </summary>
		protected virtual void RenderBackground( View convertView )
		{
			ExpandableListViewHolder viewHolder = ( ExpandableListViewHolder )convertView.Tag;

			// If the item is selected then set the background to the selected colour.
			if ( IsItemSelected( viewHolder.ItemTag ) == true )
			{
				SetSelectedBackground( convertView );
			}
			else
			{
				// If this is a group item then check if any of it's children are selected
				if ( IsGroupTag( viewHolder.ItemTag ) == true )
				{
					if ( AnyChildSelected( viewHolder.ItemTag ) == true )
					{
						// At least one child is selected
						SetPartialBackground( convertView );
					}
					else
					{
						// No child items selected
						SetUnselectedBackground( convertView );
					}
				}
				else
				{
					SetUnselectedBackground( convertView );
				}
			}
		}

		/// <summary>
		/// Are any of the child items associated with a group selected
		/// </summary>
		/// <param name="tag">The tag of the group</param>
		/// <returns></returns>
		protected bool AnyChildSelected( int tag )
		{
			int groupPosition = GetGroupFromTag( tag );
			int childIndex = 0;
			while ( ( childIndex < GetChildrenCount( groupPosition ) ) && ( IsItemSelected( FormChildTag( groupPosition, childIndex ) ) == false ) )
			{
				childIndex++;
			}

			return ( childIndex < GetChildrenCount( groupPosition ) );
		}

		/// <summary>
		/// Set the background of the item to the standard 'selected' colour
		/// </summary>
		/// <param name="itemView"></param>
		protected void SetSelectedBackground( View itemView ) => itemView.SetBackgroundColor( Color.PeachPuff );

		/// <summary>
		/// Set the background of the item to the standard 'unselected' colour
		/// </summary>
		/// <param name="itemView"></param>
		protected void SetUnselectedBackground( View itemView ) => itemView.SetBackgroundColor( Color.Transparent );

		/// <summary>
		/// Set the background of the item to the standard 'partial' resource
		/// </summary>
		/// <param name="itemView"></param>
		protected void SetPartialBackground( View itemView ) => itemView.SetBackgroundResource( Resource.Drawable.tiled_background );

		/// <summary>
		/// The set of groups items displayed by the ExpandableListView
		/// </summary>
		protected List<T> Groups { get; set; } = [];

		/// <summary>
		/// The type of sorting applied to the data - used for indexing
		/// </summary>
		protected SortType SortType { get; set; } = SortType.alphabetic;

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
		protected readonly IAdapterEventHandler stateChangeReporter = null;

		/// <summary>
		/// The section names sent back to the Java Adapter base class
		/// </summary>
		protected Java.Lang.Object[] javaSections = null;

		/// <summary>
		/// Lookup table specifying the strings used when fast scrolling, and the index into the data collection
		/// </summary>
		protected List<Tuple<string, int>> fastScrollSections = null;

		/// <summary>
		/// The base implementation selects or deselects the containing group according to the state of its children
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="selected"></param>
		/// 
		private bool UpdateGroupSelectionState( int groupPosition, bool selected )
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
		/// Called to perform the actual group collapse or expansion
		/// If a group is being expanded then get its contents if not previously displayed
		/// Keep track of which groups have been expanded and the last group expanded
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="groupPosition"></param>
		private void OnGroupClick( ExpandableListView parent, int groupPosition )
		{
			if ( parent.IsGroupExpanded( groupPosition ) == false )
			{
				// If there is a group already expanded then collapse it now
				if ( adapterModel.LastGroupOpened != -1 )
				{
					// Attempt to prevent the collapsing of a group prior to the one being expanded changing the 
					// item at the top of the view
					if ( adapterModel.LastGroupOpened < groupPosition )
					{
						// The item at the top of the view should be reduced by the number of children in the group being collapsed 
						int requiredPosition = parent.FirstVisiblePosition - GetChildrenCount( adapterModel.LastGroupOpened );

						// Collapse the group and then scroll to set the new position
						_ = parent.CollapseGroup( adapterModel.LastGroupOpened );

						parent.SmoothScrollToPositionFromTop( Math.Max( requiredPosition, 0 ), 0, 0 );
					}
					else
					{
						// If the group being collapsed is after the one being expanded then the poistion of the list will not change
						_ = parent.CollapseGroup( adapterModel.LastGroupOpened );
					}
				}

				// This group is expanding. Get its contents
				// If any content is supplied and the group is selected then select the new items
				int childCount = GetChildrenCount( groupPosition );

				contentsProvider.ProvideGroupContents( Groups[ groupPosition ] );

				// Have any items been supplied
				if ( GetChildrenCount( groupPosition ) != childCount )
				{
					// If the group is selected then select the new items
					// N.B. This is not changing the selection state of the group so there is no need for derived classes
					// to do any group selection processing
					if ( IsItemSelected( FormGroupTag( groupPosition ) ) == true )
					{
						_ = SelectGroupContents( groupPosition, true );
					}
				}

				adapterModel.LastGroupOpened = groupPosition;

				// Now expand the group
				_ = parent.ExpandGroup( groupPosition );
			}
			else
			{
				adapterModel.LastGroupOpened = -1;

				// Now collapse the group
				_ = parent.CollapseGroup( groupPosition );
			}

			// Let the derived classes process the group's new state
			GroupCollapseStateChanged( parent, groupPosition );

			// Need to reset the index whenever a group is expanded or collapsed
			SetGroupIndex();
		}

		/// <summary>
		/// Called when an item has been selected in Action mode
		/// </summary>
		/// <param name="tag">Identity of the selected item</param>
		private void ItemSelected( int tag )
		{
			// Toggle the selection
			_ = RecordItemSelection( tag, !IsItemSelected( tag ) );

			// Get the new selection state
			bool selected = IsItemSelected( tag );

			int groupPosition = GetGroupFromTag( tag );

			// If this is a group item then select or deselect all of its children
			if ( IsGroupTag( tag ) == true )
			{
				_ = SelectGroupContents( groupPosition, selected );

				// Let derived classes carry out any extra processing required due to the selection or deselection of a group
				_ = GroupSelectionHasChanged( groupPosition, selected );
			}
			else
			{
				// Determine how the selection or deselection of a child alters the selection state of the containing group
				// If the group selection has changed then tell the derived classes
				if ( UpdateGroupSelectionState( groupPosition, selected ) == true )
				{
					_ =	GroupSelectionHasChanged( groupPosition, selected );
				}
			}

			// A selection always requires a redraw
			NotifyDataSetChanged();

			// Report any changes to the set of selected items
			stateChangeReporter.SelectedItemsChanged( adapterModel.CheckedObjects );

			// Report this interaction
			UserActivityDetected();
		}

		/// <summary>
		/// Data for the IsUserActive property
		/// </summary>
		private bool isUserActive = false;

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
