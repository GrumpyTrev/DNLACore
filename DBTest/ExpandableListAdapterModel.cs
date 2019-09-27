using System.Collections.Generic;

namespace DBTest
{
	class ExpandableListAdapterModel : StateModel
	{
		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		public HashSet<int> CheckedObjects { get; } = new HashSet<int>();

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		public bool ActionMode
		{
			get;
			set;
		} = false;

		/// <summary>
		/// Keep track of the id's of the groups that have been expanded
		/// </summary>
		public HashSet<int> ExpandedGroups { get; set; } = new HashSet<int>();

		/// <summary>
		/// The last group expanded
		/// </summary>
		public int LastGroupOpened { get; set; } = -1;

		/// <summary>
		/// Clear the states held by this model
		/// </summary>
		public override void OnClear()
		{
			base.OnClear();

			LastGroupOpened = -1;
			ExpandedGroups.Clear();
			ActionMode = false;
			CheckedObjects.Clear();
		}
	}
}