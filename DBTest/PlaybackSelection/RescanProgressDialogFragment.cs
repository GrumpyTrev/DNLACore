using Android.App;
using Android.OS;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Select library dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class RescanProgressDialogFragment : DialogFragment
	{
		/// <summary>
		/// Save the playlist and display the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager ) => new RescanProgressDialogFragment().Show( manager, FragmentName );

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public RescanProgressDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Activity )
				.SetTitle( "Scanning for remote devices" )
				.SetView( Resource.Layout.rescan_progress_layout )
				.SetCancelable( false )
				.Create();

		/// <summary>
		/// The name used by the fragment manager for this fragment
		/// </summary>
		public static string FragmentName { get; } = "fragment_rescan_devices";
	}
}