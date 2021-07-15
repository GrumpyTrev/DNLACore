using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using System.Threading;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// Base class for all the fragments showing the database contents
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PagedFragment<T>: Fragment, IAdapterEventHandler, SortSelector.ISortReporter, ActionModeHandler.ICallback
	{
		/// <summary>
		/// Default constructor.
		/// Initialise but don't start the user interaction timer
		/// </summary>
		public PagedFragment()
		{
			userInteractionTimer = new Timer( timer => UserInteractionTimerExpired(), null, Timeout.Infinite, Timeout.Infinite );
			ActionMode = new ActionModeHandler( this );
		}

		/// <summary>
		/// Called when the fragment is first created
		/// </summary>
		/// <param name="savedInstanceState"></param>
		public override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Allow this fragment to add menu items to the activity toolbar
			HasOptionsMenu = true;

			// Link into the ActionModeHandler
			ActionMode.Activity = Activity;
			ActionMode.ViewContext = Context;
		}

		/// <summary>
		/// Called to initialise the UI elements for this fragment
		/// </summary>
		/// <param name="inflater"></param>
		/// <param name="container"></param>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public sealed override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			// Create the view
			FragmentView = inflater.Inflate( Layout, container, false );

			// Get the ExpandableListView used by this fragment
			ListView = FragmentView.FindViewById<ExpandableListView>( ListViewLayout );

			// Create the adapter for the list view and link to it
			CreateAdapter( ListView );
			ListView.SetAdapter( Adapter );
			Adapter.UserActivityDetectedAction = UserActivityDetected;

			// Link the ListView to the GotoTopControl
			BaseModel.GotoTopControl.BindControl( FragmentView, ListView );

			// Create an CommandBar to encapsulate the bottom toolbar and its command buttons
			CommandBar = new CommandBar( FragmentView, Resource.Id.bottomToolbar, HandleCommand );

			// Link this fragement's post command action to the CommandHandlerCallback instance
			commandCallback.Callback = LeaveActionMode;

			// Check if a delayed restoration of action mode can be carried out now
			ActionMode.RestoreDelayedActionMode();

			// Carry out post view creation action via a Post so that any response comes back after the UI has been created
			FragmentView.Post( () => { PostViewCreateAction(); } );

			return FragmentView;
		}

		/// <summary>
		/// Called when the fragment is destroyed
		/// Release any resources help by the fragment
		/// </summary>
		public sealed override void OnDestroyView()
		{
			FragmentView = null;
			commandCallback.Callback = null;
			FilterSelector?.BindToMenu( null );

			// Turn off the timer
			userInteractionTimer.Change( Timeout.Infinite, Timeout.Infinite );

			// Some BaseModel resources 

			// Remove this object from the sort selector
			BaseModel.SortSelector.Reporter = null;

			// Save the scroll position 
			BaseModel.ListViewState = ListView.OnSaveInstanceState();

			// Allow derived fragments to release their own resources
			ReleaseResources();
			base.OnDestroyView();
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			// Inflate the menu for this fragment
			inflater.Inflate( Menu, menu );

			// Show or hide the collapse icon
			collapseItem = menu.FindItem( Resource.Id.action_collapse );
			collapseItem?.SetVisible( expandedGroupCount > 0 );

			// If there is a filter selector then bind to it
			FilterSelector?.BindToMenu( menu.FindItem( Resource.Id.filter ) );

			// Bind the SortSelector
			BaseModel.SortSelector.BindToMenu( menu.FindItem( Resource.Id.sort ), Context, this );
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			bool handled = false;

			int id = item.ItemId;

			// Let the main command router have a look first
			if ( CommandRouter.HandleCommand( id ) == true )
			{
				handled = true;
			}
			// Pass on a collapse request to the adapter
			else if ( id == Resource.Id.action_collapse )
			{
				Adapter.OnCollapseRequest();
				handled = true;
			}
			else if ( id == Resource.Id.filter )
			{
				FilterSelector?.SelectFilter();
			}

			if ( handled == false )
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Called when the Controller has obtained the fragments data
		/// Pass it on to the adapter
		/// </summary>
		public virtual void DataAvailable()
		{
			if ( BaseModel.ListViewState != null )
			{
				ListView.OnRestoreInstanceState( BaseModel.ListViewState );
				BaseModel.ListViewState = null;
			}

			// Display the current sort order
			BaseModel.SortSelector.DisplaySortIcon();
		}

		/// <summary>
		/// Called by the SortSelector when the sort order changes
		/// </summary>
		public virtual void SortOrderChanged() { }

		/// <summary>
		/// Override the UserVisibleHint to trap when the fragment's visibility changes
		/// </summary>
		public override bool UserVisibleHint
		{
			get => base.UserVisibleHint;

			set
			{
				// Only process a change of visibility
				if ( base.UserVisibleHint != value )
				{
					base.UserVisibleHint = value;

					// Is the fragment visible
					if ( base.UserVisibleHint == true )
					{
						ActionMode.RestoreActionMode( FragmentView == null );
					}
					else
					{
						// Record that the Contextual Action Bar was being shown and then destroy it
						ActionMode.StopActionMode( true );
					}
				}
			}
		}

        /// <summary>
        /// Called when the user has exited action mode
        /// </summary>
        public void LeaveActionMode() => ActionMode.StopActionMode( false );

        /// <summary>
        /// Called when the count of expanded groups has changed
        /// Show or hide associated UI elements
        /// </summary>
        /// <param name="count"></param>
        public virtual void ExpandedGroupCountChanged( int count )
		{
			expandedGroupCount = count;

			collapseItem?.SetVisible( expandedGroupCount > 0 );
		}

        /// <summary>
        /// A request to enter action mode has been requested
        /// </summary>
        public void EnteredActionMode() => ActionMode.StartActionMode( ( IsVisible == true ) && ( UserVisibleHint == true ) );

        /// <summary>
        /// Called when the selected items have changed
        /// Update the visibility of any command bar buttons and pass on the objects to the derived classes
        /// </summary>
        /// <param name="selectedItems"></param>
        public void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			GroupedSelection selectedObjects = new GroupedSelection( selectedItems.Values );

			CommandBar.DetermineButtonsVisibility( selectedObjects );
			SelectedItemsChanged( selectedObjects );

			// Show the command bar if any of the buttons are visible
			CommandBar.Visibility = ShowCommandBar();
		}

		/// <summary>
		/// Called when the ActionBar has been created
		/// </summary>
		public void OnActionBarCreated()
		{
			// Should the command bar be shown
			CommandBar.Visibility = ShowCommandBar();

			// Treat this as user interaction
			UserActivityDetected();
		}

		/// <summary>
		/// Called when the ActionBar has been destroyed
		/// </summary>
		/// <param name="informAdapter"></param>
		public void OnActionBarDestroyed( bool informAdapter )
		{
			if ( informAdapter == true )
			{
				Adapter.ActionMode = false;
			}

			// Hide the bottom toolbar as well
			CommandBar.Visibility = false;

			// Treat this as user interaction
			UserActivityDetected();
		}

        /// <summary>
        /// Called when the Select All checkbox has been clicked on the Action Bar.
        /// Let the derived class handle this
        /// </summary>
        /// <param name="checkedState"></param>
        public virtual void AllSelected( bool checkedState )
        {
        }

		/// <summary>
		/// Let the derived classes process changed selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		protected abstract void SelectedItemsChanged( GroupedSelection selectedObjects );

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected abstract int Layout { get; }

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected abstract int ListViewLayout { get; }

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected abstract int Menu { get; }

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected abstract void CreateAdapter( ExpandableListView listView );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected abstract void PostViewCreateAction();

		/// <summary>
		/// Allow derived classes to release thier own resources
		/// </summary>
		protected virtual void ReleaseResources() { }

		/// <summary>
		/// Call when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		protected virtual void HandleCommand( int commandId, AppCompatImageButton button ) => 
			CommandRouter.HandleCommand( commandId, Adapter.SelectedItems.Values, commandCallback, button );

		/// <summary>
		/// The FilterSelection object used by this fragment
		/// </summary>
		protected virtual FilterSelection FilterSelector { get; } = null;

		/// <summary>
		/// The common model features are contained in the BaseViewModel
		/// </summary>
		protected abstract BaseViewModel BaseModel { get; }

		/// <summary>
		/// Show the command bar if any of the command bar buttons are visible
		/// </summary>
		/// <returns></returns>
		protected bool ShowCommandBar() => CommandBar.AnyButtonsVisible();

		/// <summary>
		/// Append the specified string to the tab title for this frasgment
		/// </summary>
		protected void AppendToTabTitle() => FragmentTitles.AppendToTabTitle( FilterSelector?.TabString() ?? "", this );

		/// <summary>
		/// The ExpandableListAdapter used to display the data for this fragment
		/// </summary>
		protected ExpandableListAdapter<T> Adapter { get; set; }

		/// <summary>
		/// The ActionModeHandler instance looking after the custom ActionBar
		/// </summary>
		protected ActionModeHandler ActionMode { get; } = null;

		/// <summary>
		/// Called when some kind of user interaction with the view has been detected
		/// Let the adapter know the user is active.
		/// If ActionMode is not in effect then start/re-start the activity timer
		/// </summary>
		private void UserActivityDetected()
		{
			Adapter.IsUserActive = true;
			if ( ActionMode.ActionModeActive == false )
			{
				userInteractionTimer.Change( UserInteractionTimeout, Timeout.Infinite );
			}
		}

		/// <summary>
		/// Called when the user interaction timer has expired
		/// If ActionMode is not in effect then inform the Adapter
		/// </summary>
		private void UserInteractionTimerExpired()
		{
			// Don't declare the user as inactive if Action Mode is in effect
			if ( ActionMode.ActionModeActive == false )
			{
				Activity?.RunOnUiThread( () => { Adapter.IsUserActive = false; } );
			}
		}

        /// <summary>
        /// The bottom toolbar
        /// </summary>
        protected CommandBar CommandBar { get; private set; } = null;

		/// <summary>
		/// The main view of the fragment used to indicate whether or not the UI has been created
		/// </summary>
		protected View FragmentView { get; set; } = null;

		/// <summary>
		/// The main ExpandableListView used by thie fragment
		/// </summary>
		protected ExpandableListView ListView { get; set; } = null;

		/// <summary>
		/// The base class does nothing special with the CurrentFilter property.
		/// Derived classes use it to filter what is being displayed
		/// </summary>
		protected virtual Tag CurrentFilter { get; } = null;

		/// <summary>
		/// The group Tags that are currently being applied by a derived class
		/// </summary>
		protected virtual List<TagGroup> TagGroups { get; } = new List<TagGroup>();

		/// <summary>
		/// Th enumber of expanded groups held by the Adapter
		/// </summary>
		private int expandedGroupCount = 0;

		/// <summary>
		/// The collapse menu item
		/// </summary>
		private IMenuItem collapseItem = null;

		/// <summary>
		/// The CommandHandlerCallback containing the action to call after a command has been handled
		/// </summary>
		private CommandRouter.CommandHandlerCallback commandCallback = new CommandRouter.CommandHandlerCallback();

		/// <summary>
		/// Timer used to detect when the user is no longer interacting with the view
		/// </summary>
		private Timer userInteractionTimer = null;

		/// <summary>
		/// How long to wait after user interaction before declaring that the user is no longer interacting
		/// 30 seconds
		/// </summary>
		private const int UserInteractionTimeout = 30000;
	}
}
