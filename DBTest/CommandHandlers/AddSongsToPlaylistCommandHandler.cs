using Android.Widget;
using System;
using Android.Views;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	internal class AddSongsToPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// The options available depend on which objects have been selected.
			// If and only if complete albums have been selected then the option to add albums to the album playlists is presented.
			// Otherwise the presented options are restricted to song playlists
			// From the Artists tab ArtistAlbum entries are selected (as well as Songs)
			// From the Albums tab Albums entries are selected (as well as Songs)

			// First of all convert a list of ArtistAlbums to a list of Albums
			foreach ( ArtistAlbum artistAlbum in selectedObjects.ArtistAlbums )
			{
				selectedObjects.Albums.Add( artistAlbum.Album );
			}

			// Check if all the selected songs are from selected albums
			CheckForCompleteAlbums();

			// Create a Popup menu containing the song and album playlist names
			PopupMenu playlistsMenu = new( commandButton.Context, commandButton );

			// Add the fixed menu items with menu ids above the range used for the actual playlists
			int nonPlaybackIndex = PlaylistsViewModel.Playlists.Count;
			playlistsMenu.Menu.Add( 0, nonPlaybackIndex++, 0, "New playlist..." );

			// If both the song and album playlist names are going to be displayed then submenus need to be added
			int itemId = 0;
			if ( completeAlbums == true )
			{
				// Create the submenus and add the playlist names to them
				ISubMenu songsSubMenu = playlistsMenu.Menu.AddSubMenu( 0, nonPlaybackIndex++, 0, "Add to song playlists" );
				ISubMenu alpbumsSubMenu = playlistsMenu.Menu.AddSubMenu( 0, nonPlaybackIndex++, 0, "Add to albums playlists" );

				PlaylistsViewModel.SongPlaylists.ForEach( list => songsSubMenu.Add( 0, itemId++, 0, list.Name ) );
				PlaylistsViewModel.AlbumPlaylists.ForEach( list => alpbumsSubMenu.Add( 0, itemId++, 0, list.Name ) );
			}
			else
			{
				// Just add the song playlists
				PlaylistsViewModel.SongPlaylists.ForEach( list => playlistsMenu.Menu.Add( 0, itemId++, 0, list.Name ) );
			}

			// When a menu item is clicked pass the songs or albums to the appropriate controller
			playlistsMenu.MenuItemClick += MenuItemClicked;

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

		/// <summary>
		/// When a menu item is clicked pass the songs or albums to the appropriate controller
		/// </summary>
		/// <param name="_"></param>
		/// <param name="args"></param>
		private void MenuItemClicked( object _, PopupMenu.MenuItemClickEventArgs args )
		{
			// Use the menu id to determine what has been selected
			int menuId = args.Item.ItemId;
			if ( menuId < PlaylistsViewModel.SongPlaylists.Count )
			{
				// Add the selected songs to the selected playlist
				PlaylistsController.AddSongsToPlaylist( selectedObjects.Songs, PlaylistsViewModel.SongPlaylists[ menuId ] );
				commandCallback.PerformAction();
			}
			else if ( menuId < PlaylistsViewModel.Playlists.Count )
			{
				// Add the selected albumns to the selected playlist
				PlaylistsController.AddAlbumsToPlaylist( selectedObjects.Albums, PlaylistsViewModel.AlbumPlaylists[ menuId - PlaylistsViewModel.SongPlaylists.Count ] );
				commandCallback.PerformAction();
			}
			else
			{
				// Finally check for a New SongPlaylist command
				if ( menuId == PlaylistsViewModel.Playlists.Count )
				{
					// Display a NewPlaylistNameDialogFragment to request a playlist name
					// If complete albums have been selected then try to choose an appropriate name for the new album playlist
					string suggestedPlaylistName = "";

					if ( completeAlbums == true )
					{
						// If just a single album then suggest the name of the album
						if ( selectedObjects.Albums.Count == 1 )
						{
							suggestedPlaylistName = $"{selectedObjects.Albums[ 0 ].ArtistName} : {selectedObjects.Albums[ 0 ].Name}";
						}
						else
						{
							// If all the albums are from the same artist then suggest the artist name
							string artistName = selectedObjects.Albums[ 0 ].ArtistName;
							if ( selectedObjects.Albums.All( album => album.ArtistName == artistName ) == true )
							{
								suggestedPlaylistName = artistName;
							}
						}
					}

					NewPlaylistNameDialogFragment.ShowFragment( CommandRouter.Manager, NameEntered, "New playlist", suggestedPlaylistName,
						completeAlbums == true, true );
				}
			}
		}

		/// <summary>
		/// Called when a playlist name has been entered has been selected.
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private async void NameEntered( string playlistName, NewPlaylistNameDialogFragment playlistNameFragment, bool isAlbum )
		{
			string alertText = "";

			// An empty playlist name is not allowed
			if ( playlistName.Length == 0 )
			{
				alertText = EmptyNameError;
			}
			else
			{
				// Check for a playlist of the same type with the same name.
				if ( ( ( isAlbum == true ) && ( PlaylistsViewModel.AlbumPlaylists.Exists( albList => albList.Name == playlistName ) == false ) ) || 
					 ( PlaylistsViewModel.SongPlaylists.Exists( albList => albList.Name == playlistName ) == false ) )
				{
					// Create a SongPlaylist or AlbumPlaylist as appropriate and add the Songs/Albums to it
					if ( isAlbum == false )
					{
						// Create the playlist and add the songs to it
						// Need to wait for the playlist to be stored as we are going to access it's Id straight away
						SongPlaylist newPlaylist = await PlaylistsController.AddSongPlaylistAsync( playlistName );
						PlaylistsController.AddSongsToPlaylist( selectedObjects.Songs, newPlaylist );
					}
					else
					{
						AlbumPlaylist newPlaylist = await PlaylistsController.AddAlbumPlaylistAsync( playlistName );
						PlaylistsController.AddAlbumsToPlaylist( selectedObjects.Albums, newPlaylist );
					}
				}
				else
				{
					alertText = DuplicatePlaylistError;
				}
			}

			// Display an error message if the playlist name is not valid. 
			if ( alertText.Length > 0 )
			{
				NotificationDialogFragment.ShowFragment( CommandRouter.Manager, alertText );
			}
			else
			{
				// Dismiss the playlist name dialogue and finally perform the command callback (exit action mode)
				playlistNameFragment.Dismiss();
				commandCallback.PerformAction();
			}
		}

		/// <summary>
		/// Check if all the selected songs are contained within the selected albums and that all the album's songs are selected
		/// </summary>
		private void CheckForCompleteAlbums()
		{
			// If there are any selected albums then check that all the selected songs belong to those albums (and none left over )
			completeAlbums = false;

			if ( selectedObjects.Albums.Count > 0 )
			{
				// Form a HashSet from all the selected songs and check if any are not included in the selected albums
				HashSet<int> selectedSongs = selectedObjects.Songs.Select( song => song.Id ).ToHashSet();

				foreach ( Album album in selectedObjects.Albums )
				{
					AlbumsController.GetAlbumContents( album );

					foreach ( Song song in album.Songs )
					{
						selectedSongs.Remove( song.Id );
					}
				}

				completeAlbums = ( selectedSongs.Count == 0 );
			}
		}

		/// <summary>
		/// Do the selected songs represent a complete set of albums
		/// </summary>
		private bool completeAlbums = false;

		/// <summary>
		/// Possible errors due to playlist name entry
		/// </summary>
		private const string EmptyNameError = "An empty name is not valid.";
		private const string DuplicatePlaylistError = "A playlist with that name already exists.";
	}
}
