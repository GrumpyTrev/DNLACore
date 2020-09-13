using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The FilterGroupAdapter is used in to display tag group selection data using an ExpandableListView
	/// </summary>
	public class FilterGroupAdapter : BaseExpandableListAdapter, ExpandableListView.IOnChildClickListener
	{
		/// <summary>
		/// FilterGroupAdapter constructor.
		/// Save the passed parameters and hook into child item selection
		/// </summary>
		/// <param name="context"></param>
		/// <param name="tags"></param>
		/// <param name="view"></param>
		public FilterGroupAdapter( Context context, List<TagGroup> tags, List<TagGroup> currentTagGroups, ExpandableListView view, IReporter reporter )
		{
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );
			tagCollection = tags;
			currentlySelectedTagGroups = currentTagGroups;
			Reporter = reporter;

			view.SetOnChildClickListener( this );

			InitialiseSelectedItems();
		}

		/// <summary>
		/// Return the number of groups
		/// </summary>
		public override int GroupCount => tagCollection.Count;

		/// <summary>
		/// Return the number of child items in a group
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		public override int GetChildrenCount( int groupPosition ) => tagCollection[ groupPosition ].Tags.Count;

		/// <summary>
		/// Called when a child item is displayed
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <param name="isLastChild"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent )
		{
			// Create a new view if requried. Hook onto its checkbox
			if ( convertView?.FindViewById<TextView>( Resource.Id.itemName ) == null )
			{
				convertView = inflator.Inflate( Resource.Layout.filter_item, null );
				convertView.FindViewById<CheckBox>( Resource.Id.checkBox ).Click -= CheckBoxClick;
				convertView.FindViewById<CheckBox>( Resource.Id.checkBox ).Click += CheckBoxClick;
			}

			// Display the tag name
			convertView.FindViewById<TextView>( Resource.Id.itemName ).Text = tagCollection[ groupPosition ].Tags[ childPosition ].Name;

			// Display the correct state of the checkbox and make sure that the checkbox has a reference to this item
			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );
			selectionBox.Checked = SelectedChildItems.Contains( FormChildTag( groupPosition, childPosition ) );
			selectionBox.Tag = FormChildTag( groupPosition, childPosition );

			return convertView;
		}

		/// <summary>
		/// Called when a group items needs to be displayed
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="isExpanded"></param>
		/// <param name="convertView"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public override View GetGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			// Create a new view if requried. Hook onto its checkbox
			if ( convertView?.FindViewById<TextView>( Resource.Id.groupName ) == null )
			{
				convertView = inflator.Inflate( Resource.Layout.filter_group, null );
				convertView.FindViewById<CheckBox>( Resource.Id.checkBox ).Click -= CheckBoxClick;
				convertView.FindViewById<CheckBox>( Resource.Id.checkBox ).Click += CheckBoxClick;
			}

			// Display the group name. If only some of the child items are selected then display an indicator next to the name
			convertView.FindViewById<TextView>( Resource.Id.groupName ).Text = string.Format( "{0}{1}", tagCollection[ groupPosition ].Name,
				( GroupStates[ groupPosition ] == TagGroup.GroupSelectionState.Some ) ? " [Some]" : "" );

			// Display the correct state of the checkbox and make sure that the checkbox has a reference to this item
			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );
			selectionBox.Checked = ( GroupStates[ groupPosition ] == TagGroup.GroupSelectionState.All );
			selectionBox.Tag = FormGroupTag( groupPosition );

			return convertView;
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
			ChildSelection( FormChildTag( groupPosition, childPosition ) );
			return false;
		}

		/// <summary>
		/// Return a collection of TagGroups reflecting the currently selected child items
		/// </summary>
		/// <returns></returns>
		public List<TagGroup> GetSelectedTagGroups()
		{
			List<TagGroup> selectedGroups = new List<TagGroup>();

			// Iterate through the groups
			for ( int groupIndex = 0; groupIndex < tagCollection.Count; ++groupIndex )
			{
				// Anything need recording?
				if ( GroupStates[ groupIndex ] == TagGroup.GroupSelectionState.Some )
				{
					// Add a new TagGroup to the collection being built
					TagGroup group = tagCollection[ groupIndex ];
					TagGroup selectedGroup = new TagGroup() { Name = group.Name };
					selectedGroups.Add( selectedGroup );

					for ( int childIndex = 0; childIndex < group.Tags.Count; ++childIndex )
					{
						// If this child/tag is selected then add it to the new TagGroup
						if ( IsItemSelected( FormChildTag( groupIndex, childIndex ) ) == true )
						{
							selectedGroup.Tags.Add( group.Tags[ childIndex ] );
						}
					}
				}
			}

			return selectedGroups;
		}

		/// <summary>
		/// The following are required by BaseExpandableListAdapter
		/// </summary>
		/// <returns></returns>
		public override bool IsChildSelectable( int groupPosition, int childPosition ) => true;
		public override Java.Lang.Object GetGroup( int groupPosition ) => null;
		public override long GetGroupId( int groupPosition ) => groupPosition;
		public override bool HasStableIds => false;
		public override Java.Lang.Object GetChild( int groupPosition, int childPosition ) => null;
		public override long GetChildId( int groupPosition, int childPosition ) => childPosition;

		/// <summary>
		/// Initialise the SelectedChildItems from the tag collection
		/// For each Tag within each TagGroup we need to check if the matchging Tag is present in the currently selected tags passed in
		/// </summary>
		private void InitialiseSelectedItems()
		{
			GroupStates = new List<TagGroup.GroupSelectionState>();

			SelectedChildItems.Clear();

			for ( int groupIndex = 0; groupIndex < tagCollection.Count; ++groupIndex )
			{
				TagGroup group = tagCollection[ groupIndex ];

				// Check if this TagGroup is present in the list of currently selected TagGroups
				TagGroup selectedGroup = currentlySelectedTagGroups.SingleOrDefault( tg => tg.Name == group.Name );
				if ( selectedGroup != null )
				{
					// At least one of the Tags in this group is selected, so need to check each one
					// Create a lookup table to make this quicker
					Dictionary<string, Tag> lookupTable = selectedGroup.Tags.ToDictionary( ta => ta.Name );

					for ( int childIndex = 0; childIndex < group.Tags.Count; ++childIndex )
					{
						if ( lookupTable.ContainsKey( group.Tags[ childIndex ].Name ) == true )
						{
							SelectedChildItems.Add( FormChildTag( groupIndex, childIndex ) );
						}
					}
				}
				else
				{
					// If a group is not present then this means that all of its child items are selected
					for ( int childIndex = 0; childIndex < group.Tags.Count; ++childIndex )
					{
						SelectedChildItems.Add( FormChildTag( groupIndex, childIndex ) );
					}
				}

				GroupStates.Add( DetermineGroupState( groupIndex ) );
			}
		}

		/// <summary>
		/// Called when either a child or group checkbox has been selected
		/// If this is a child then call the common child selection processing.
		/// Otherwise either select or deselect alll the group's child items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckBoxClick( object sender, EventArgs e )
		{
			int tag = ( int )( ( CheckBox )sender ).Tag;
			if ( IsGroupTag( tag ) == false )
			{
				ChildSelection( tag );
			}
			else
			{
				// Selecting the group item when all items are selected will clear all child items
				// Otherwise select all child items
				int groupPosition = GetGroupFromTag( tag );
				if ( GroupStates[ groupPosition ] == TagGroup.GroupSelectionState.All )
				{
					for ( int childPosition = 0; childPosition < tagCollection[ groupPosition ].Tags.Count; ++childPosition )
					{
						SelectedChildItems.Remove( FormChildTag( groupPosition, childPosition ) );
					}

					GroupStates[ groupPosition ] = TagGroup.GroupSelectionState.None;
					Reporter.OnGroupStatusChange( GroupStates );
				}
				else
				{
					for ( int childPosition = 0; childPosition < tagCollection[ groupPosition ].Tags.Count; ++childPosition )
					{
						SelectedChildItems.Add( FormChildTag( groupPosition, childPosition ) );
					}

					GroupStates[ groupPosition ] = TagGroup.GroupSelectionState.All;
					Reporter.OnGroupStatusChange( GroupStates );
				}

				NotifyDataSetChanged();
			}
		}

		/// <summary>
		/// Common processing carried out when a child has been selected
		/// Record the selection change and then determine the state of its parent group item
		/// </summary>
		/// <param name="tag"></param>
		private void ChildSelection( int tag )
		{
			if ( IsItemSelected( tag ) == true )
			{
				SelectedChildItems.Remove( tag );
			}
			else
			{
				SelectedChildItems.Add( tag );
			}

			// Determine the new state of the parent group
			int groupPosition = GetGroupFromTag( tag );
			TagGroup.GroupSelectionState oldState = GroupStates[ groupPosition ];

			GroupStates[ groupPosition ] = DetermineGroupState( groupPosition );

			if ( GroupStates[ groupPosition ] != oldState )
			{
				Reporter.OnGroupStatusChange( GroupStates );
			}

			NotifyDataSetChanged();
		}

		/// <summary>
		/// Determine the state of a group items from the states of its child items
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		private TagGroup.GroupSelectionState DetermineGroupState( int groupPosition )
		{
			int childCount = tagCollection[ groupPosition ].Tags.Count;
			int childSelectedCount = 0;

			for ( int childIndex = 0; childIndex < childCount; ++childIndex )
			{
				if ( IsItemSelected( FormChildTag( groupPosition, childIndex ) ) == true )
				{
					childSelectedCount++;
				}
			}

			return ( childSelectedCount == 0 ) ? TagGroup.GroupSelectionState.None :
				( childSelectedCount == childCount ) ? TagGroup.GroupSelectionState.All : TagGroup.GroupSelectionState.Some;
		}

		/// <summary>
		/// Is the specified item selected
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private bool IsItemSelected( int tag ) => SelectedChildItems.Contains( tag );

		/// <summary>
		/// Form a tag for a child item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <param name="childPosition"></param>
		/// <returns></returns>
		private static int FormChildTag( int groupPosition, int childPosition ) => ( groupPosition << 16 ) + childPosition;

		/// <summary>
		/// Form a tag for a group item
		/// </summary>
		/// <param name="groupPosition"></param>
		/// <returns></returns>
		private static int FormGroupTag( int groupPosition ) => ( groupPosition << 16 ) + 0xFFFF;

		/// <summary>
		/// Does the tag represent a group
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private static bool IsGroupTag( int tag ) => ( tag & 0xFFFF ) == 0xFFFF;

		/// <summary>
		/// Return the group number from a tag
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private static int GetGroupFromTag( int tag ) => tag >> 16;

		/// <summary>
		/// The state of the TagGroup items based on thier child selection states
		/// </summary>
		private List<TagGroup.GroupSelectionState> GroupStates { get; set; }

		/// <summary>
		/// The inflatore used to create the views
		/// </summary>
		private readonly LayoutInflater inflator;

		/// <summary>
		/// The collection of TagGroup items to be displayed
		/// </summary>
		private readonly List<TagGroup> tagCollection;

		/// <summary>
		/// The collection of currently selected Tag/TagGroups
		/// </summary>
		private readonly List<TagGroup> currentlySelectedTagGroups;

		/// <summary>
		/// Keep track of which child items are selected.
		/// </summary>
		private HashSet<int> SelectedChildItems { get; set; } = new HashSet<int>();

		/// <summary>
		/// The interface used to report back group status changes
		/// </summary>
		private static IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back group status changes
		/// </summary>
		public interface IReporter
		{
			void OnGroupStatusChange( List<TagGroup.GroupSelectionState> newStates );
		}
	}
}