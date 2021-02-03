using Android.Support.V7.Widget;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// The PlaybackModeView class displays the current playback mode and allows the user to change the playback mode
	/// </summary>
	class PlaybackModeView : BaseController.IReporter
	{
		/// <summary>
		/// 
		/// </summary>
		public PlaybackModeView()
		{
		}

		/// <summary>
		/// Bind to the specified menu item.
		/// Replace the standard view associated with the menu item with our own reduced margin version
		/// Store the AppCompatImageButton from the view so that the icon can be changed.
		/// Add an event handler for the button.
		/// </summary>
		/// <param name="menu"></param>
		public void BindToMenu( IMenu menu )
		{
			if ( menu != null )
			{
				// Find the playback_info menu item if it exists
				IMenuItem boundMenuItem = menu.FindItem( Resource.Id.playback_mode );
				if ( boundMenuItem != null )
				{
					boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
					imageButton = boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.toolbarSpecialButton );
					DisplayPlaybackIcon();
				}

				PlaybackModeController.DataReporter = this;

			}
			else
			{
				imageButton = null;
			}
		}

		/// <summary>
		/// Called when the playback mode data has been first read or when it changes
		/// </summary>
		public void DataAvailable() => DisplayPlaybackIcon();

		/// <summary>
		/// Display the icon associated with the current playback state
		/// </summary>
		private void DisplayPlaybackIcon() => imageButton?.SetImageResource( SelectedResource );

		/// <summary>
		/// Get the resource associated with the current monitor state
		/// </summary>
		private int SelectedResource => resources[ ( int )PlaybackModeModel.ActivePlayMode ];

		/// <summary>
		/// The resource ids representing icons to be displayed. These aree in the same order as the PlayModeType enum
		/// </summary>
		private readonly int[] resources = new int[] { Resource.Drawable.linear_play, Resource.Drawable.repeat, Resource.Drawable.shuffle,
			Resource.Drawable.repeat_shuffle, Resource.Drawable.auto_play };

		/// <summary>
		/// The button (icon) item that this view is bound to
		/// </summary>
		private AppCompatImageButton imageButton = null;
	}
}