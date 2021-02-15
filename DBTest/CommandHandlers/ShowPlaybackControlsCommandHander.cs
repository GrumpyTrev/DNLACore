namespace DBTest
{
	/// <summary>
	/// The ShowPlaybackControlsCommandHander class is used to process a request to display the playback controls
	/// </summary>
	class ShowPlaybackControlsCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackManagementController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => PlaybackManagementController.ShowPlaybackControls();

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.show_media_controls;
	}
}