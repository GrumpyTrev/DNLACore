using System;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// This class wraps up a standard ImageButton, allowing the image source to be specified and button click events to be captured
	/// </summary>
	class DefinedSourceImageButton
	{
		public DefinedSourceImageButton( View parentView, int buttonResource, int imageResource, Clicked clickedDelegate )
		{
			Button = parentView.FindViewById<ImageButton>( buttonResource );
			if ( Button != null )
			{
				Button.SetImageResource( imageResource );
				Button.Click += ButtonClicked;

				Reporter = clickedDelegate;
			}
		}

		/// <summary>
		/// Called when the button is cleick.
		/// Report it back via the interface
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonClicked( object sender, EventArgs e )
		{
			Reporter( Button );
		}

		/// <summary>
		/// Delegate used to report the buttonn being clciked
		/// </summary>
		/// <param name="button"></param>
		public delegate void Clicked( ImageButton button );
		
		/// <summary>
		/// The button to be reported back when clicked
		/// </summary>
		private ImageButton Button { get; set; }

		/// <summary>
		/// The delegate used to report the clicked event
		/// </summary>
		private Clicked Reporter { get; set; } = null;
	}
}