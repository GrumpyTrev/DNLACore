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
	/// <summary>
	/// Select device dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SelectDeviceDialogFragment : DialogFragment
	{
		/// <summary>
		/// Save the playlist and display the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager ) => new SelectDeviceDialogFragment().Show( manager, "fragment_device_selection" );

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SelectDeviceDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Lookup the currently selected device in the collection of device to get its index
			List<string> devices = PlaybackSelectionModel.RemoteDevices.ConnectedDevices();
			int deviceIndex = devices.IndexOf( PlaybackSelectionModel.SelectedDeviceName );

			AlertDialog alert = new AlertDialog.Builder( Activity )
				.SetTitle( "Select playback device" )
				.SetSingleChoiceItems( devices.ToArray(), deviceIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e ) {
						// Only select the device if it has changed
						if ( e.Which != deviceIndex )
						{
							PlaybackSelectionController.SetSelectedPlaybackAsync( devices[ e.Which ] );
						}

						// Dismiss the dialogue
						( sender as AlertDialog ).Dismiss();
					} ) )
				.SetNegativeButton( "Cancel", delegate { } )
				.SetNeutralButton( "Rescan", delegate {
					Dismiss();
					PlaybackSelectionController.RescanRequested();
				} )
				.Create();

			return alert;
		}
	}
}