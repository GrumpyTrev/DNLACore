﻿using Android.OS;
using Android.Views;
using Android.Support.V4.App;
using Android.Widget;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// Base class for all the fragments showing the database contents
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class PagedFragment<T> : Fragment, ActionMode.ICallback, IAdapterActionHandler
	{
		/// <summary>
		/// Called when the fragment is first created
		/// </summary>
		/// <param name="savedInstanceState"></param>
		public override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Allow this fragment to add menu items to the activity toolbar
			HasOptionsMenu = true;
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

			// Create an CommandBar to encapsulate the bottom toolbar and its command buttons
			CommandBar = new CommandBar( FragmentView, Resource.Id.bottomToolbar, BindCommands, HandleCommand );

			// Sometimes the fragment is made visible before its views have been created.
			// Any attempt to re-start action mode is delayed until now.
			if ( delayedActionMode == true )
			{
				Activity.StartActionMode( this );
				delayedActionMode = false;
			}

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
			bool handled = false;

			int id = item.ItemId;

			// Pass on a collapse request to the adapter
			if ( id == Resource.Id.action_collapse )
			{
				Adapter.OnCollapseRequest();
				handled = true;
			}

			if ( handled == false )
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool OnActionItemClicked( ActionMode mode, IMenuItem item ) => false;

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

			// Set the common text title
			actionModeInstance.Title = actionModeTitle;

			// Let the derived classed create any menus they require
			OnSpecialisedCreateActionMode( mode, menu );

			// Only show the command bar if the derived classes allow
			CommandBar.Visibility = ShowCommandBar();

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
				Adapter.ActionMode = false;
			}

			// Hide the bottom toolbar as well
			CommandBar.Visibility = false;

			actionModeInstance = null;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		public bool OnPrepareActionMode( ActionMode mode, IMenu menu ) => false;

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
						// If the Contextual Action Bar was being displayed before the fragment was hidden then show it again
						if ( retainAdapterActionMode == true )
						{
							// If the view has not been created yet delay showing the Action Bar until later
							if ( FragmentView != null )
							{
								Activity.StartActionMode( this );
							}
							else
							{
								delayedActionMode = true;
							}

							retainAdapterActionMode = false;
						}
					}
					else
					{
						// Record that the Contextual Action Bar was being shown and then destroy it
						if ( ActionModeActive == true )
						{
							retainAdapterActionMode = true;
							actionModeInstance.Finish();
						}
					}
				}
			}
		}

		/// <summary>
		/// Called when the user has exited action mode
		/// </summary>
		public void LeaveActionMode()
		{
			if ( ActionModeActive == true )
			{
				retainAdapterActionMode = false;
				actionModeInstance.Finish();
			}
		}

		/// <summary>
		/// Called when the count of expanded groups has changed
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
		}

		/// <summary>
		/// A request to enter action mode has been requested
		/// Display the Contextual Action Bar
		/// </summary>
		public void EnteredActionMode()
		{
			// If this fragment is not being displayed then record that the Contextual Action Bar should be displayed when the fragment 
			// is visible
			if ( ( IsVisible == true ) && ( UserVisibleHint == true ) )
			{
				// Make sure action mode has not already been started due to this fragment being visible
				if ( ActionModeActive == false )
				{
					Activity.StartActionMode( this );
				}
			}
			else
			{
				retainAdapterActionMode = true;
			}
		}

		/// <summary>
		/// Called when the selected items have changed
		/// </summary>
		/// <param name="selectedItems"></param>
		public abstract void SelectedItemsChanged( SortedDictionary<int, object> selectedItems );

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected abstract int Layout { get; }

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected abstract int ListViewLayout { get; }

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected abstract void CreateAdapter( ExpandableListView listView );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected abstract void PostViewCreateAction();

		/// <summary>
		/// Allow derived classes to add their own menu items
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		protected virtual void OnSpecialisedCreateActionMode( ActionMode mode, IMenu menu )
		{
		}

		/// <summary>
		/// Allow derived classes to release thier own resources
		/// </summary>
		protected virtual void ReleaseResources()
		{
		}

		/// <summary>
		/// Called to allow derived classes to bind to the command bar commands
		/// </summary>
		protected abstract void BindCommands( CommandBar commandBar );

		/// <summary>
		/// Call when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		protected virtual void HandleCommand( int commandId)
		{
		}

		/// <summary>
		/// Let derived classes determine whether or not the bottom toolbar should be shown
		/// </summary>
		/// <returns></returns>
		protected virtual bool ShowCommandBar()
		{
			return false;
		}

		/// <summary>
		/// The ExpandableListAdapter used to display the data for this fragment
		/// </summary>
		protected ExpandableListAdapter<T> Adapter { get; set; }

		/// <summary>
		/// Is Action Mode in effect
		/// </summary>
		protected bool ActionModeActive => ( actionModeInstance != null );

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
		/// The title to be shown on the action bar
		/// </summary>
		protected string ActionModeTitle
		{
			get => actionModeTitle;

			set
			{
				actionModeTitle = value;

				if ( ActionModeActive == true )
				{
					actionModeInstance.Title = actionModeTitle;
				}
			}
		}

		/// <summary>
		/// The Action Mode instance
		/// </summary>
		private ActionMode actionModeInstance = null;

		/// <summary>
		/// Used to record that Action Mode should be re-started when the fragment is made visible again
		/// </summary>
		private bool retainAdapterActionMode = false;

		/// <summary>
		/// Th enumber of expanded groups held by the Adapter
		/// </summary>
		private int expandedGroupCount = 0;

		/// <summary>
		/// The collapse menu item
		/// </summary>
		private IMenuItem collapseItem = null;

		/// <summary>
		/// Has the start of Action Mode been delayed until the view has been created
		/// </summary>
		private bool delayedActionMode = false;

		/// <summary>
		/// The title to display in the action mode
		/// </summary>
		private string actionModeTitle = "";
	}
}