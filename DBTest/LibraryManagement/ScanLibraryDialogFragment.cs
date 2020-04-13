using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The ScanLibraryDialogFragment lets the user choose a library to scan and uses the ScanProgressDialogFragment to perform the scan
	/// </summary>
	internal class ScanLibraryDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the available libraries
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager ) => new ScanLibraryDialogFragment().Show( manager, "fragment_scan_library" );

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanLibraryDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue. When a library is selected pass it to the ScanProgressDialogFragment 	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			List<string> libraryNames = LibraryManagementModel.Libraries.Select( lib => lib.Name ).ToList();

			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( "Select library to scan" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						( sender as AlertDialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
					} ) )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// Disable the OK button when the dialogue is displayed and no library is selected
		/// Install a handler for the OK button
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;
			Button okButton = alert.GetButton( ( int )DialogButtonType.Positive );

			// If the dialogue is being redisplayed due to a configuration change a library may have been selected so check
			if ( alert.ListView.CheckedItemPosition < 0 )
			{
				okButton.Enabled = false;
			}

			// Install a handler for the OK button that gets the selected item from the list
			okButton.Click += ( sender, args ) =>
			{
				ScanProgressDialogFragment.ShowFragment( Activity.SupportFragmentManager, LibraryManagementModel.Libraries[ alert.ListView.CheckedItemPosition ] );
				alert.Dismiss();
			};
		}
	}
}