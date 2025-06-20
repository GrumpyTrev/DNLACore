﻿using System.Collections.Generic;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The AddSongsToNowPlayingListCommandHandler class is used to process a command to add selected songs to the Now Playing list.
	/// The songs can be selected from the Artists tab, the Albums tab and the Playlists tab. For the Artists and Albums tabs the selected items are
	/// Song objects, for the Playlists tab they are either SongPlaylistItem or AlbumPlaylistItem objects
	/// </summary>
	internal class AddSongsToNowPlayingListCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// If PlaylistItems are selected then get the songs from them
			if ( selectedObjects.PlaylistItems.Count > 0 )
			{
				// If all of the PlaylistItems are from a single Playlist, and all that Playlist's items have been selected then
				// check if playback of the playlist is in progress, i.e. not at the start and not at the end. If so, prompt
				// the user about progress and check if playback is to be resumed
				if ( ( selectedObjects.Playlists.Count == 1 ) && ( selectedObjects.PlaylistItems.Count == selectedObjects.Playlists[ 0 ].PlaylistItems.Count ) &&
					( selectedObjects.ParentPlaylist.InProgress == true ) && ( Playback.ShuffleOn == false ) )
				{
					// Keep a local reference to the parent playlist to be accessed by the confirmation callback
					Playlist parentPlaylist = selectedObjects.ParentPlaylist;

					Song currentSong = parentPlaylist.InProgressSong;
					string artistName = ( parentPlaylist as AlbumPlaylist )?.InProgressAlbum.ArtistName ?? currentSong.Artist.Name;

					ConfirmationDialog.Show( 
						$"This playlist is currently playing '{currentSong.Title}' by '{artistName}'. Do you want to continue or start from the beginning?",
						() => MainApp.CommandInterface.AddPlaylistToNowPlayingList( parentPlaylist, commandIdentity == Resource.Id.play_now, true ),
						() => MainApp.CommandInterface.AddPlaylistToNowPlayingList( parentPlaylist, commandIdentity == Resource.Id.play_now, false ),
						"Continue", "Start" );
				}
				else
				{
					// This includes both SongPlaylistItem and AlbumPlaylistItem entries.
					List<Song> selectedSongs = new();
					foreach ( PlaylistItem basePlaylistItem in selectedObjects.PlaylistItems )
					{
						if ( basePlaylistItem is AlbumPlaylistItem albumPlaylistItem )
						{
							selectedSongs.AddRange( albumPlaylistItem.Album.Songs );
						}
						else
						{
							selectedSongs.Add( ( ( SongPlaylistItem )basePlaylistItem ).Song );
						}
					}

					MainApp.CommandInterface.AddSongsToNowPlayingList( selectedSongs, ( commandIdentity == Resource.Id.play_now ) );
				}
			}
			// Otherwise just use the selected songs themselves
			else
			{
				MainApp.CommandInterface.AddSongsToNowPlayingList( selectedObjects.Songs, ( commandIdentity == Resource.Id.play_now ) );
			}

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
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.PlaylistItems.Count > 0 ) || ( selectedObjects.Songs.Count > 0 );

		/// <summary>
		/// (one of)The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.add_to_queue;
	}
}
