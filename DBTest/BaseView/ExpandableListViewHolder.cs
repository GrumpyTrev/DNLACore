using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// This is the base class for all ViewHolder classes used by the derived adapters
	/// All ViewHolders have a tag to identify the index of the view and a reference to the view's checkbox, if there is one
	/// </summary>
	internal class ExpandableListViewHolder : Java.Lang.Object
	{
		public int ItemTag { get; set; } = -1;
	}
}
