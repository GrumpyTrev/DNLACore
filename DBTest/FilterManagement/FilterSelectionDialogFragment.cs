using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;

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
		public static void ShowFragment( FragmentManager manager, Tag currentFilter, FilterSelection.FilterSelectionDelegate selectionDelegate )
		{
			// Save the currently selected filter and delegate to survive a rotation.
			CurrentlySelectedFilter = currentFilter;
			SelectionDelegate = selectionDelegate;

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
			// Form a list of choices including None
			List<string> tagNames = FilterManagementModel.Tags.Select( tag => tag.Name ).ToList();
			tagNames.Insert( 0, "None" );

			// Which one of these is currently selected
			int tagIndex = ( CurrentlySelectedFilter != null ) ? tagNames.IndexOf( CurrentlySelectedFilter.Name ) : 0;

			// Don't provide a handler for the OK button yet as it requires access to the created dialogue
			// See OnResume
			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( "Apply filter" )
				.SetSingleChoiceItems( tagNames.ToArray(), tagIndex, delegate { } )
				.SetPositiveButton( "OK", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// Install a handler for the Ok button that gets the selected item from the internal ListView
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;
			alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
			{
				// Get the newly selected Tag and if it is different call the supplied delegate
				int tagIndex = alert.ListView.CheckedItemPosition;

				Tag newTag = ( tagIndex == 0 ) ? null : FilterManagementModel.Tags[ tagIndex - 1 ];
				if ( newTag != CurrentlySelectedFilter )
				{
					SelectionDelegate?.Invoke( newTag );
				}

				alert.Dismiss();
			};
		}

		/// <summary>
		/// The delegate to call when a filter has been selected
		/// </summary>
		private static FilterSelection.FilterSelectionDelegate SelectionDelegate { get; set; } = null;

		/// <summary>
		/// The currently selected filter
		/// </summary>
		private static Tag CurrentlySelectedFilter { get; set; } = null;
	}
}