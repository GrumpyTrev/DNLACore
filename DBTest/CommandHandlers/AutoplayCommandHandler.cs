using Android.Support.V7.Widget;

namespace DBTest
{
	/// <summary>
	/// The AutoplayCommandHandler class is used to display a set of options when the Autoplay toolbar button is clicked.
	/// </summary>
	class AutoplayCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// Create a Popup menu containing the Autoplay options
			PopupMenu autoplayMenu = new PopupMenu( commandButton.Context, commandButton );

			autoplayMenu.Inflate( Resource.Menu.menu_autoplay );
			autoplayMenu.MenuItemClick += ( sender, args ) =>
			{
				CommandRouter.HandleCommand( args.Item.ItemId, selectedObjects.SelectedObjects, commandCallback, commandButton );
			};

			autoplayMenu.Show();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => 	( selectedObjects.ArtistsCount == 1 ) || ( selectedObjects.ArtistAlbumsCount == 1 ) ||
			( selectedObjects.SongsCount == 1 ) || ( selectedObjects.AlbumsCount == 1 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_gen;
	}
}