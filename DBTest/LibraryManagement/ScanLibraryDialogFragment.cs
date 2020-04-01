using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	internal class ScanLibraryDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the specified list of libraries
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, List<string> libraryNamesList )
		{
			// Save the library names statically to survive a rotation.
			libraryNames = libraryNamesList;

			new ScanLibraryDialogFragment().Show( manager, "fragment_scan_library" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ScanLibraryDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			Library libraryToScan = null;

			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( "Select library to scan" )
				.SetSingleChoiceItems( libraryNames.ToArray(), -1,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						libraryToScan = LibraryManagementModel.Libraries[ e.Which ];
						( sender as AlertDialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
					} ) )
				.SetPositiveButton( "Ok", delegate { ScanSelectedLibrary( libraryToScan ); } )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// Disable the OK button when the dialogue is first displayed
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;
			alert.GetButton( ( int )DialogButtonType.Positive ).Enabled = false;
		}

		/// <summary>
		/// Scan the selected library
		/// Display a cancellable progress dialogue and start the scan process going
		/// </summary>
		/// <param name="libraryToScan"></param>
		private void ScanSelectedLibrary( Library libraryToScan )
		{
			ScanProgressDialogFragment.ShowFragment( Activity.SupportFragmentManager, libraryToScan );
		}

		/// <summary>
		/// The list of library names to present in the dialogue
		/// </summary>
		private static List<string> libraryNames = null;
	}
}