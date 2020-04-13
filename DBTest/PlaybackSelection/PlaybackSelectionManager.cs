using Android.Support.V7.App;
using FragmentManager = Android.Support.V4.App.FragmentManager;

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
		public PlaybackSelectionManager( AppCompatActivity alertContext ) => manager = alertContext.SupportFragmentManager;

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
		public void ShowSelection() => SelectDeviceDialogFragment.ShowFragment( manager );

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
			if ( manager.FindFragmentByTag( RescanProgressDialogFragment.FragmentName ) is RescanProgressDialogFragment possibleDialogue )
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

			RescanProgressDialogFragment.ShowFragment( manager );
		}

		/// <summary>
		/// FragmentManager to use for the dialogue fragments
		/// </summary>
		private readonly FragmentManager manager = null;
	}

}