using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The FilterSelectionDialogFragment class allows the user to select a filter
	/// </summary>
	internal class FilterSelectionDialogFragment : DialogFragment, FilterGroupAdapter.IReporter
	{
		/// <summary>
		/// Show the dialogue displaying the specified list of tags and the current tag
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Tag currentFilter, List<TagGroup> tagGroups, FilterSelection.FilterSelectionDelegate selectionDelegate )
		{
			// Save the currently selected filter and delegate to survive a rotation.
			CurrentlySelectedFilter = currentFilter;
			SelectionDelegate = selectionDelegate;
			CurrentlySelectedTagGroups = tagGroups;

			new FilterSelectionDialogFragment().Show( manager, "fragment_select_filter" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public FilterSelectionDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue. When a library is selected pass it to the ScanProgressDialogFragment 	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Context )
				.SetTitle( "Apply filter" )
				.SetView( Resource.Layout.filter_selection_dialogue_layout )
				.SetPositiveButton( "OK", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

		/// <summary>
		/// Install a handler for the Ok button that gets the selected item from the internal ListView
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// Form a list of choices including None
			List<string> tagNames = FilterManagementModel.Tags.Select( tag => tag.Name ).ToList();
			tagNames.Insert( 0, "None" );

			// Which one of these is currently selected
			int currentTagIndex = ( CurrentlySelectedFilter != null ) ? tagNames.IndexOf( CurrentlySelectedFilter.Name ) : 0;

			// Create an adapter for the list view to display the tag names
			ListView simpleTags = alert.FindViewById<ListView>( Resource.Id.simpleTags );
			simpleTags.Adapter = new ArrayAdapter<string>( Context, Resource.Layout.select_dialog_singlechoice_material, tagNames.ToArray() );
			simpleTags.ChoiceMode = ChoiceMode.Single;
			simpleTags.SetItemChecked( currentTagIndex, true );

			// Create an adapter for the ExpandableListView of group tags
			ExpandableListView groupTags = alert.FindViewById<ExpandableListView>( Resource.Id.groupTags );
			FilterGroupAdapter adapter = new FilterGroupAdapter( Context, FilterManagementModel.TagGroups, CurrentlySelectedTagGroups, groupTags, this );
			groupTags.SetAdapter( adapter );

			// Specify the action to take when the Ok button is selected
			alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
			{
				// Get the GroupTag selections
				List<TagGroup> selectedGroups = adapter.GetSelectedTagGroups();

				// Get the simple tag
				int tagIndex = simpleTags.CheckedItemPosition;
				Tag newTag = ( tagIndex == 0 ) ? null : FilterManagementModel.Tags[ tagIndex - 1 ];

				// Check for simple or group tag changes
				if ( ( newTag != CurrentlySelectedFilter ) || ( selectedGroups.Count != CurrentlySelectedTagGroups.Count ) || 
					 ( selectedGroups.Any( group => GroupChanged( group ) ) == true ) )
				{
					// Update the FilterManagementModel TagGroups with the possibly updated data from the Adapter
					CurrentlySelectedTagGroups.Clear();
					CurrentlySelectedTagGroups.AddRange( selectedGroups );

					SelectionDelegate?.Invoke( newTag );
				}

				alert.Dismiss();
			};
		}

		/// <summary>
		/// Called when a filter group state changes
		/// If any filter group is totally deselected then disable the OK button
		/// </summary>
		public void OnGroupStatusChange( List<TagGroup.GroupSelectionState> newStates )
		{
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = !newStates.Any( state => ( state == TagGroup.GroupSelectionState.None ) );
		}

		/// <summary>
		/// Determine whether or not the group represents a changed selection
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		private bool GroupChanged( TagGroup selectedGroup )
		{
			bool selectionChanged = false;

			// Get the matching group in the current selection
			TagGroup existingGroup = CurrentlySelectedTagGroups.SingleOrDefault( tg => tg.Name == selectedGroup.Name );

			// If there is no existing group, or the group size has changed, or there are entries in the new group that are not in the old one
			if ( ( existingGroup == null ) || ( selectedGroup.Tags.Count != existingGroup.Tags.Count ) ||
				 ( existingGroup.Tags.Except( selectedGroup.Tags ).ToList().Count > 0 ) )
			{
				selectionChanged = true;
			}

			return selectionChanged;
		}

		/// <summary>
		/// The delegate to call when a filter has been selected
		/// </summary>
		private static FilterSelection.FilterSelectionDelegate SelectionDelegate { get; set; } = null;

		/// <summary>
		/// The currently selected filter
		/// </summary>
		private static Tag CurrentlySelectedFilter { get; set; } = null;

		/// <summary>
		/// Any currently selected tag groups
		/// </summary>
		private static List<TagGroup> CurrentlySelectedTagGroups { get; set; } = null;
	}
}