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
		public MediaControllerNoHide( Context theContext ) : base( theContext )
		{
		}

		/// <summary>
		/// Override the Hide to do nothing
		/// </summary>
		public override void Hide()
		{
		}

		public override bool DispatchKeyEvent( KeyEvent @event )
		{
			bool handled = false;

			if ( Visibility == ViewStates.Visible )
			{
				if ( @event.KeyCode == Keycode.Back )
				{
					base.Hide();
					Visibility = ViewStates.Gone;
					handled = true;
				}
			}

			if ( handled == false )
			{
				handled = base.DispatchKeyEvent( @event );
			}

			return handled;
		}
	}
}