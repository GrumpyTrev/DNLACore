using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using CoreMP;
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
		/// Save the callbacks and display the dialogue
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
		public SelectDeviceDialogFragment() { }

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			InitialiseDeviceList();

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
						Dismiss();
					} ) )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
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
			binder.Invoke( null );
			base.OnPause();
		}

		/// <summary>
		/// Called when the set of available Playback Devices has changed
		/// Reload the dialog adapter with the current set of devices and redisplay
		/// </summary>
		public void PlaybackDevicesChanged()
		{
			// Double check that the Dialog is still around
			if ( Dialog != null )
			{
				AlertDialog alert = ( AlertDialog )Dialog;
				ArrayAdapter<string> adapter = ( ArrayAdapter<string> )alert.ListView.Adapter;

				InitialiseDeviceList();

				// Clear and then reload the data
				adapter.Clear();
				adapter.AddAll( devices );
				adapter.NotifyDataSetChanged();

				// This may have changed the index of the currently selected device, so tell the ListView
				alert.ListView.SetSelection( initialDeviceIndex );
			}
		}

		/// <summary>
		/// Delegate type used to report back the selected device
		/// </summary>
		public delegate void DeviceSelected( string selectedDevice );

		/// <summary>
		/// Initialise the device list from the PlaybackSelectionModel
		/// </summary>
		private void InitialiseDeviceList()
		{
			devices = PlaybackSelectionModel.PlaybackCapableDevices.Select( dev => dev.FriendlyName ).ToList();
			initialDeviceIndex = devices.IndexOf( PlaybackSelectionModel.SelectedDeviceName );
		}

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
