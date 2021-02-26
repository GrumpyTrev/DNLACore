using Android.Content;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// The BaseBoundControl class provides a couple of binding interfaces, one of whioch derived types can override
	/// </summary>
	class BaseBoundControl : Java.Lang.Object
	{
		/// <summary>
		/// Allow derived classes to bind to the menu
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="context"></param>
		public virtual void BindToMenu( IMenu menu, Context context, View activityContent )
		{
		}

		/// <summary>
		/// Allow derived classes to bind to the view
		/// </summary>
		/// <param name="view"></param>
		/// <param name="context"></param>
		public virtual void BindToView( View view, Context context )
		{
		}
	}
}