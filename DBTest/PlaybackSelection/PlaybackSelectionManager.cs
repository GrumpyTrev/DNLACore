using System;
using System.Collections.Generic;
//using System.Linq;
using Android.App;
using Android.Content;

namespace DBTest
{
	/// <summary>
	/// The PlaybackSelectionManager class controls the discovery and selection of playback device
	/// </summary>
	class PlaybackSelectionManager : PlaybackSelectionController.IReporter
	{
		/// <summary>
		/// PlaybackConnection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public PlaybackSelectionManager( Context alertContext )
		{
			contextForAlert = alertContext;
		}


		public void StartSelection()
		{
			// Initialise the PlaybackSelectionController and start detecting remote devices
			PlaybackSelectionController.Reporter = this;
			PlaybackSelectionController.DiscoverDevices();
		}

		public void ShowSelection()
		{
			// Lookup the currently selected device in the collection of device to get its index
			List<string> devices = PlaybackSelectionModel.RemoteDevices.ConnectedDevices();
			int deviceIndex = devices.IndexOf( PlaybackSelectionModel.SelectedDeviceName );

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Select playback device" )
				.SetSingleChoiceItems( devices.ToArray(), deviceIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						// Save the selection
						if ( e.Which != deviceIndex )
						{
							PlaybackSelectionController.SetSelectedPlayback( devices[ e.Which ] );
						}

						// Dismiss the dialogue
						( sender as AlertDialog ).Dismiss();
					} ) )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();
		}

		/// <summary>
		/// Called when the remote devices that support DNLA have been discovered
		/// </summary>
		/// <param name="message"></param>
		public void PlaybackSelectionDataAvailable()
		{
		}

		public void DiscoveryFinished()
		{
		}

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private Context contextForAlert = null;
	}
}