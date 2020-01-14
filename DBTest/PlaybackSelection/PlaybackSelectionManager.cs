using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The PlaybackSelectionManager class controls the discovery and selection of playback device
	/// </summary>
	class PlaybackSelectionManager : PlaybackSelectionController.IReporter
	{
		/// <summary>
		/// PlaybackConnection constructor
		/// Save the supplied context
		/// </summary>
		/// <param name="bindContext"></param>
		public PlaybackSelectionManager( AppCompatActivity alertContext ) => contextForAlert = alertContext;

		/// <summary>
		/// Initialise the PlaybackSelectionController and start detecting remote devices
		/// </summary>
		public void StartSelection()
		{
			PlaybackSelectionController.Reporter = this;
			PlaybackSelectionController.DiscoverDevicesAsync();
		}

		/// <summary>
		/// Show the device selection dialogue
		/// </summary>
		public void ShowSelection() => new SelectDeviceDialogFragment().Show( contextForAlert.SupportFragmentManager, "fragment_device_selection" );

		/// <summary>
		/// Called when the remote devices that support DNLA have been discovered
		/// </summary>
		/// <param name="message"></param>
		public void PlaybackSelectionDataAvailable()
		{
		}

		/// <summary>
		/// Called when the remote device discovery process has finished.
		/// If a RescanProgress dialogue is being shown then dismiss it and show the selection dialogue again
		/// </summary>
		public void DiscoveryFinished()
		{
			if ( contextForAlert.SupportFragmentManager.FindFragmentByTag( "fragment_rescan_devices" ) is RescanProgressDialogFragment possibleDialogue )
			{
				possibleDialogue.Dismiss();
				ShowSelection();
			}
		}

		/// <summary>
		/// Called when a rescan has been requested by the user
		/// Display a progress dialog and start the scanning process
		/// </summary>
		public void RescanRequested()
		{
			PlaybackSelectionController.ReDiscoverDevices();

			new RescanProgressDialogFragment().Show( contextForAlert.SupportFragmentManager, "fragment_rescan_devices" );
		}

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly AppCompatActivity contextForAlert = null;
	}

	/// <summary>
	/// Select library dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SelectDeviceDialogFragment: DialogFragment
	{
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
						// Save the selection
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

	/// <summary>
	/// Select library dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class RescanProgressDialogFragment: DialogFragment
	{
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
	}
}