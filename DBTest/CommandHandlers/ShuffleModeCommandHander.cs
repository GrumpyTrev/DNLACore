using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The ShuffleModeCommandHander class is used to process a request to toggle the Shuffle play mode
	/// </summary>
	internal class ShuffleModeCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackModeController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => CoreMPApp.Instance.CommandInterface.SetShuffle( !PlaybackModeModel.ShuffleOn );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.shuffle_on_off;
	}
}
