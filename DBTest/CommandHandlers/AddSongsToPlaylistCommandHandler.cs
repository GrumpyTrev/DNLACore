using System;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class AddSongsToPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// Create a Popup menu containing the play list names and show it
			PopupMenu playlistsMenu = new PopupMenu( commandButton.Context, commandButton );

			int itemId = 0;
			PlaylistsViewModel.Playlists.ForEach( list => playlistsMenu.Menu.Add( 0, itemId++, 0, list.Name ) );

			// When a menu item is clicked get the songs from the adapter and the playlist name from the selected item
			// and pass them both to the ArtistsController
			playlistsMenu.MenuItemClick += ( sender1, args1 ) => {

				// Determine which Playlist has been selected and add the selected songs to the playlist
				PlaylistsController.AddSongsToPlaylist( selectedObjects.Songs, PlaylistsViewModel.Playlists[ args1.Item.ItemId ] );

				commandCallback.PerformAction();
			};

			playlistsMenu.Show();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Songs.Count > 0 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_to_playlist;
	}
}