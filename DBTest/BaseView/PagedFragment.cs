using Android.OS;
using Android.Views;
using Android.Support.V4.App;
using Android.Support.V7.Widget;

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
			// Let the concrete fragment initiialise the view
			createdView = OnSpecialisedCreateView( inflater, container, savedInstanceState );

			if ( createdView != null )
			{
				// Save a reference to the fragments's bottom toolbar if there is one
				bottomBar = createdView.FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.bottomToolbar );

				if ( bottomBar != null )
				{
					// Hide the bottom toolbar until either action mode is restored or entered
					bottomBar.Visibility = ViewStates.Gone;

					// Let the fragment initialise the bottom toolbar
					InitialiseBottomToolbar( bottomBar );
				}

				// Sometimes the fragment is made visible before its views have been created.
				// Any attempt to re-start action mode is delayed until now.
				if ( delayedActionMode == true )
				{
					Activity.StartActionMode( this );
					delayedActionMode = false;
				}
			}

			return createdView;
		}

		/// <summary>
		/// Called when the fragment is destroyed
		/// Release any resources help by the fragment
		/// </summary>
		public sealed override void OnDestroyView()
		{
			createdView = null;
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
			int id = item.ItemId;

			// Pass on a collapse request to the adapter
			if ( id == Resource.Id.action_collapse )
			{
				Adapter.OnCollapseRequest();
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

			// Set the common text title
			actionModeInstance.Title = itemsSelectedText;

			// Let the derived classed create any menus they require
			OnSpecialisedCreateActionMode( mode, menu );

			if ( bottomBar != null )
			{
				// Only show the bottom toolbar if items are selected
				bottomBar.Visibility = ( itemsSelected > 0 ) ? ViewStates.Visible : ViewStates.Gone;
			}

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
			if ( bottomBar != null )
			{
				bottomBar.Visibility = ViewStates.Gone;
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
		/// Override the UserVisibleHint to trap when the fragment's visibility changes
		/// </summary>
		public override bool UserVisibleHint
		{
			get
			{
				return base.UserVisibleHint;
			}

			set
			{
				// Only process a change of visibility
				if ( base.UserVisibleHint != value )
				{
					base.UserVisibleHint = value;

					// Is the fragment visible
					if ( UserVisibleHint == true )
					{
						// If the Contextual Action Bar was being displayed before the fragment was hidden then show it again
						if ( retainAdapterActionMode == true )
						{
							// If the view has not been created yet delay showing the Action Bar until later
							if ( createdView != null )
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
						if ( actionModeInstance != null )
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
			if ( actionModeInstance != null )
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
				if ( actionModeInstance == null )
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
		/// Called when the number of selected items (songs) has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		/// <param name="selectedItemsCount"></param>
		public virtual void SelectedItemsChanged( int selectedItemsCount )
		{
			itemsSelected = selectedItemsCount;
			itemsSelectedText = ( itemsSelected == 0 ) ? NoItemsSelectedText : string.Format( ItemsSelectedText, itemsSelected );

			// If the Action Mode bar is being displayed then update its title
			if ( actionModeInstance != null )
			{
				actionModeInstance.Title = itemsSelectedText;

				if ( bottomBar != null )
				{
					// Only show the bottom toolbar is items are selected
					bottomBar.Visibility = ( itemsSelected > 0 ) ? ViewStates.Visible : ViewStates.Gone;
				}
			}
		}

		/// <summary>
		/// Allow derived classed to create their own views
		/// </summary>
		/// <param name="inflater"></param>
		/// <param name="container"></param>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		protected virtual View OnSpecialisedCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			return null;
		}

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
		/// Called to allow the specialised fragment to initialise the bottom toolbar
		/// </summary>
		/// <param name="bottomToolbar"></param>
		protected virtual void InitialiseBottomToolbar( Toolbar bottomToolbar )
		{
		}

		/// <summary>
		/// The ExpandableListAdapter used to display the data for this fragment
		/// </summary>
		protected ExpandableListAdapter<T> Adapter { get; set; }

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
		/// The bottom toolbar
		/// </summary>
		private Toolbar bottomBar = null;

		/// <summary>
		/// The collapse menu item
		/// </summary>
		private IMenuItem collapseItem = null;

		/// <summary>
		/// The main view of rthe fragment used to indicate whether or not the UI has been created
		/// </summary>
		private View createdView = null;

		/// <summary>
		/// Has the start of Action Mode been delayed until the view has been created
		/// </summary>
		private bool delayedActionMode = false;

		/// <summary>
		/// Keep track of the number of selected items so that the Action Bar can be updated
		/// </summary>
		private int itemsSelected = 0;

		/// <summary>
		/// The text to be displayed on the Action Mode bar
		/// </summary>
		private string itemsSelectedText = NoItemsSelectedText;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs";
		private const string ItemsSelectedText = "{0} selected";
	}
}