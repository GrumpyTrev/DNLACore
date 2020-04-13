using System.Collections.Generic;
using Android.Support.V7.App;

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
		public TagSelection( AppCompatActivity activityContext, TagSelectionDelegate selectionCallback )
		{
			contextForDialogue = activityContext;
			selectionDelegate = selectionCallback;
		}

		/// <summary>
		/// Allow the user to select one or more tags to be applied to the selected albums 
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter( List<Album> selectedAlbums )
		{
			TagApplicationDialogFragment.ShowFragment( contextForDialogue.SupportFragmentManager, selectedAlbums, selectionDelegate );
		}

		/// <summary>
		/// Delegate used to report back the result of the tag selection
		/// </summary>
		public delegate void TagSelectionDelegate( List<AppliedTag> appliedTags );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForDialogue = null;

		/// <summary>
		/// The delegate to call when the tags have been selected
		/// </summary>
		private readonly TagSelectionDelegate selectionDelegate = null;
	}
}