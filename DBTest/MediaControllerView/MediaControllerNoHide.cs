using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The MediaControllerNoHide class extends the standard MediaController in order to prevent it being hidden
	/// </summary>
	class MediaControllerNoHide : MediaController
	{
		/// <summary>
		/// MediaControllerNoHide constructor
		/// Pass the context on to the base class
		/// </summary>
		/// <param name="theContext"></param>
		public MediaControllerNoHide( Context theContext ) : base( theContext )
		{
		}

		/// <summary>
		/// Override the Hide to do nothing
		/// </summary>
		public override void Hide()
		{
		}

		/// <summary>
		/// Intercept the back key to hide the Media Controller
		/// Only use the key up event. If the key down event is used to hide (or allow to be hidden) the controls then the up event
		/// may still be processed by some other view
		/// </summary>
		/// <param name="keyEvent"></param>
		/// <returns></returns>
		public override bool DispatchKeyEvent( KeyEvent keyEvent )
		{
			bool handled = false;

			if ( ( keyEvent.KeyCode == Keycode.Back ) && ( keyEvent.Action == KeyEventActions.Up ) )
			{
				// Hide the UI and record this in the model
				Visibility = ViewStates.Gone;

				base.Hide();

				MediaControllerViewModel.MediaControllerHiddenByUser = true;

				// Don't pass this event on
				handled = true;
			}

			if ( handled == false )
			{
				handled = base.DispatchKeyEvent( keyEvent );
			}

			return handled;
		}
	}
}