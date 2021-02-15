using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The LibraryNameDisplay class is used to display the name of the current library and to pass click events on to the appropriate command handler
	/// </summary>
	class LibraryNameDisplay : DataReporter.IReporter
	{
		/// <summary>
		/// Bind to the specified menu item.
		/// Replace the standard view associated with the menu item with out own reduced margin version
		/// Store the AppCompatImageButton from the view
		/// </summary>
		/// <param name="menu"></param>
		public void BindToMenu( IMenu menu, Context context )
		{
			if ( menu != null )
			{
				// Find the playback_info menu item if it exists
				IMenuItem boundMenuItem = menu.FindItem( Resource.Id.library_name );
				if ( boundMenuItem != null )
				{
					titleTextView = ( TextView )LayoutInflater.FromContext( context ).Inflate( Resource.Layout.toolbarText, null );
					boundMenuItem.SetActionView( titleTextView );

					// Create a Popup for this text view and route it's selections to the CommandRouter
					titlePopup = new PopupMenu( context, titleTextView );
					titlePopup.Inflate( Resource.Menu.menu_library );
					titlePopup.MenuItemClick += ( sender, args ) =>
					{
						CommandRouter.HandleCommand( args.Item.ItemId );
					};

					// Show the popup when the textview is selected
					titleTextView.Click += ( sender, args ) =>
					{
						titlePopup.Show();
					};

					LibraryNameDisplayController.DataReporter = this;
				}
			}
			else
			{
				titleTextView = null;
				titlePopup = null;
			}
		}

		/// <summary>
		/// Called when the Library name is first known or changes
		/// </summary>
		/// <param name="libraryName"></param>
		public void DataAvailable() => titleTextView.Text = LibraryNameViewModel.LibraryName;

		/// <summary>
		/// The TextView to display the Library name
		/// </summary>
		private TextView titleTextView = null;

		/// <summary>
		/// The PopupMenu to display when the text view is selected
		/// </summary>
		private PopupMenu titlePopup = null;
	}
}