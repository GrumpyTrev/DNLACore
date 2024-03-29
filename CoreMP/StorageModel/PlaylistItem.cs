﻿using System.Linq;
using SQLite;

namespace CoreMP
{
	public class PlaylistItem
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		[Column( "Track" )]
		public int Index { get; set; }

		public int PlaylistId { get; set; }

		/// <summary>
		/// Get the parent playlist for a playlistitem
		/// </summary>
		/// <param name="playlistId"></param>
		/// <returns></returns>
		public Playlist GetParentPlaylist()
		{
			Playlist parentPlaylist = null;

			// Playlist ids are not unique as they are held in different tables, so we need to match the playlist type as well as its id
			if ( this is SongPlaylistItem )
			{
				parentPlaylist = Playlists.PlaylistCollection.Where( play => ( play.Id == PlaylistId ) && ( play is SongPlaylist ) ).FirstOrDefault();
			}
			else
			{
				parentPlaylist = Playlists.PlaylistCollection.Where( play => ( play.Id == PlaylistId ) && ( play is AlbumPlaylist ) ).FirstOrDefault();
			}

			return parentPlaylist;
		}
	}
}
