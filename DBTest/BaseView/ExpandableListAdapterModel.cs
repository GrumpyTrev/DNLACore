using System.Collections.Generic;

namespace DBTest
{
	public class ExpandableListAdapterModel
	{
		/// <summary>
		/// Keep track of the id's of the groups that have been expanded
		/// </summary>
		public HashSet<int> ExpandedGroups { get; set; } = new HashSet<int>();

		/// <summary>
		/// The last group expanded
		/// </summary>
		public int LastGroupOpened { get; set; } = -1;

		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		public HashSet<int> CheckedObjects { get; } = new HashSet<int>();

		/// <summary>
		/// The number of selected items. This is not the same as the number of checked objects as
		/// some checked objects are groups
		/// </summary>
		public int SelectedItemsCount { get; set; } = 0;

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		public bool ActionMode { get; set; } = false;

		/// <summary>
		/// Clear the states held by this model
		/// </summary>
		public void OnClear()
		{
			ActionMode = false;
			CheckedObjects.Clear();
			LastGroupOpened = -1;
			ExpandedGroups.Clear();
			SelectedItemsCount = 0;
		}
	}
}