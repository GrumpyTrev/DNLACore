namespace DBTest
{
	/// <summary>
	/// The ShuffleNowPlayingCommandHandler class is used to process a request to shuffle the now playing list a library
	/// </summary>
	class ShuffleNowPlayingCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the NowPlayingController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => NowPlayingController.ShuffleNowPlayingList();

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.shuffle_now_playing;
	}
}