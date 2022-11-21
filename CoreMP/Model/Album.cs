using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
	public partial class Album
	{
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

				// Report the change
				NotificationHandler.NotifyPropertyChanged( this );
//				new AlbumPlayedStateChangedMessage() { AlbumChanged = this }.Send();
			}
		}

		private List<Song> songs = null;

		[Ignore]
		public List<Song> Songs
		{
			get
			{
				if ( songs == null )
				{
					songs = CoreMP.Songs.GetAlbumSongs( Id );

					// Sort the songs by track number
					songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}

				return songs;
			}
		}
	}
}
