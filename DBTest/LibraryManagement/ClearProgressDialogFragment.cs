using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Display a progress dialogue whilst clearing the specified library
	/// </summary>
	internal class ClearProgressDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string libraryName, BindDialog callback )
		{
			// Save the parameters statically so that they are available after a configuration change
			libraryToClear = libraryName;
			binder = callback;

			new ClearProgressDialogFragment().Show( manager, "fragment_clear_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ClearProgressDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) => new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Clearing library: {0}", libraryToClear ) )
				.SetPositiveButton( "Ok", delegate { } )
				.SetCancelable( false )
				.Create();

		/// <summary>
		/// Bind this dialogue to its command handler.
		/// The command handler will then update the dialogue's state
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();
			binder.Invoke( this );
		}

		/// <summary>
		/// Unbind this dialogue so that it can be garbage collected if required
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			binder.Invoke( null );
		}

		/// <summary>
		/// Update the state of the dialogue according to whether or not the library is still being cleared
		/// </summary>
		public void UpdateDialogueState( bool clearFinished )
		{
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = clearFinished;
			if ( clearFinished == true )
			{
				Dialog.SetTitle( string.Format( "Library: {0} cleared", libraryToClear ) );
			}
		}

		/// <summary>
		/// The delegate used to report back the ClearProgressDialogFragment object
		/// </summary>
		private static BindDialog binder = null;

		/// <summary>
		/// Delegate type used to report back the ClearProgressDialogFragment object
		/// </summary>
		public delegate void BindDialog( ClearProgressDialogFragment dialogue );

		/// <summary>
		/// The name of the library to clear
		/// </summary>
		private static string libraryToClear = "";
	}
}