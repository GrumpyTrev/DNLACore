namespace DBTest
{
	/// <summary>
	/// The ShowPlaybackControlsCommandHander class is used to process a request to display the playback controls
	/// </summary>
	class ShowPlaybackControlsCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the MediaControllerController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int _commandIdentity ) => MediaControllerController.ShowMediaController();

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.show_media_controls;
	}
}