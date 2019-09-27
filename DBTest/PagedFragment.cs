using Android.OS;
using Android.Views;
using Android.Support.V4.App;

namespace DBTest
{
	public class PagedFragment: Fragment, ActionMode.ICallback, IPageVisible
	{
		public override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Get the ConnectionDetailsModel to provide the database path and library identity
			connectionModel = StateModelProvider.Get( typeof( ConnectionDetailsModel ) ) as ConnectionDetailsModel;

			// Allow this fragment to add menu items to the activity toolbar
			HasOptionsMenu = true;
		}

		public sealed override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			RegisterMessages();

			View createdView = OnSpecialisedCreateView( inflater, container, savedInstanceState );

			if ( createdView != null )
			{
				bottomBar = createdView.FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.bottomToolbar );

				if ( bottomBar != null )
				{
					bottomBar.Visibility = ( expandedGroupCount > 0 ) ? ViewStates.Visible : ViewStates.Gone;
				}
			}

			return createdView;
		}

		public sealed override void OnDestroyView()
		{
			DeregisterMessages();
			base.OnDestroyView();
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			// Show or hide the collapse icon
			collapseItem = menu.FindItem( Resource.Id.action_collapse );

			if ( collapseItem != null )
			{
				collapseItem.SetVisible( expandedGroupCount > 0 );
			}
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			int id = item.ItemId;

			// Pass on a collapse request to the adapter
			if ( id == Resource.Id.action_collapse )
			{
				AdapterCollapseRequest();
			}

			return base.OnOptionsItemSelected( item );
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool OnActionItemClicked( ActionMode mode, IMenuItem item )
		{
			return false;
		}

		/// <summary>
		/// Called when the Contextual Action Bar is created.
		/// Add any configured menu items
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		public bool OnCreateActionMode( ActionMode mode, IMenu menu )
		{
			// Keep a record of the ActionMode instance so that it can be destroyed when this fragment is hidden
			actionModeInstance = mode;
			actionModeInstance.MenuInflater.Inflate( Resource.Menu.action_mode, menu );

			OnSpecialisedCreateActionMode( menu );

			return true;
		}

		/// <summary>
		/// Called when the Contextual Action Bar is destroyed.
		/// </summary>
		/// <param name="mode"></param>
		public void OnDestroyActionMode( ActionMode mode )
		{
			// If the Contextual Action Bar is being destroyed by the user then inform the adapter
			if ( retainAdapterActionMode == false )
			{
				AdapterActionModeOff();
			}
			actionModeInstance = null;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		public bool OnPrepareActionMode( ActionMode mode, IMenu menu )
		{
			return false;
		}

		/// <summary>
		/// Called when this fragment is shown or hidden
		/// </summary>
		/// <param name="visible"></param>
		public void PageVisible( bool visible )
		{
			if ( visible == true )
			{
				// If the Contextual Action Bar was being displyed before the fragment was hidden then show it again
				if ( retainAdapterActionMode == true )
				{
					Activity.StartActionMode( this );
					retainAdapterActionMode = false;
				}
			}
			else
			{
				// Record that the Contextual Action Bar was being shown and then destroy it
				if ( actionModeInstance != null )
				{
					retainAdapterActionMode = true;
					actionModeInstance.Finish();
				}
			}
		}

		public void LeaveActionMode()
		{
			if ( actionModeInstance != null )
			{
				retainAdapterActionMode = false;
				actionModeInstance.Finish();
			}
		}

		/// <summary>
		/// Called when the count of expanded artist groups has changed
		/// Show or hide associated UI elements
		/// </summary>
		/// <param name="count"></param>
		public void ExpandedGroupCountChanged( int count )
		{
			expandedGroupCount = count;

			if ( collapseItem != null )
			{
				collapseItem.SetVisible( expandedGroupCount > 0 );
			}

			if ( bottomBar != null )
			{
				bottomBar.Visibility = ( expandedGroupCount > 0 ) ? ViewStates.Visible : ViewStates.Gone;
			}
		}

		/// <summary>
		/// A request to enter action mode has been requested
		/// Display the Contextual Action Bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void EnteredActionMode( object sender, System.EventArgs e )
		{
			// If this fragment is not being displayed then record that the Contextual Action Bar should be displayed when the fragment 
			// is visible
			if ( IsVisible == true )
			{
				Activity.StartActionMode( this );
			}
			else
			{
				retainAdapterActionMode = true;
			}
		}

		/// <summary>
		/// Overridden in specialised classes to tell the adapter to turn off action mode
		/// </summary>
		protected virtual void AdapterActionModeOff()
		{
		}

		/// <summary>
		/// Overridden in specialised classes to tell the adapter to turn off action mode
		/// </summary>
		protected virtual void AdapterCollapseRequest()
		{
		}

		protected virtual View OnSpecialisedCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			return null;
		}

		protected virtual void OnSpecialisedCreateActionMode( IMenu menu )
		{
		}

		protected virtual void RegisterMessages()
		{
		}

		protected virtual void DeregisterMessages()
		{
		}

		protected ConnectionDetailsModel connectionModel = null;

		private ActionMode actionModeInstance = null;

		private bool retainAdapterActionMode = false;

		private int expandedGroupCount = 0;

		private Android.Support.V7.Widget.Toolbar bottomBar = null;

		private IMenuItem collapseItem = null;

	}
}