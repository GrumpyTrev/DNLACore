namespace DBTest
{
	/// <summary>
	/// The AutoModeCommandHander class is used to process a request to toggle the Auto play mode
	/// </summary>
	internal class AutoModeCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackModeController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => PlaybackModeController.AutoOn = !PlaybackModeModel.AutoOn;

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_on_off;
	}
}
