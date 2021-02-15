namespace DBTest
{
	/// <summary>
	/// The NewTabCommandHander class is used to process a request to create a new Tag
	/// </summary>
	class NewTabCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackModeController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => TagCreator.AddNewTag();

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_tag;
	}
}