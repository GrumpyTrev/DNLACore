namespace DBTest
{
	/// <summary>
	/// Interface allowing a fragment to be notified when its visibility changes
	/// </summary>
	internal interface IPageVisible
	{
		/// <summary>
		/// Called when the fragment is shown or hidden
		/// </summary>
		/// <param name="visible"></param>
		void PageVisible( bool visible );
	}
}
