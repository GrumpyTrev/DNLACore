using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The FilterSelectionDialogFragment class allows the user to select a filter
	/// </summary>
	internal class FilterSelectionDialogFragment : DialogFragment
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
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Initialise the controls holding the simple Tags and Genres
			View dialogView = LayoutInflater.From( Context ).Inflate( Resource.Layout.filter_selection_dialogue_layout, null );
			Spinner tagSpinner = dialogView.FindViewById<Spinner>( Resource.Id.simpleTags );
			MultiSpinner genreSpinner = dialogView.FindViewById<MultiSpinner>( Resource.Id.genreSpinner );

			InitialiseTagSpinner( tagSpinner );
			InitialiseGenreSpinner( genreSpinner );

			return new AlertDialog.Builder( Context )
				.SetTitle( "Apply filter" )
				.SetView( dialogView )
				.SetPositiveButton( "OK", delegate { OnOk( genreSpinner, tagSpinner ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}

		/// <summary>
		/// Initialise the contents of the Tag spinner
		/// </summary>
		/// <param name="tagSpinner"></param>
		private void InitialiseTagSpinner( Spinner tagSpinner )
		{
			// Form a list of the tag choices including None
			List<string> tagNames = new List<string> { "None" };
			tagNames.AddRange( FilterManagementController.GetTagNames() );

			// Which one of these is currently selected
			int currentTagIndex = ( CurrentlySelectedFilter != null ) ? tagNames.IndexOf( CurrentlySelectedFilter.Name ) : 0;

			// Create an adapter for the spinner to display the tag names
			ArrayAdapter<string> spinnerAdapter = new ArrayAdapter<string>( Context, Resource.Layout.select_dialog_item_material, tagNames.ToArray() );
			spinnerAdapter.SetDropDownViewResource( Resource.Layout.support_simple_spinner_dropdown_item );

			// Associate the adapter with the spinner and preselect the current entry
			tagSpinner.Adapter = spinnerAdapter;
			tagSpinner.SetSelection( currentTagIndex );
		}

		/// <summary>
		/// Initialise the contents of the Genre spinner
		/// </summary>
		/// <param name="genreSpinner"></param>
		private void InitialiseGenreSpinner( MultiSpinner genreSpinner )
		{
			// Get the currently selected Genre items from the CurrentlySelectedTagGroups.
			// If there is no Genre TagGroup then all the Genre items are selected.
			// Otherwise only the items in the TagGroup are selected
			bool[] selected = Enumerable.Repeat( true, FilterManagementModel.GenreTags.Tags.Count ).ToArray();

			// Is there a Genre TagGroup
			TagGroup genreGroup = CurrentlySelectedTagGroups.FirstOrDefault( group => ( group.Name == FilterManagementModel.GenreTags.Name ) );
			if ( genreGroup != null )
			{
				// Set the selected flag for each tag according to whether or not it is in the TagGroup
				for ( int genreIndex = 0; genreIndex < FilterManagementModel.GenreTags.Tags.Count; ++genreIndex )
				{
					selected[ genreIndex ] = genreGroup.Tags.Exists( tag => tag.Name == FilterManagementModel.GenreTags.Tags[ genreIndex ].Name );
				}
			}

			// Display the names of all the genre tags in a multi-select spinner
			genreSpinner.SetItems( FilterManagementModel.GenreTags.Tags.Select( ta => ta.Name ).ToList(), selected, "All" );
		}

		/// <summary>
		/// Get the selected simple and Genre tags and determine if there has been any changes
		/// </summary>
		private void OnOk( MultiSpinner genreSpinner, Spinner tagSpinner )
		{
			// Get the selected record from the Genre spinner. If not all of the items are selected then add an entry for each selected item to a new TagGroup
			List<TagGroup> selectedGroups = new List<TagGroup>();

			if ( genreSpinner.SelectionRecord.All( genre => genre ) == false )
			{
				TagGroup group = new TagGroup() { Name = FilterManagementModel.GenreTags.Name };
				selectedGroups.Add( group );

				// Merge the Spinner's selection record and the Genre tags into a single list and then add to the new group any tags that are selected
				IEnumerable<Tuple<bool, Tag>> merged = genreSpinner.SelectionRecord.Zip( FilterManagementModel.GenreTags.Tags, ( x, y ) => Tuple.Create( x, y ) );
				group.Tags.AddRange( merged.Where( t => ( t.Item1 == true ) ).Select( t => t.Item2 ) );
			}

			// Get the simple tag
			Tag newTag = ( tagSpinner.SelectedItemPosition == 0 ) ? null : Tags.GetTagByName( tagSpinner.SelectedItem.ToString() );

			// Check for simple or group tag changes
			if ( ( newTag != CurrentlySelectedFilter ) || ( selectedGroups.Count != CurrentlySelectedTagGroups.Count ) || 
				 ( selectedGroups.Any( group => GroupChanged( group ) ) == true ) )
			{
				// Update the FilterManagementModel TagGroups with the possibly updated data from the Adapter
				CurrentlySelectedTagGroups.Clear();
				CurrentlySelectedTagGroups.AddRange( selectedGroups );
				SelectionDelegate?.Invoke( newTag );
			}
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