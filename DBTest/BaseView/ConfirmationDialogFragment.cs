using Android.App;
using Android.OS;
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
		public static void ShowFragment( FragmentManager manager, Confirmed callback, string actionToConfirm )
		{
			// Save the title and reporter statically to survive a rotation.
			reporter = callback;
			titleString = actionToConfirm;

			new ConfirmationDialogFragment().Show( manager, "fragment_delete_tag_tag" );
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
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder(Activity )
				.SetTitle( titleString )
				.SetPositiveButton( "Yes", delegate { reporter?.Invoke( true ); } )
				.SetNegativeButton( "No", delegate { reporter?.Invoke( false ); } )
				.Create();

		/// <summary>
		/// The delegate to call when the delete type has been selected
		/// </summary>
		private static Confirmed reporter = null;

		// The title for the dialogue
		private static string titleString = "";

		/// <summary>
		/// Type of delegate to be called with the confirmation result
		/// </summary>
		/// <param name="confirm"></param>
		public delegate void Confirmed( bool confirm );
	}
}