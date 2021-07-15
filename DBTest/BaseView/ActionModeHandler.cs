using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace DBTest
{
    /// <summary>
    /// The ActionModeHandler handles entering and leaving ActionMode on behalf of parent fragments.
    /// It listens for ActionBar commands and passes them back to the fragment
    /// </summary>
    public class ActionModeHandler : Java.Lang.Object, ActionMode.ICallback
    {
        /// <summary>
        /// Create an instance of the class with a callback to notify creation and deletions
        /// </summary>
        /// <param name="parentFragment"></param>
        public ActionModeHandler( ICallback callback ) => CallbackNotification = callback;

        /// <summary>
		/// Called to start ActionMode
		/// If the parent fragment is not visible then record that ActionMode should be re-started.
		/// Otherwise start action mode if not already in progress
		/// </summary>
		/// <param name="fragmentVisible"></param>
		public void StartActionMode( bool fragmentVisible )
        {
            if ( fragmentVisible == true )
            {
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
        /// Called to stop ActionMode.
        /// Only process this if already in progress.
        /// Optionally record whether or not the ActionMode was in effect
        /// </summary>
        /// <param name="recordState"></param>
        public void StopActionMode( bool recordState )
        {
            if ( ActionModeActive == true )
            {
                retainAdapterActionMode = recordState;
                actionModeInstance.Finish();
            }
        }

        /// <summary>
        /// Sometimes the parent fragment is made visible before its views have been created.
        /// Any attempt to re-start action mode is delayed until now.
        /// </summary>
        public void RestoreDelayedActionMode()
        {
            if ( delayedActionMode == true )
            {
                Activity.StartActionMode( this );
                delayedActionMode = false;
            }
        }

        /// <summary>
        /// Restore ActionMode if it was being displayed previously. Delay the restoration if requried
        /// </summary>
        public void RestoreActionMode( bool delayRestoration )
        {
            // If the Action Bar was being displayed before the fragment was hidden then show it again
            if ( retainAdapterActionMode == true )
            {
                // If the view has not been created yet delay showing the Action Bar until later
                if ( delayRestoration == false )
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

        /// <summary>
        /// Called when the Contextual Action Bar is created.
        /// Add any configured menu items
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        public bool OnCreateActionMode( ActionMode mode, IMenu menu )
        {
            // Keep a record of the ActionMode instance so that it can be destroyed when the parent fragment is hidden
            actionModeInstance = mode;

            // Set the view to our custom view
            actionModeInstance.CustomView = LayoutInflater.FromContext( ViewContext ).Inflate( Resource.Layout.action_mode, null ); ;

            // Save the text view and checkbox for for ease of access
            titleView = actionModeInstance.CustomView.FindViewById<TextView>( Resource.Id.title );
            checkBox = actionModeInstance.CustomView.FindViewById<CheckBox>( Resource.Id.checkBox );

            // Report when the checkbox is clicked
            checkBox.Click += ( sender, args ) => CallbackNotification.AllSelected( !AllSelected );

            // Set the common text title
            ActionModeTitle = ActionModeTitle;

            // Let the fragment know
            CallbackNotification.OnActionBarCreated();

            return true;
        }

        /// <summary>
        /// Called when the Contextual Action Bar is destroyed.
        /// </summary>
        /// <param name="mode"></param>
        public void OnDestroyActionMode( ActionMode mode )
        {
            actionModeInstance = null;

            // Let the fragment know
            CallbackNotification.OnActionBarDestroyed( retainAdapterActionMode == false );
        }

        /// <summary>
        /// The title to be shown on the action bar
        /// </summary>
        public string ActionModeTitle
        {
            get => actionModeTitle;

            set
            {
                actionModeTitle = value;

                if ( ActionModeActive == true )
                {
                    titleView.Text = actionModeTitle;
                }
            }
        }

        /// <summary>
        /// Set or clear the all selected checkbox
        /// </summary>
        public bool AllSelected
        {
            get => allSelected;

            set
            {
                allSelected = value;

                if ( ActionModeActive == true )
                {
                    checkBox.Checked = allSelected;
                }
            }
        }

        /// <summary>
        /// Called when a menu item on the Contextual Action Bar has been selected
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool OnActionItemClicked( ActionMode mode, IMenuItem item ) => false;

        /// <summary>
        /// Required by the interface
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        public bool OnPrepareActionMode( ActionMode mode, IMenu menu ) => false;

        /// <summary>
        /// Is Action Mode in effect
        /// </summary>
        public bool ActionModeActive => ( actionModeInstance != null );

        /// <summary>
        /// The parent fragment
        /// </summary>
        public FragmentActivity Activity { get; set; } = null;

        /// <summary>
        /// The context used to expand the custom view
        /// </summary>
        public Context ViewContext { get; set; } = null;

        /// <summary>
        /// Interface used to report the action bar being created and destroyed and the clickbox being clicked
        /// </summary>
        public interface ICallback
        {
            /// <summary>
            /// The Action Bar has been created
            /// </summary>
            void OnActionBarCreated();

            /// <summary>
            /// The action bar has been destroyed
            /// </summary>
            /// <param name="informAdapter"></param>
            void OnActionBarDestroyed( bool informAdapter );

            /// <summary>
            /// Thge check box has been clicked
            /// </summary>
            /// <param name="clickedState"></param>
            void AllSelected( bool clickedState );
        }

        /// <summary>
        /// The Action Mode instance wrapped up by this handler
        /// </summary>
        private ActionMode actionModeInstance = null;

        /// <summary>
        /// The title to display in the action mode
        /// </summary>
        private string actionModeTitle = "";

        /// <summary>
        /// Has the start of Action Mode been delayed until the view has been created
        /// </summary>
        private bool delayedActionMode = false;

        /// <summary>
        /// Used to record that Action Mode should be re-started when the parent fragment is made visible again
        /// </summary>
        private bool retainAdapterActionMode = false;

        /// <summary>
        /// Callback to be used to notify ActionMode creation and deletion
        /// </summary>
        private ICallback CallbackNotification { get; set; } = null;

        /// <summary>
        /// The TextView used to display the title
        /// </summary>
        private TextView titleView = null;

        /// <summary>
        /// The checkbox control
        /// </summary>
        private CheckBox checkBox = null;

        /// <summary>
        /// Keep track of whether all the items have been selected
        /// </summary>
        private bool allSelected = false;
    }
}
