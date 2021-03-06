﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;

namespace DBTest
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
	public partial class Album
	{
		/// <summary>
		/// Get the songs associated with this Album
		/// </summary>
		public async Task GetSongsAsync()
		{
			if ( Songs == null )
			{
				Songs = await DbAccess.GetAlbumSongsAsync( Id );

				// Sort the songs by track number
				Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}
		}

		/// <summary>
		/// The Album's Played flag
		/// </summary>
		[Ignore]
		public bool Played
		{
			get => DBPlayed;
			set
			{
				DBPlayed = value;

				// No need to wait for the storage to complete
				DbAccess.UpdateAsync( this );

				// Report the change
				new AlbumPlayedStateChangedMessage() { AlbumChanged = this }.Send();
			}
		}

		[Ignore]
		public List<Song> Songs { get; set; } = null;
	}
}