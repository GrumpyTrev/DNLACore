using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using System.Threading;
using Android.Views;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// Base class for all the fragments showing the database contents
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PagedFragment<T>: Fragment, IAdapterEventHandler, ActionModeHandler.ICallback
	{
		/// <summary>
		/// Default constructor.
		/// Initialise but don't start the user interaction timer
		/// </summary>
		public PagedFragment()
		{
			userInteractionTimer = new Timer( timer => UserInteractionTimerExpired(), null, Timeout.Infinite, Timeout.Infinite );
			ActionMode = new ActionModeHandler( this, ActionMenu );
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
			gotoTopControl.BindControl( FragmentView, ListView );

			// Link this fragement's post command action to the CommandHandlerCallback instance
			commandCallback.Callback = LeaveActionMode;

			// Check if a delayed restoration of action mode can be carried out now
			ActionMode.RestoreDelayedActionMode();

			// Create the MediaControlsView and link it in to the notification system
			songDetails.BindToView( FragmentView, MediaControlsLayout );

			// Carry out post view creation action via a Post so that any response comes back after the UI has been created
			_ = FragmentView.Post( PostViewCreateAction );

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
			_ = userInteractionTimer.Change( Timeout.Infinite, Timeout.Infinite );

			// Save the scroll position 
			ListViewState = ListView.OnSaveInstanceState();

			// Get rid of any MediaControlsView bindings
			songDetails.BindToView( null, -1 );

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

			// If there is a filter selector then bind to it
			FilterSelector?.BindToMenu( menu.FindItem( Resource.Id.filter ) );

			// Bind the SortSelector
			SortSelector?.BindToMenu( menu.FindItem( Resource.Id.sort ), Context );
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
			if ( ListViewState != null )
			{
				ListView.OnRestoreInstanceState( ListViewState );
				ListViewState = null;
			}

			// Display the current sort order
			SortSelector?.DisplaySortIcon();
		}

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
        /// A request to enter action mode has been requested
        /// </summary>
        public void EnteredActionMode() => ActionMode.StartActionMode( ( IsVisible == true ) && ( UserVisibleHint == true ) );

        /// <summary>
        /// Called when the selected items have changed
        /// Update the visibility of any command bar buttons
        /// </summary>
        /// <param name="selectedItems"></param>
        public void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			GroupedSelection selectedObjects = new( selectedItems.Values );

			ActionMode.ActionModeTitle = ( SelectedItemCount( selectedObjects ) > 0 ) ? $"{SelectedItemCount( selectedObjects )} selected" : string.Empty;
			ActionMode.DetermineMenuItemsVisibility( selectedObjects );
		}

		/// <summary>
		/// Called when the ActionBar has been created.
		/// Treat this as user interaction
		/// </summary>
		public void OnActionBarCreated() => UserActivityDetected();

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

			// Treat this as user interaction
			UserActivityDetected();
		}

		/// <summary>
		/// Call when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		public void HandleCommand( int commandId, View anchorView ) =>
			CommandRouter.HandleCommand( commandId, Adapter.SelectedItems.Values, commandCallback, anchorView, Context );

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
		/// The menu resource for this fragment's action bar
		/// </summary>
		protected abstract int ActionMenu { get; }

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
		/// The FilterSelector object used by this fragment
		/// </summary>
		protected virtual FilterSelector FilterSelector { get; } = null;

		/// <summary>
		/// Class used to select the album sort order
		/// </summary>
		protected virtual SortSelector SortSelector { get; } = null;

		/// <summary>
		/// Append the specified string to the tab title for this frasgment
		/// </summary>
		protected void AppendToTabTitle() => FragmentTitles.AppendToTabTitle( FilterSelector?.FilterData.TabString() ?? "", this );

		/// <summary>
		/// By default the selected items count displayed is the number of songs. Allow this to be overloaded
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected virtual int SelectedItemCount( GroupedSelection selectedObjects ) => selectedObjects.Songs.Count;

		/// <summary>
		/// The ExpandableListAdapter used to display the data for this fragment
		/// </summary>
		protected ExpandableListAdapter<T> Adapter { get; set; }

		/// <summary>
		/// The ActionModeHandler instance looking after the custom ActionBar
		/// </summary>
		protected ActionModeHandler ActionMode { get; } = null;

		/// <summary>
		/// The layout resource to be used for this fragment's MediaControlsView
		/// </summary>
		protected virtual int MediaControlsLayout { get; } = Resource.Id.media_controller_standard_layout;

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
				_ = userInteractionTimer.Change( UserInteractionTimeout, Timeout.Infinite );
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
				Activity?.RunOnUiThread( () => Adapter.IsUserActive = false );
			}
		}

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
		protected virtual List<TagGroup> TagGroups { get; } = [];

		/// <summary>
		/// The CommandHandlerCallback containing the action to call after a command has been handled
		/// </summary>
		private readonly CommandRouter.CommandHandlerCallback commandCallback = new();

		/// <summary>
		/// Timer used to detect when the user is no longer interacting with the view
		/// </summary>
		private readonly Timer userInteractionTimer = null;

		/// <summary>
		/// Control used to provide goto top shortcut
		/// </summary>
		private readonly GotoTopControl gotoTopControl = new ();

		/// <summary>
		/// Control used to display the details of the songe currentlty being played
		/// </summary>
		private readonly MediaControlsView songDetails = new();

		/// <summary>
		/// The scroll state of the list view
		/// </summary>
		private IParcelable ListViewState { get; set; } = null;

		/// <summary>
		/// How long to wait after user interaction before declaring that the user is no longer interacting
		/// 30 seconds
		/// </summary>
		private const int UserInteractionTimeout = 30000;
	}
}
