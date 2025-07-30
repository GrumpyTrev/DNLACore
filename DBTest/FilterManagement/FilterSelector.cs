using Android.Views;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The FilterSelector class controls the selection of filters to be applied
	/// </summary>
	public class FilterSelector
	{
		/// <summary>
		/// Constructor with callback delegate
		/// </summary>
		/// <param name="selectionCallback"></param>
		public FilterSelector( FilterSelectionDelegate selectionCallback, FilterSelectionModel selectionData )
		{
			selectionDelegate = selectionCallback;
			FilterData = selectionData;
		}

		public void BindToMenu( IMenuItem item )
		{
			boundMenuItem = item;

			// Allow a null item to be passed in which case nothing is bound
			if ( boundMenuItem != null )
			{
				DisplayFilterIcon();
			}
		}

		/// <summary>
		/// Allow the user to pick one of the available Tags as a filter, or to turn off filtering
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter() =>
			FilterSelectionDialogFragment.ShowFragment( CommandRouter.Manager, FilterData.CurrentFilter, FilterData.TagGroups, FilterData.GetTagNames(), selectionDelegate );

		/// <summary>
		/// Set the filter icon according to whether or not filtering is in effect
		/// </summary>
		public void DisplayFilterIcon() => boundMenuItem?.SetIcon( ( FilterData.CurrentFilter == null ) ? Resource.Drawable.filter_off : Resource.Drawable.filter_on );

		/// <summary>
		/// The menu item bound to this class
		/// </summary>
		private IMenuItem boundMenuItem = null;

		/// <summary>
		/// Delegate used to report back the result of the filter selection
		/// </summary>
		/// <param name="newFilter"></param>
		public delegate void FilterSelectionDelegate( Tag newFilter );

		/// <summary>
		/// The FilterSelectionDelegate to be used for this instance
		/// </summary>
		private readonly FilterSelectionDelegate selectionDelegate = null;

		public FilterSelectionModel FilterData { get; private set; }
	}
}
