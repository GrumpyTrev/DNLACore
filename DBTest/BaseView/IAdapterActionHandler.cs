using System.Collections.Generic;

namespace DBTest
{
	public interface IAdapterActionHandler
	{
		void EnteredActionMode();

		void SelectedItemsChanged( SortedDictionary<int, object> selectedItems );
	}
}