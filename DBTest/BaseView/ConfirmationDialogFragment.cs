using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Dialogue requesting a confirmation of an action
	/// </summary>
	internal class ConfirmationDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the fragment
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Confirmed callback, string actionToConfirm, string positiveText = "Yes", string negativeText = "No" )
		{
			// Save the title and reporter statically to survive a rotation.
			reporter = callback;
			titleString = actionToConfirm;
			positiveButtonText = positiveText;
			negativeButtonText = negativeText;

			new ConfirmationDialogFragment().Show( manager, "fragment_confirmation_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ConfirmationDialogFragment()
		{
			Cancelable = false;
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Replace with a layout resource?
			TextView customTitle = new TextView( Context );
			customTitle.SetPadding( 20, 10, 20, 10 );
			customTitle.SetTypeface( null, TypefaceStyle.Bold );
			customTitle.SetTextColor( Color.Black );
			customTitle.SetTextSize( Android.Util.ComplexUnitType.Dip, 24 );

			customTitle.Text = titleString;

			return new AlertDialog.Builder( Activity )
				.SetCustomTitle( customTitle )
				.SetPositiveButton( positiveButtonText, delegate { reporter?.Invoke( true ); } )
				.SetNegativeButton( negativeButtonText, delegate { reporter?.Invoke( false ); } )
				.Create();
		}

		/// <summary>
		/// The delegate to call when the delete type has been selected
		/// </summary>
		private static Confirmed reporter = null;

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
		/// <param name="confirm"></param>
		public delegate void Confirmed( bool confirm );
	}
}