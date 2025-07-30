using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// Base class for playlists holding items derived from PlaylistItem
	/// </summary>
	public abstract partial class Playlist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		public int LibraryId { get; set; }

		/// <summary>
		/// The index of the song curently selected in the SongPlaylist
		/// For the NowPlaying playlist this is the song being played, for all other playlists this is the song 
		/// that was being played when that playlist was playing.
		/// </summary>
		[Column( "SongIndex" )]
		public int DBSongIndex { get; set; }

		/// <summary>
		/// Change the name of this playlist
		/// </summary>
		/// <param name="newName"></param>
		public void Rename( string newName )
		{
			Name = newName;

			// Update the item in the model. No need to wait for this.
			_ = DbAccess.UpdateAsync( this );
		}

		/// <summary>
		/// Access the SongIndex
		/// </summary>
		[Ignore]
		public int SongIndex
		{
			get => DBSongIndex;

			set
			{
				DBSongIndex = value;

				// No need to wait for the update to complete
				_ = DbAccess.UpdateAsync( this );

				// Report this change
				NotificationHandler.NotifyPropertyChanged( this );
			}
		}

		/// <summary>
		/// Clear the contents of the playlist
		/// </summary>
		/// <param name="playlistToClear"></param>
		public void Clear()
		{
			DbAccess.DeleteItems( PlaylistItems );
			PlaylistItems.Clear();
		}

		/// <summary>
		/// Delete the specified PlayListItems from the Playlist
		/// </summary>
		/// <param name="items"></param>
		public void DeletePlaylistItems( List<PlaylistItem> items )
		{
			foreach ( PlaylistItem item in items )
			{
				_ = PlaylistItems.Remove( item );
			}

			DbAccess.DeleteItems( items );
		}

		/// <summary>
		/// Move a set of selected items down and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsDown( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			// Make sure that the index of the last item is valid
			int lastItemIndex = items.Last().Index;
			if ( lastItemIndex < PlaylistItems.Count - 1 )
			{
				PlaylistItem itemToMove = PlaylistItems[ lastItemIndex + 1 ];
				PlaylistItems.RemoveAt( lastItemIndex + 1 );
				PlaylistItems.Insert( items.First().Index, itemToMove );

				// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
				AdjustTrackNumbers();
			}
			else
			{

			}
		}

		/// <summary>
		/// Move a set of selected items up and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsUp( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry above those that are selected. That entry needs to be moved to below the end of the selection
			// Make sure that the index of the first item is valid
			int firstItemIndex = items.First().Index;
			if ( firstItemIndex > 0 )
			{
				PlaylistItem itemToMove = PlaylistItems[ firstItemIndex - 1 ];
				PlaylistItems.RemoveAt( firstItemIndex - 1 );
				PlaylistItems.Insert( items.Last().Index, itemToMove );

				// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
				AdjustTrackNumbers();
			}
			else
			{
			}
		}

		/// <summary>
		/// Adjust the track numbers to match the indexes in the collection
		/// </summary>
		/// <param name="thePlaylist"></param>
		public void AdjustTrackNumbers()
		{
			// The track numbers in the PlaylistItems must be updated to match their index in the collection
			for ( int index = 0; index < PlaylistItems.Count; ++index )
			{
				PlaylistItem itemToCheck = PlaylistItems[ index ];
				if ( itemToCheck.Index != index )
				{
					itemToCheck.Index = index;

					// Update the item in the model. No need to wait for this.
					_ = DbAccess.UpdateAsync( itemToCheck );
				}
			}
		}

		/// <summary>
		/// Check if the two song identities are for adjacent songs and the first entry is the current selected song
		/// If so then move the selected song index on one
		/// </summary>
		/// <param name="previousSongIdentity"></param>
		/// <param name="currentSongIdentity"></param>
		internal void CheckForAdjacentSongEntries( int previousSongIdentity, int currentSongIdentity )
		{
			if ( PlaylistItems.Count > 0 )
			{
				// If the current song is the first song in this playlist and the current song index is not defined
				// then set the index to the first song
				if ( ( SongIndex == -1 ) && ( IndexedSongIdentity( 0 ) == currentSongIdentity ) )
				{
					IncrementSongIndex();
				}
				else if ( IndexedSongIdentity() == previousSongIdentity )
				{
					if ( NextIndexedSongIdentity() == currentSongIdentity )
					{
						IncrementSongIndex();
					}
				}
			}
		}

		/// <summary>
		/// Called when a SongFinishedMessage has been received. If this is the currently indexed song, and its the last song in the playlist then
		/// reset the song index
		/// </summary>
		/// <param name="songId"></param>
		internal void SongFinished( int songId )
		{
			if ( ( IndexedSongIdentity() == songId ) && ( NextIndexedSongIdentity() == -1 ) )
			{
				SongIndex = -1;
			}
		}

		/// <summary>
		/// Is playback of this playlist in proress
		/// </summary>
		public bool InProgress => ( SongIndex > 0 );

		/// <summary>
		/// The Song last played (or started to be played) in this playlist
		/// </summary>
		public abstract Song InProgressSong { get; }

		/// <summary>
		/// The index of the last played song in the collection of all songs
		/// </summary>
		internal abstract int InProgressIndex { get; }

		/// <summary>
		/// Return a list of the songs in this playlist, optionally only the songs from the SongIndex onwards
		/// </summary>
		/// <param name="resume"></param>
		/// <returns></returns>
		public abstract List<Song> GetSongsForPlayback( bool resume );

		/// <summary>
		/// Add an item to the collection and storage.
		/// </summary>
		/// <param name="itemToAdd"></param>
		protected void AddItem( PlaylistItem itemToAdd )
		{
			PlaylistItems.Add( itemToAdd );
			DbAccess.Insert( itemToAdd );
		}

		/// <summary>
		/// Return the Song Id of the entry referenced by the SongIndex
		/// </summary>
		/// <returns></returns>
		protected abstract int IndexedSongIdentity( int songIndex = -1 );

		/// <summary>
		/// Return the Song Id of the entry referenced by the next SongIndex
		/// </summary>
		/// <returns></returns>
		protected abstract int NextIndexedSongIdentity();

		/// <summary>
		/// Set the SongIndex to point to the next song entry
		/// </summary>
		protected abstract void IncrementSongIndex();

		/// <summary>
		/// The PlaylistItem derived items associated with this playlist
		/// </summary>
		[Ignore]
		public List<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();

		/// <summary>
		/// The name given to the Now Playing playlist
		/// </summary>
		public const string NowPlayingPlaylistName = "Now Playing";
	}
}
