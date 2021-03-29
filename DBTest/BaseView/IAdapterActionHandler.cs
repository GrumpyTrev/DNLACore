using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The IAdapterEventHandler defines a set of methods to call in response to user interaction with the adapter's contents
	/// </summary>
	public interface IAdapterEventHandler
	{
		/// <summary>
		/// Called when the user has long clicked on an item thereby putting the fragment into Action Mode
		/// </summary>
		void EnteredActionMode();

		/// <summary>
		/// Called when in ActionMode the selected items have changed
		/// </summary>
		/// <param name="selectedItems"></param>
		void SelectedItemsChanged( SortedDictionary<int, object> selectedItems );
	}
}