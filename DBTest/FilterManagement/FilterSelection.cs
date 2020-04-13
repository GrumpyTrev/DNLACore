using Android.Support.V7.App;

namespace DBTest
{
	/// <summary>
	/// The FilterSelection class controls the selection of filters to be applied
	/// </summary>
	public class FilterSelection
	{
		/// <summary>
		/// PlaybackConnection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public FilterSelection( AppCompatActivity activity, FilterSelectionDelegate selectionCallback )
		{
			contextForDialogue = activity;
			selectionDelegate = selectionCallback;
		}

		/// <summary>
		/// Allow the user to pick one of the available Tags as a filter, or to turn off filtering
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter( Tag currentFilter ) =>
			FilterSelectionDialogFragment.ShowFragment( contextForDialogue.SupportFragmentManager, currentFilter, selectionDelegate );

		/// <summary>
		/// Delegate used to report back the result of the filter selection
		/// </summary>
		/// <param name="newFilter"></param>
		public delegate void FilterSelectionDelegate( Tag newFilter );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForDialogue = null;

		/// <summary>
		/// The FilterSelectionDelegate to be used for this instance
		/// </summary>
		private readonly FilterSelectionDelegate selectionDelegate = null;
	}
}