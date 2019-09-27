using System.Collections.Generic;

namespace DBTest
{
	class NowPlayingAdpaterModel : StateModel
	{
		/// <summary>
		/// Keep track of items that have been selected
		/// </summary>
		public HashSet<int> CheckedObjects { get; } = new HashSet<int>();
		
		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		public bool ActionMode { get; set; } = false;
	}
}