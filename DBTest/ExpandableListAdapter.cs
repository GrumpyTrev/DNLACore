using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	abstract class ExpandableListAdapter< T > : BaseExpandableListAdapter
	{
		/// <summary>
		/// ExpandableListAdapter constructor. Set up a long click listener and the group expander helper class
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parentView"></param>
		/// <param name="provider"></param>
		public ExpandableListAdapter( Context context, ExpandableListView parentView, IGroupContentsProvider< T > provider, 
			ExpandableListAdapterModel model )
		{
			// Save the model
			adapterModel = model;

			// Save the inflator to use when creating the item views
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );

			// Set up listeners for group and child selection and item long click
			groupListener = new GroupClickListener() { Adapter = this, Provider = provider, Parent = parentView, Model = adapterModel.ExpansionModel };

			parentView.SetOnGroupClickListener( groupListener );
			parentView.SetOnChildClickListener( new ChildClickListener() { Adapter = this } );
			parentView.OnItemLongClickListener = new LongClickListener() { Adapter = this };
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
			convertView = GetSpecialisedGroupView( groupPosition, isExpanded, convertView, parent );

			// Tag the view with the group and child position
			convertView.Tag = ( groupPosition << 16 ) + 0x0FFFF;

			// Display the checkbox
			RenderCheckbox( convertView, groupPosition, 0x0FFFF );

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
				groupListener.RestoreExpansions();

				// Report if ActionMode is in effect
				if ( adapterModel.ActionMode == true )
				{
					EnteredActionMode?.Invoke( this, new EventArgs() );
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
						EnteredActionMode?.Invoke( this, new EventArgs() );
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
		/// Collapse any expanded groups
		/// </summary>
		public void OnCollapseRequest()
		{
			groupListener.OnCollapseRequest();
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
		/// Called when an item's checkbox has been selected
		/// Update the stored state for the item contained in the tag
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionBoxClick( object sender, EventArgs e )
		{
			CheckBox selectionBox = sender as CheckBox;
			int tag = ( int )( ( CheckBox )sender ).Tag;

			if ( adapterModel.CheckedObjects.Contains( tag ) == true )
			{
				adapterModel.CheckedObjects.Remove( tag );
			}
			else
			{
				adapterModel.CheckedObjects.Add( tag );
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
			selectionBox.Checked = ( adapterModel.CheckedObjects.Contains( ( int )selectionBox.Tag ) == true );

			// Trap checkbox clicks
			selectionBox.Click -= SelectionBoxClick;
			selectionBox.Click += SelectionBoxClick;
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
		/// The event used to indicate that Acion Mode has been entered
		/// </summary>
		public event EventHandler EnteredActionMode;

		/// <summary>
		/// Inflator used to create the item view 
		/// </summary>
		protected readonly LayoutInflater inflator = null;

		/// <summary>
		/// GroupClickListener instance used to handle the expansion and collapsing of artist entries 
		/// </summary>
		private readonly GroupClickListener groupListener = null;

		/// <summary>
		/// ExpandableListAdapterModel instance holding details of the UI state
		/// </summary>
		private readonly ExpandableListAdapterModel adapterModel = null;

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
					// This group is expanding. Get its contents
					Provider.ProvideGroupContents( Adapter.Groups[ groupPosition ] );

					// Add this to the record of which groups are expanded
					Model.ExpandedGroups.Add( groupPosition );

					Model.LastGroupOpened = groupPosition;
				}
				else
				{
					Model.ExpandedGroups.Remove( groupPosition );

					Model.LastGroupOpened = -1;
				}

				// Report the new expanded count
				Provider.ExpandedGroupCountChanged( Model.ExpandedGroups.Count );

				return false;
			}

			/// <summary>
			/// Called when a group collapse has been requested
			/// Either collapse the last group or all the groups
			/// </summary>
			public void OnCollapseRequest()
			{
				// Close either the last group opened or all groups
				if ( Model.LastGroupOpened != -1 )
				{
					Parent.CollapseGroup( Model.LastGroupOpened );
					Parent.SetSelection( Model.LastGroupOpened );
					Model.ExpandedGroups.Remove( Model.LastGroupOpened );
					Model.LastGroupOpened = -1;
				}
				else
				{
					// Close all open groups
					foreach ( int groupId in Model.ExpandedGroups )
					{
						Parent.CollapseGroup( groupId );
					}

					Model.ExpandedGroups.Clear();
				}

				// Report the new expanded count
				Provider.ExpandedGroupCountChanged( Model.ExpandedGroups.Count );
			}

			/// <summary>
			/// Expand all the groups that were previously expanded
			/// </summary>
			public void RestoreExpansions()
			{
				// Close all open groups
				foreach ( int groupId in Model.ExpandedGroups )
				{
					Parent.ExpandGroup( groupId );
				}

				// Report the new expanded count
				Provider.ExpandedGroupCountChanged( Model.ExpandedGroups.Count );
			}

			/// <summary>
			/// The Adapter used to get the group's contents
			/// </summary>
			public ExpandableListAdapter< T > Adapter { get; set; }

			/// <summary>
			/// Interface used to obtain Artist details
			/// </summary>
			public IGroupContentsProvider< T > Provider { get; set; }

			/// <summary>
			/// The parent ExpandableListView
			/// </summary>
			public ExpandableListView Parent { get; set; }

			/// <summary>
			/// GroupExpansionModel used to maintain details of which groups have been expanded
			/// </summary>
			public GroupExpansionModel Model { get; set; }
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
			public ExpandableListAdapter< T > Adapter { get; set; }
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
				}

				Toast.MakeText( parent.Context, string.Format( "Long click Group {0}, Child {1}", tag >> 16, tag & 0x0ffff ), ToastLength.Short ).Show();
				return true;
			}

			public ExpandableListAdapter< T > Adapter { get; set; }
		}
	}
}