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
			if ( pleaseHideMe == true )
			{
				// Hide the UI and record this in the model
				Visibility = ViewStates.Gone;

				base.Hide();

				PlaybackManagerModel.MediaControllerVisible = false;

				pleaseHideMe = false;
			}
		}

		/// <summary>
		/// Intercept the back key to hide the Media Controller
		/// </summary>
		/// <param name="event"></param>
		/// <returns></returns>
		public override bool DispatchKeyEvent( KeyEvent @event )
		{
			/*
						bool handled = false;

						// Only trap the back key if the Media Controller is visible
						if ( Visibility == ViewStates.Visible )
						{
							if ( @event.KeyCode == Keycode.Back )
							{
								base.Hide();

								// Hide the UI and record this in the model
								Visibility = ViewStates.Gone;

								PlaybackManagerModel.MediaControllerVisible = false;

								handled = true;
							}
						}

						if ( handled == false )
						{
							handled = base.DispatchKeyEvent( @event );
						}

						return handled;
				*/

			// Only trap the back key if the Media Controller is visible
			if ( Visibility == ViewStates.Visible )
			{
				if ( @event.KeyCode == Keycode.Back )
				{
					pleaseHideMe = true;
				}
			}

			return base.DispatchKeyEvent( @event );
		}

		private bool pleaseHideMe = false;
	}
}