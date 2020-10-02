using System;
using System.Collections.Generic;

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
	/// Select device dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SelectDeviceDialogFragment : DialogFragment
	{
		/// <summary>
		/// Save the playlist and display the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, DeviceSelected callback, BindDialog bindCallback )
		{
			// Save the parameters so that they are available after a configuration change
			reporter = callback;
			binder = bindCallback;

			new SelectDeviceDialogFragment().Show( manager, "fragment_device_selection" );
		}

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
			devices = PlaybackSelectionModel.RemoteDevices.ConnectedDevices();
			initialDeviceIndex = devices.IndexOf( PlaybackSelectionModel.SelectedDeviceName );

			return new AlertDialog.Builder( Activity )
				.SetTitle( "Select playback device" )
				.SetSingleChoiceItems(
					new ArrayAdapter<string>( Context, Resource.Layout.select_dialog_singlechoice_material, Android.Resource.Id.Text1, devices ),
					initialDeviceIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e ) 
					{
						// Only select the device if it has changed
						if ( e.Which != initialDeviceIndex )
						{
							reporter.Invoke( devices[ e.Which ] );
						}

						// Dismiss the dialogue
						Dialog.Dismiss();
					} ) )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create(); ;
		}

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
		/// Called when the set of available Playback Devices has changed
		/// Reload the dialog adapter with the current set of devices and redisplay
		/// </summary>
		public void PlaybackDevicesChanged()
		{
			// Make sure that this runs on the UI thread
			Activity.RunOnUiThread( () =>
			{
				AlertDialog alert = ( AlertDialog )Dialog;
				ArrayAdapter<string> adapter = ( ArrayAdapter<string> )alert.ListView.Adapter;

				// Clear and then reload the data
				adapter.Clear();
				List<string> devices = PlaybackSelectionModel.RemoteDevices.ConnectedDevices();
				adapter.AddAll( devices );
				adapter.NotifyDataSetChanged();

				// This may have changed the index of the currently selected device, so work it out and tell the ListView
				initialDeviceIndex = devices.IndexOf( PlaybackSelectionModel.SelectedDeviceName );
				alert.ListView.SetSelection( initialDeviceIndex );
			} );
		}

		/// <summary>
		/// Delegate type used to report back the selected device
		/// </summary>
		public delegate void DeviceSelected( string selectedDevice );

		/// <summary>
		/// The delegate to call when a playback device has been selected
		/// </summary>
		private static DeviceSelected reporter = null;

		/// <summary>
		/// Delegate type used to report back the SelectDeviceDialogFragment object
		/// </summary>
		public delegate void BindDialog( SelectDeviceDialogFragment dialogue );

		/// <summary>
		/// The delegate used to report back the SelectDeviceDialogFragment object
		/// </summary>
		private static BindDialog binder = null;

		/// <summary>
		/// The list of devices being displayed
		/// </summary>
		private List<string> devices = null;

		/// <summary>
		/// The index of the currently selected device in the list of devices
		/// </summary>
		private int initialDeviceIndex = -1;
	}
}
