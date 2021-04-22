using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The MultiScrollListenerExpandableListView class overrides the SetOnScrollListener method in order to provide
	/// </summary>
	public class MultiScrollListenerExpandableListView : ExpandableListView, AbsListView.IOnScrollListener
	{
		/// <summary>
		/// Constructor used by the framework 
		/// </summary>
		/// <param name="context"></param>		
		public MultiScrollListenerExpandableListView( Context context, IAttributeSet arg1 ) : base( context, arg1 ) => base.SetOnScrollListener( this );

		/// <summary>
		/// Add a listener to the list held by this class
		/// </summary>
		/// <param name="listener"></param>
		public override void SetOnScrollListener( IOnScrollListener listener ) => listeners.Add( listener );

		/// <summary>
		/// Called when the list view has been scrolled.
		/// Pass this on to all the listeners
		/// </summary>
		/// <param name="view"></param>
		/// <param name="firstVisibleItem"></param>
		/// <param name="visibleItemCount"></param>
		/// <param name="totalItemCount"></param>
		public void OnScroll( AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount )
		{
			foreach ( AbsListView.IOnScrollListener onScrollListener in listeners )
			{
				onScrollListener.OnScroll( view, firstVisibleItem, visibleItemCount, totalItemCount );
			}
		}

		/// <summary>
		/// Called when the scroll has started or stopped. 
		/// Pass this on to all the listeners
		/// </summary>
		/// <param name="view"></param>
		/// <param name="scrollState"></param>
		public void OnScrollStateChanged( AbsListView view, [GeneratedEnum] ScrollState scrollState )
		{
			foreach ( AbsListView.IOnScrollListener onScrollListener in listeners )
			{
				onScrollListener.OnScrollStateChanged( view, scrollState );
			}
		}

		/// <summary>
		/// The set of listeners handled by this class
		/// </summary>
		private readonly List<AbsListView.IOnScrollListener> listeners = new List<AbsListView.IOnScrollListener>();
	}
}