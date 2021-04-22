using Android.OS;

namespace DBTest
{
	/// <summary>
	/// The common model features are contained in the BaseViewModel
	/// </summary>
	public class BaseViewModel
	{
		public void Clear()
		{
			ListViewState = null;
		}

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		public IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// Class used to select the album sort order
		/// </summary>
		public SortSelector SortSelector { get; } = new SortSelector();

		/// <summary>
		/// Control used to provide goto top shortcut
		/// </summary>
		public GotoTopControl GotoTopControl { get; } = new GotoTopControl();
	}
}