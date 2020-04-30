using System.Collections.Generic;

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
	/// The TagApplicationDialogFragment allows the user to apply or remove tags from the selected albums
	/// </summary>
	internal class TagApplicationDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the specified list of tags and the current tag
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, List<Album> selectedAlbums, TagSelection.TagSelectionDelegate selectionCallback )
		{
			// Save the albums and delegate statically so that they can be referenced follwoing a configuration change
			SelectedAlbums = selectedAlbums;
			SelectionCallback = selectionCallback;

			// Show the dialogue
			new TagApplicationDialogFragment().Show( manager, "fragment_apply_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public TagApplicationDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue. When a library is selected pass it to the ScanProgressDialogFragment 	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Create the custom view to contain the tags and their selected states
			View tagView = Activity.LayoutInflater.Inflate( Resource.Layout.tag_dialogue_layout, null );
			ListView listView = tagView.FindViewById<ListView>( Resource.Id.tagsList );

			// Create and link in an adapter
			TagDataAdapter tagAdapter = new TagDataAdapter( Context, listView );
			listView.Adapter = tagAdapter;

			// Apply the selected albums to the tags and pass to the adapter
			FilterManagementController.GetAppliedTagsAsync( SelectedAlbums, ( List<AppliedTag> appliedTags ) => { tagAdapter.SetData( appliedTags ); } );

			// Create and display the dialogue
			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( "Apply tag" )
				.SetView( tagView )
				.SetPositiveButton( "OK", delegate
				{
					// Convert the index back to a Tag and report back
					SelectionCallback?.Invoke( tagAdapter.TagData );
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// The selected albums
		/// </summary>
		private static List<Album> SelectedAlbums { get; set; } = null;

		/// <summary>
		/// The delegate to call to apply the tags
		/// </summary>
		private static TagSelection.TagSelectionDelegate SelectionCallback { get; set; } = null;
	}
}