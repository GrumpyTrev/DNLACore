using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The LibraryNameDisplay class is used to display the name of the current library and to pass click events on to the appropriate command handler
	/// </summary>
	class LibraryNameDisplay : LibraryNameDisplayController.IReporter
	{
		/// <summary>
		/// Bind this class to the specified view
		/// </summary>
		public LibraryNameDisplay( View bindView )
		{
			// Find the textview to display the library name and install a click handler
			titleTextView = bindView.FindViewById<TextView>( Resource.Id.toolbar_title );
			titleTextView.Click += ( sender, args ) =>
			{
				CommandRouter.HandleCommand( Resource.Id.toolbar_title );
			};

			LibraryNameDisplayController.DataReporter = this;
		}

		/// <summary>
		/// Remove this instance from the LibraryNameDisplayController
		/// </summary>
		public void UnBind() => LibraryNameDisplayController.DataReporter = null;

		/// <summary>
		/// Called when the Library name is first known or changes
		/// </summary>
		/// <param name="libraryName"></param>
		public void DataAvailable() => titleTextView.Text = LibraryNameViewModel.LibraryName;

		/// <summary>
		/// The TextView to display the Library name
		/// </summary>
		private TextView titleTextView = null;
	}
}