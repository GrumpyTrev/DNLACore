using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// Dialogue requesting a confirmation of an action
	/// </summary>
	internal class ConfirmationDialog : DialogFragment
	{
		/// <summary>
		/// Show the fragment
		/// </summary>
		public static void Show( string actionToConfirm, ConfirmationResponse positiveCallback, ConfirmationResponse negativeCallback = null, string positiveText = "Yes", string negativeText = "No" )
		{
			// Save the title and reporter statically to survive a rotation.
			positiveResult = positiveCallback;
			negativeResult = negativeCallback;
			titleString = actionToConfirm;
			positiveButtonText = positiveText;
			negativeButtonText = negativeText;

			new ConfirmationDialog().Show( CommandRouter.Manager, "fragment_confirmation_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ConfirmationDialog() => Cancelable = false;

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Replace with a layout resource?
			TextView customTitle = new( Context );
			customTitle.SetPadding( 20, 10, 20, 10 );
			customTitle.SetTypeface( null, TypefaceStyle.Bold );
			customTitle.SetTextColor( Color.Black );
			customTitle.SetTextSize( Android.Util.ComplexUnitType.Dip, 24 );

			customTitle.Text = titleString;

			return new AlertDialog.Builder( Activity )
				.SetCustomTitle( customTitle )
				.SetPositiveButton( positiveButtonText, delegate { positiveResult?.Invoke(); } )
				.SetNegativeButton( negativeButtonText, delegate { negativeResult?.Invoke(); } )
				.Create();
		}

		/// <summary>
		/// The positive and negative confirmation responses
		/// </summary>
		private static ConfirmationResponse positiveResult = null;
		private static ConfirmationResponse negativeResult = null;

		// The title for the dialogue
		private static string titleString = "";

		/// <summary>
		/// Button text
		/// </summary>
		private static string positiveButtonText = "";
		private static string negativeButtonText = "";

		/// <summary>
		/// Type of delegate to be called with the confirmation result
		/// </summary>
		public delegate void ConfirmationResponse();
	}
}
