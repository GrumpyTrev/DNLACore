namespace DBTest
{
	/// <summary>
	/// The SelectDeviceCommandHandler class is used to process a request to select a playback device
	/// </summary>
	class SelectDeviceCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the device selection dialogue
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => SelectDeviceDialogFragment.ShowFragment( CommandRouter.Manager );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.select_playback_device;
	}
}