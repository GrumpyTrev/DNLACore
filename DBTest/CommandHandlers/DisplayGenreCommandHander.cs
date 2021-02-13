namespace DBTest
{
	/// <summary>
	/// The DisplayGenreCommandHander class is used to process a request to toggle the displaying of genres
	/// </summary>
	class DisplayGenreCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackManagementController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => DisplayGenreController.DisplayGenre = !DisplayGenreViewModel.DisplayGenre;

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.genreOption;
	}
}