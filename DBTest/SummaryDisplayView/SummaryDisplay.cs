using Android.Content;
using Android.Views;
using Android.Widget;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The SummaryDisplay class is used to display the summary items to the user
	/// </summary>
	internal class SummaryDisplay : BaseBoundControl
	{
		/// <summary>
		/// Bind to the specified view.
		/// </summary>
		/// <param name="view"></param>
		public override void BindToView( View view, Context context )
		{
			if ( view != null )
			{
				// Find the textview to display the library name and playback name
				libraryNameTextView = view.FindViewById<TextView>( Resource.Id.library_name );
				playbackNameTextView = view.FindViewById<TextView>( Resource.Id.playback_audio );

				// Register for changes to the SummaryDisplayViewModel and update the displayed library name and playback names
				NotificationHandler.Register( typeof( SummaryDisplayViewModel ), () => 
				{ 
					libraryNameTextView.Text = SummaryDisplayViewModel.LibraryName;
					playbackNameTextView.Text = SummaryDisplayViewModel.PlaybackName;
				});
			}
			else
			{
				libraryNameTextView = null;
				playbackNameTextView = null;

				// Remove any model notifications
				NotificationHandler.Deregister();
			}
		}

		/// <summary>
		/// The TextView to display the Library name
		/// </summary>
		private TextView libraryNameTextView = null;

		/// <summary>
		/// The TextView to display the playback device name
		/// </summary>
		private TextView playbackNameTextView = null;
	}
}
