using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The TagSelection class controls the setting of tags on albums
	/// </summary>
	class TagSelection
	{
		/// <summary>
		/// TagSelection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public TagSelection( Context alertContext, TagSelectionDelegate selectionCallback )
		{
			contextForAlert = alertContext;
			selectionDelegate = selectionCallback;
			inflator = ( LayoutInflater )contextForAlert.GetSystemService( Context.LayoutInflaterService );
		}

		/// <summary>
		/// Allow the user to select one or more tags to be applied to the selected albums 
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter( List<Album> selectedAlbums )
		{
			// Create the custom view to contain the tags and their selected states
			View tagView = inflator.Inflate( Resource.Layout.tag_dialogue_layout, null );
			ListView listView = tagView.FindViewById<ListView>( Resource.Id.tagsList );

			// Create and link in an adapter
			TagDataAdapter tagAdapter = new TagDataAdapter( contextForAlert, listView );
			listView.Adapter = tagAdapter;

			// Apply the selected albums to the tags and pass to the adapter
			FilterManagementController.GetAppliedTagsAsync( selectedAlbums, ( List<AppliedTag> appliedTags ) => { tagAdapter.SetData( appliedTags ); } );

			// Create and display the dialogue
			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Apply tag" )
				.SetView( tagView )
				.SetPositiveButton( "OK", delegate 
				{
					// Convert the index back to a Tag and report back
					selectionDelegate( tagAdapter.TagData );
				} )
				.SetNegativeButton( "Cancel", delegate {} )
				.Show();
		}

		/// <summary>
		/// Delegate used to report back the result of the tag selection
		/// </summary>
		public delegate void TagSelectionDelegate( List<AppliedTag> appliedTags );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;

		/// <summary>
		/// The delegate to call when the tags have been selected
		/// </summary>
		private readonly TagSelectionDelegate selectionDelegate = null;

		/// <summary>
		/// Inflator used to create the dialogue view
		/// </summary>
		private readonly LayoutInflater inflator = null;
	}
}