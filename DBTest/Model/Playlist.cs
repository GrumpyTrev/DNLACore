using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// Base class for playlists holding items derived from PlaylistItem
	/// </summary>
	public partial class Playlist
	{
		/// <summary>
		/// Change the name of this playlist
		/// </summary>
		/// <param name="newName"></param>
		public void Rename( string newName )
		{
			Name = newName;

			// Update the item in the model. No need to wait for this.
			DbAccess.UpdateAsync( this );
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
				DbAccess.UpdateAsync( this );
			}
		}

		/// <summary>
		/// Clear the contents of the playlist
		/// </summary>
		/// <param name="playlistToClear"></param>
		public void Clear()
		{
			DbAccess.DeleteItemsAsync( PlaylistItems );

			PlaylistItems.Clear();
		}

		/// <summary>
		/// Delete the specified PlayListItems from the SongPlaylist
		/// </summary>
		/// <param name="items"></param>
		public void DeletePlaylistItems( IEnumerable<PlaylistItem> items )
		{
			foreach ( PlaylistItem item in items )
			{
				PlaylistItems.Remove( item );
			}

			DbAccess.DeleteItemsAsync( items );
		}

		/// <summary>
		/// Move a set of selected items down and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsDown( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry beyond those that are selected. That entry needs to be moved to above the start of the selection
			PlaylistItem itemToMove = PlaylistItems[ items.Last().Index + 1 ];
			PlaylistItems.RemoveAt( items.Last().Index + 1 );
			PlaylistItems.Insert( items.First().Index, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			AdjustTrackNumbers();
		}

		/// <summary>
		/// Move a set of selected items up and update the track numbers
		/// </summary>
		/// <param name="items"></param>
		public void MoveItemsUp( IEnumerable<PlaylistItem> items )
		{
			// There must be at least one PlayListItem entry above those that are selected. That entry needs to be moved to below the end of the selection
			PlaylistItem itemToMove = PlaylistItems[ items.First().Index - 1 ];
			PlaylistItems.RemoveAt( items.First().Index - 1 );
			PlaylistItems.Insert( items.Last().Index, itemToMove );

			// Now the track numbers in the PlaylistItems must be updated to match their index in the collection
			AdjustTrackNumbers();
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
					DbAccess.UpdateAsync( itemToCheck );
				}
			}
		}

		/// <summary>
		/// Add an item to the collection and storage.
		/// </summary>
		/// <param name="itemToAdd"></param>
		protected void AddItem( PlaylistItem itemToAdd )
		{
			PlaylistItems.Add( itemToAdd );
			DbAccess.InsertAsync( itemToAdd );
		}

		/// <summary>
		/// The PlaylistItem derived items associated with this playlist
		/// </summary>
		[Ignore]
		public List<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
	}
}