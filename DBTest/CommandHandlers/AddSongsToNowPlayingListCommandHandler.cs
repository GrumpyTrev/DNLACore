using System.Linq;

namespace DBTest
{
	class AddSongsToNowPlayingListCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			BaseController.AddSongsToNowPlayingList( selectedObjects.Songs, ( commandIdentity == Resource.Id.play_now ) );
			commandCallback.PerformAction();
		}

		/// <summary>
		/// Bind this command to the router. An override is required here as this command can be launched using two identities
		/// </summary>
		public override void BindToRouter()
		{
			CommandRouter.BindHandler( CommandIdentity, this );
			CommandRouter.BindHandler( Resource.Id.play_now, this );
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.PlaylistItems.Count() > 0 );

		/// <summary>
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_to_queue;
	}
}