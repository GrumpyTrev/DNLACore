using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Playlists class holds a collection of all the SongPlaylist and AlbumPlaylist entries read from storage.
	/// It allows access to these entries and automatically persists changes back to storage
	/// </summary>	
	internal static class Playlists
	{
		/// <summary>
		/// Get the Playlists collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( PlaylistCollection == null )
			{
				PlaylistCollection = new List<Playlist>();

				// Get the current set of SongPlaylists
				List<SongPlaylist> songPlaylists = await DbAccess.LoadAsync<SongPlaylist>();

				// Get all the SongPlaylistItems
				List<SongPlaylistItem> songPlaylistItems = await DbAccess.LoadAsync<SongPlaylistItem>();

				// Make sure all these items are linked to songs. Remove any that aren't
				List<SongPlaylistItem> orphanItems = songPlaylistItems.Where( item => Songs.GetSongById( item.SongId ) == null ).ToList();

				// Remove any orphaned items
				orphanItems.ForEach( item => songPlaylistItems.Remove( item ) );
				DbAccess.DeleteItems( orphanItems );

				// Link the playlists with their playlistitems
				songPlaylists.ForEach( playlist => playlist.GetContents( songPlaylistItems ) );

				// Delete any song playlists that have no items associated with them
				List<SongPlaylist> emptySongPlaylists = songPlaylists.Where( list => ( list.Name != Playlist.NowPlayingPlaylistName ) &&
					( list.PlaylistItems.Count == 0 ) ).ToList();
				DbAccess.DeleteItems( emptySongPlaylists );
				emptySongPlaylists.ForEach( playlist => songPlaylists.Remove( playlist ) );

				// Add these to the main collection
				PlaylistCollection.AddRange( songPlaylists );

				// Now do the same for the AlbumPlaylists
				List<AlbumPlaylist> albumPlaylists = await DbAccess.LoadAsync<AlbumPlaylist>();

				// Get all the PlaylistItems
				List<AlbumPlaylistItem> albumPlaylistItems = await DbAccess.LoadAsync<AlbumPlaylistItem>();

				// Link the album playlist items to thier playlists
				albumPlaylists.ForEach( playlist => playlist.GetContents( albumPlaylistItems ) );

				PlaylistCollection.AddRange( albumPlaylists );
			}
		}

		/// <summary>
		/// Get the user defined playlists for the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static List<Playlist> GetPlaylistsForLibrary( int libraryId ) =>
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && ( play.Name != Playlist.NowPlayingPlaylistName ) ).ToList();

		/// <summary>
		/// Get the Now Playing playlist for the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetNowPlayingPlaylist( int libraryId ) => GetPlaylist( Playlist.NowPlayingPlaylistName, libraryId );

		/// <summary>
		/// Get a playlist given its name and library
		/// </summary>
		/// <param name="name"></param>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static Playlist GetPlaylist( string name, int libraryId ) =>
			PlaylistCollection.Where( play => ( play.LibraryId == libraryId ) && ( play.Name == name ) ).FirstOrDefault();

		/// <summary>
		/// Delete the specified Playlist from the collections and from the storage
		/// </summary>
		/// <param name="playlistToDelete"></param>
		public static void DeletePlaylist( Playlist playlistToDelete )
		{
			// TEST
			if ( playlistToDelete != null )
			{
				PlaylistCollection.Remove( playlistToDelete );

				playlistToDelete.Clear();

				// Now delete the playlist itself. No need to wait for this to finish
				DbAccess.DeleteAsync( playlistToDelete );
			}
		}

		/// <summary>
		/// Add a playlist to the local and storage collections.
		/// Wait for the storage to complete in case the called requires access to the stored Id
		/// </summary>
		/// <param name="playlistToAdd"></param>
		public static async Task AddPlaylistAsync( Playlist playlistToAdd )
		{
			PlaylistCollection.Add( playlistToAdd );
			await DbAccess.InsertAsync( playlistToAdd );
		}

		/// <summary>
		/// Delete any SongPlaylistItem objects associated with the list of songs
		/// </summary>
		/// <param name="songIds"></param>
		/// <returns></returns>
		public static void DeletePlaylistItems( HashSet<int> songIds )
		{
			foreach ( Playlist playlist in PlaylistCollection )
			{
				if ( playlist is SongPlaylist songPlaylist )
				{
					songPlaylist.DeleteMatchingSongs( songIds );
				}
			}
		}

		/// <summary>
		/// Access the NowPlaying playlist's SongIndex
		/// </summary>
		public static int CurrentSongIndex
		{
			get => GetNowPlayingPlaylist( ConnectionDetailsModel.LibraryId ).SongIndex;

			set
			{
				Playlist nowPlayingList = GetNowPlayingPlaylist( ConnectionDetailsModel.LibraryId );

				// Normalise the value being set
				nowPlayingList.SongIndex = ( value >= nowPlayingList.PlaylistItems.Count ) ? -1 : value ;

				// Publish this change
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// Check if for any playlist the two identities represent adjacent songs.
		/// If so update the SongIndex recorded for the playlist
		/// </summary>
		/// <param name="previousSongIdentity"></param>
		/// <param name="currentSongIdentity"></param>
		internal static void CheckForAdjacentSongEntries( int previousSongIdentity, int currentSongIdentity )
		{
			foreach ( Playlist playlist in PlaylistCollection )
			{
				if ( playlist.Name != Playlist.NowPlayingPlaylistName )
				{
					playlist.CheckForAdjacentSongEntries( previousSongIdentity, currentSongIdentity );
				}
			}
		}

		/// <summary>
		/// A song has finished. Let the playlists decide if this means the entire playlist has been played
		/// </summary>
		/// <param name="songIdentity"></param>
		internal static void SongFinished( int songIdentity )
		{
			foreach ( Playlist playlist in PlaylistCollection )
			{
				if ( playlist.Name != Playlist.NowPlayingPlaylistName )
				{
					playlist.SongFinished( songIdentity );
				}
			}
		}

		/// <summary>
		/// The set of Playlists currently held in storage
		/// </summary>
		public static List<Playlist> PlaylistCollection { get; set; } = null;
	}
}
