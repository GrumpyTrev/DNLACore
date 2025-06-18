using System;
using System.Collections.Generic;

namespace CoreMP
{
	public class Commander
	{
		// LibraryManagementController commands
		public void SelectLibrary( Library libraryToSelect ) => libraryManagementController.SelectLibrary( libraryToSelect );
		public void ClearLibraryAsync( Library libraryToClear, Action finishedAction ) => libraryManagementController.ClearLibraryAsync( libraryToClear, finishedAction );
		public void DeleteLibraryAsync( Library libraryToDelete, Action finishedAction ) => libraryManagementController.DeleteLibraryAsync( libraryToDelete, finishedAction );
		public bool CheckLibraryEmpty( Library libraryToCheck ) => libraryManagementController.CheckLibraryEmpty( libraryToCheck );
		public void CreateSourceForLibrary( Library libraryToAddSourceTo ) => libraryManagementController.CreateSourceForLibrary( libraryToAddSourceTo );
		public void DeleteSource( Source sourceToDelete ) => libraryManagementController.DeleteSource( sourceToDelete );
		public void CreateLibrary( string libraryName ) => libraryManagementController.CreateLibrary( libraryName );

		// AlbumsController commands
		public void FilterAlbums( Tag newFilter ) => albumsController.SetNewFilter( newFilter );
		public void SortAlbums() => albumsController.SortData();

		// ArtistsController commands
		public void FilterArtists( Tag newFilter ) => artistsController.SetNewFilter(newFilter );
		public void SortArtists() => artistsController.SortArtists();

		// FilterManagementController commands
		public void AddAlbumToTag( Tag toTag, Album albumToAdd, bool synchronise = true ) => filterManagementController.AddAlbumToTag( toTag, albumToAdd, synchronise );
		public void RemoveAlbumFromTag( Tag fromTag, Album albumToRemove ) => filterManagementController.RemoveAlbumFromTag( fromTag, albumToRemove );
		public void SynchroniseAlbumPlayedStatus() => filterManagementController.SynchroniseAlbumPlayedStatus();

		// LibraryScanController commands
		public void ScanLibrary( Library libraryToScan, Action scanFinished, Func<bool> scanCancelledCheck ) =>
			libraryScanController.ScanLibraryAsynch( libraryToScan, scanFinished, scanCancelledCheck );

		// MediaControllerController commands

		// PlaybackSelectionController commands
		public void SetSelectedPlayback( string deviceName ) => playbackSelectionController.SetSelectedPlayback( deviceName );

		// PlaybackManagementController commands
		public void StopRouter() => playbackManagementController.StopRouter();
		public void SetLocalPlayer( BasePlayback localPlayer ) => playbackManagementController.SetLocalPlayer( localPlayer );
		public void Pause() => playbackManagementController.MediaControlPause();
		public void Start() => playbackManagementController.MediaControlStart();

		// NowPlayingController commands
		public void UserSongSelected( int songIndex ) => nowPlayingController.UserSongSelected( songIndex );
		public void AddSongsToNowPlayingList( IEnumerable<Song> songsToAdd, bool clearFirst ) => 
			nowPlayingController.AddSongsToNowPlayingList( songsToAdd, clearFirst );
		public void DeleteNowPlayingItems( IEnumerable<PlaylistItem> items ) => nowPlayingController.DeleteNowPlayingItems( items );
		public void AddPlaylistToNowPlayingList( Playlist playlistToAdd, bool clearFirst, bool resume ) =>
			nowPlayingController.AddPlaylistToNowPlayingList( playlistToAdd, clearFirst, resume );
		public void MoveItemsUp( IEnumerable<PlaylistItem> items ) => nowPlayingController.MoveItemsUp( items );
		public void MoveItemsDown( IEnumerable<PlaylistItem> items ) => nowPlayingController.MoveItemsDown( items );
		public void PlayNext() => nowPlayingController.MediaControlPlayNext();
		public void PlayPrevious() => nowPlayingController.MediaControlPlayPrevious();

		// PlaylistsController commands
		public void AddSongsToPlaylist( IEnumerable<Song> songsToAdd, SongPlaylist playlist ) => 
			playlistsController.AddSongsToPlaylist( songsToAdd, playlist );
		public void AddAlbumsToPlaylist( IEnumerable<Album> albumsToAdd, AlbumPlaylist playlist ) =>
			playlistsController.AddAlbumsToPlaylist( albumsToAdd, playlist );
		public void AddSongsToNewPlaylist( IEnumerable<Song> songsToAdd, string playlistName ) =>
			playlistsController.AddSongsToNewPlaylistAsync( songsToAdd, playlistName );
		public void AddAlbumsToNewPlaylist( IEnumerable<Album> albumsToAdd, string playlistName ) =>
			playlistsController.AddAlbumsToNewPlaylist( albumsToAdd, playlistName );
		public void DeletePlaylistItems( Playlist thePlaylist, IEnumerable<PlaylistItem> items ) =>
			playlistsController.DeletePlaylistItems( thePlaylist, items );
		public void DeletePlaylist( Playlist thePlaylist ) => playlistsController.DeletePlaylist( thePlaylist );
		public void DuplicatePlaylist( Playlist playlistToDuplicate ) => playlistsController.DuplicatePlaylist( playlistToDuplicate );
		public bool CheckForOtherPlaylists( string name, int playListLibrary ) => playlistsController.CheckForOtherPlaylists( name, playListLibrary );
		public void MoveItemsUp( Playlist thePlaylist, IEnumerable<PlaylistItem> items ) => playlistsController.MoveItemsUp( thePlaylist, items );
		public void MoveItemsDown( Playlist thePlaylist, IEnumerable<PlaylistItem> items ) => playlistsController.MoveItemsDown( thePlaylist, items );
		public void RenamePlaylist( Playlist playlist, string newName ) => playlistsController.RenamePlaylist( playlist, newName );

		// PlaybackModeController commands

		public void SetRepeat( bool on ) => playbackModeController.RepeatOn = on;
		public void SetShuffle( bool on ) => playbackModeController.ShuffleOn = on;

		/// <summary>
		/// The controller instances
		/// </summary>
		private readonly AlbumsController albumsController = new AlbumsController();
		private readonly ArtistsController artistsController = new ArtistsController();
		private readonly FilterManagementController filterManagementController = new FilterManagementController();
		private readonly LibraryManagementController libraryManagementController = new LibraryManagementController();
		private readonly SummaryDetailsDisplayController libraryNameDisplayController = new SummaryDetailsDisplayController();
		private readonly LibraryScanController libraryScanController = new LibraryScanController();
		private readonly MediaControllerController mediaControllerController = new MediaControllerController();
		private readonly PlaybackSelectionController playbackSelectionController = new PlaybackSelectionController();
		private readonly PlaybackManagementController playbackManagementController = new PlaybackManagementController();
		private readonly NowPlayingController nowPlayingController = new NowPlayingController();
		private readonly PlaylistsController playlistsController = new PlaylistsController();
		private readonly PlaybackModeController playbackModeController = new PlaybackModeController();
		private readonly MediaNotificationController mediaNotificationController = new MediaNotificationController();
	}
}
