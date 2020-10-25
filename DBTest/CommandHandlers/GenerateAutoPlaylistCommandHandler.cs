using System.Linq;

namespace DBTest
{
	class GenerateAutoPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override async void HandleCommand( int commandIdentity )
		{
			// Pass the selected items to an AutoPlaylistConfiguration object
			AutoPlaylistConfiguration autoPlayConfig = new AutoPlaylistConfiguration();
			await autoPlayConfig.LoadConfigurationFromSelection( selectedObjects );
			BaseController.AddSongsToNowPlayingList( await autoPlayConfig.FirstGeneration( 50 ), true );

			commandCallback.PerformAction();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) =>
			( selectedObjects.Artists.Count() == 1 ) || ( selectedObjects.ArtistAlbums.Count() == 1 ) || 
			( selectedObjects.Songs.Count() == 1 ) || ( selectedObjects.Albums.Count() == 1 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_gen;
	}
}