namespace DBTest
{
	/// <summary>
	/// The SelectDeviceCommandHandler class is used to process a request to select a playback device
	/// </summary>
	class SelectDeviceCommandHandler : CommandHandler
	{
		/// <summary>
		/// Constructor - register interest in Playback Device changes
		/// </summary>
		public SelectDeviceCommandHandler() => Mediator.RegisterPermanent( PlaybackDevicesChanged, typeof( PlaybackDevicesChangedMessage ) );

		/// <summary>
		/// Called to handle the command. Show the device selection dialogue
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => SelectDeviceDialogFragment.ShowFragment( CommandRouter.Manager, DeviceSelected, BindDialog );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.select_playback_device;

		/// <summary>
		/// Called when a Playback Device has been selected
		/// </summary>
		/// <param name="selectedSource"></param>
		private void DeviceSelected( string selectedDevice ) => PlaybackSelectionController.SetSelectedPlayback( selectedDevice );

		/// <summary>
		/// Called when the available Playback Devices has changed
		/// If the SelectDeviceDialogFragment is showing then inform it of the changed
		/// </summary>
		/// <param name="message"></param>
		private void PlaybackDevicesChanged( object message ) => deviceSelectionDialog?.PlaybackDevicesChanged();

		/// <summary>
		/// Called when the SelectDeviceDialogFragment dialog is displayed (OnResume)
		/// Save the reference for letting it know when the Playback Devices have changed
		/// </summary>
		/// <param name="dialogue"></param>
		private void BindDialog( SelectDeviceDialogFragment dialogue ) => deviceSelectionDialog = dialogue;

		/// <summary>
		/// SelectDeviceDialogFragment reference held so that it can be informed of PLayback Device changes
		/// </summary>
		private SelectDeviceDialogFragment deviceSelectionDialog = null;
	}
}