using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The GotoTopControl links together a Floating Action Button, an Adapter and a ListView to provide a shortcut for going back to the top of the ListView
	/// </summary>
	public class GotoTopControl : Java.Lang.Object, AbsListView.IOnScrollListener
	{
		/// <summary>
		/// Bind this class to the goto top button in the parent view
		/// </summary>
		/// <param name="parentView"></param>
		/// <param name="listView"></param>
		public void BindControl( View parentView, AbsListView listView )
		{
			if ( parentView != null )
			{
				fab = parentView.FindViewById<FloatingActionButton>( Resource.Id.goto_top_button );
				if ( fab != null )
				{
					// Hide the button initially
					fab.Visibility = ViewStates.Gone;

					// Hook into the listview's scroll events
					listView.SetOnScrollListener( this );

					// Scroll back to the top when the button is clicked
					fab.Click += ( sender, e ) =>
					{
						if ( listView.FirstVisiblePosition > 20 )
						{
							listView.SetSelection( 20 );
						}

						listView.SmoothScrollToPosition( 0 );
					};
				}
			}
			else
			{
				fab = null;
			}
		}

		/// <summary>
		/// Called when the list view has been scrolled.
		/// Hide the FAB if the view has been scrolled to the top.
		/// Otherwise show the FAB if scrolling is active
		/// </summary>
		/// <param name="view"></param>
		/// <param name="firstVisibleItem"></param>
		/// <param name="visibleItemCount"></param>
		/// <param name="totalItemCount"></param>
		public void OnScroll( AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount )
		{
			if ( fab != null )
			{
				if ( firstVisibleItem == 0 )
				{
					fab.Visibility = ViewStates.Gone;
				}
				else if ( currentScrollState != ScrollState.Idle )
				{
					fab.Visibility = ViewStates.Visible;

					// Make sure any delayed FAB visibility changes are removed
					handler.RemoveCallbacksAndMessages( null );
				}
			}
		}

		/// <summary>
		/// Called when the scroll has started or stopped. 
		/// Hide the FAB with a delay if scrolling has stopped
		/// </summary>
		/// <param name="view"></param>
		/// <param name="scrollState"></param>
		public void OnScrollStateChanged( AbsListView view, [GeneratedEnum] ScrollState scrollState )
		{
			if ( fab != null )
			{
				if ( scrollState == ScrollState.Idle )
				{
					_ = handler.PostDelayed( () => fab.Visibility = ViewStates.Gone, 2000 );
				}

				currentScrollState = scrollState;
			}
		}

		/// <summary>
		/// The GotoTop fab
		/// </summary>
		private FloatingActionButton fab = null;

		/// <summary>
		/// The current scroll state
		/// </summary>
		private ScrollState currentScrollState = ScrollState.Idle;

		/// <summary>
		/// Handler used to post delayed FAB visibility changes
		/// </summary>
		private readonly Handler handler = new();
	}
}
