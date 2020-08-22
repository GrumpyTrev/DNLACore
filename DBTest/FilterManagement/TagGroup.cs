using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The TagGroup class holds a collection of Tags that together form a group of Tags that can be used as a filter
	/// </summary>
	public class TagGroup
	{
		/// <summary>
		/// The name of this group of tags
		/// </summary>
		public string Name { get; set; } = "";

		/// <summary>
		/// The tags in the group
		/// </summary>
		public List<Tag> Tags { get; set; } = new List<Tag>();

		/// <summary>
		/// The selection state of the group determined from the individual tags
		/// </summary>
		public GroupSelectionState SelectionState { get; set; } = GroupSelectionState.All;

		/// <summary>
		/// Possible group selection states
		/// </summary>
		public enum GroupSelectionState { None, All, Some };
	}
}