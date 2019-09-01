using System.Collections.Generic;

namespace DBTest
{
	class ExpandableListAdapterModel : ViewModel
	{
		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		public HashSet<int> CheckedObjects { get; } = new HashSet<int>();

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		public bool ActionMode { get; set; } = false;

		/// <summary>
		/// The GroupExpansionModel used by the included GroupClickListener
		/// </summary>
		public GroupExpansionModel ExpansionModel { get; } = new GroupExpansionModel();

		/// <summary>
		/// Clear the states held by this model
		/// </summary>
		public override void OnClear()
		{
			base.OnClear();

			ExpansionModel.OnClear();
			ActionMode = false;
			CheckedObjects.Clear();
		}
	}
}